using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    private const string PLAYER_NAME_PLAYER_PREF = "PlayerNameMultiplayer";
    private string _playerName;

    [SerializeField] private ColorPickersSO _colorPickers;

    public event Action OnConnectedPlayersCountChange;
    public event Action<int, Color> OnPlayerColorChange;
    public event Action OnTryingToJoin;
    public event Action OnFailedToJoin;

    private NetworkList<LobbyPlayerData> _connectedPlayersData;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);

            return;
        }

        _playerName = PlayerPrefs.GetString(PLAYER_NAME_PLAYER_PREF, "Player Name");

        Instance = this;

        _connectedPlayersData = new();
        _connectedPlayersData.OnListChanged += (NetworkListEvent<LobbyPlayerData> eventArgs) => OnConnectedPlayersCountChange?.Invoke();

        DontDestroyOnLoad(this.gameObject);
    }

    public string GetPlayerName () => _playerName;
    public void SetPlayerName (string name)
    {   
        _playerName = name;
        PlayerPrefs.SetString(PLAYER_NAME_PLAYER_PREF, name);
    } 
    
    public void StartHost()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += _addLobbyPlayerData;
        NetworkManager.Singleton.OnClientConnectedCallback += _setPlayerNameServerRpc;
        NetworkManager.Singleton.OnClientDisconnectCallback += _removeLobbyPlayerData;

        NetworkManager.Singleton.StartHost();
        SceneLoader.LoadSceneOnNetwork(Scene.PlayerSelectScene);
    }

    public void StartClient()
    {
        OnTryingToJoin?.Invoke();

        NetworkManager.Singleton.OnClientDisconnectCallback += _triggerOnFailedToJoin;
        NetworkManager.Singleton.StartClient();
    }

    private void _triggerOnFailedToJoin(ulong clientId)
    {
        OnFailedToJoin?.Invoke();
    }

    public int GetConnectedPlayersCount()
    {
        return _connectedPlayersData.Count;
    }

    public ulong GetClientIdByIndex(int index)
    {
        return _connectedPlayersData[index].ClientId;
    }

    public int GetIndexByClientId(ulong clientId)
    {
        for (int i = 0; i < _connectedPlayersData.Count; i++)
            if (_connectedPlayersData[i].ClientId == clientId) return i;

        return 0;    
    }

    public Color GetClientColorByIndex(int index)
    {
        return _connectedPlayersData[index].CharacterColor;
    }

    public void SetPlayerColor(int playerIndex, Color newColor)
    {
        _triggerPlayerColorChangeServerRpc(playerIndex, newColor);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _triggerPlayerColorChangeServerRpc(int playerIndex, Color newColor)
    {
        LobbyPlayerData data = _connectedPlayersData[playerIndex];
        data.CharacterColor = newColor;
        _connectedPlayersData[playerIndex] = data;

        _triggerPlayerColorChangeClientRpc(playerIndex, newColor);
    }

    [ServerRpc]
    private void _setPlayerNameServerRpc(ulong clientId)
    {
        _setPlayerNameClientRpc();
    }

    [ClientRpc]
    private void _setPlayerNameClientRpc()
    {
        int index = GetIndexByClientId(OwnerClientId);

        LobbyPlayerData data = _connectedPlayersData[index];
        data.PlayerName = GetPlayerName();
        _connectedPlayersData[index] = data;
    }

    [ClientRpc]
    private void _triggerPlayerColorChangeClientRpc(int playerIndex, Color newColor)
    {
        OnPlayerColorChange?.Invoke(playerIndex, newColor);
    }

    private void _addLobbyPlayerData(ulong clientId)
    {
        LobbyPlayerData data = new (clientId, color: _getFirstAvailibleColor(), playerName: "");
        _connectedPlayersData.Add(data);
    }

    private Color _getFirstAvailibleColor()
    {
        foreach (Color color in _colorPickers.GetColorsList())
        {
            if (IsColorAvailible(color))
            {
                return color;
            }
        }

        return _colorPickers.GetColorsList()[0];
    }

    public bool IsColorAvailible(Color color)
    {
        foreach (LobbyPlayerData playerData in _connectedPlayersData)
        {
            if (playerData.CharacterColor.Equals(color))
            {
                return false;
            }
        }

        return true;
    }

    private void _removeLobbyPlayerData(ulong clientId)
    {
        foreach (LobbyPlayerData playerData in _connectedPlayersData)
        {
            if (playerData.ClientId == clientId)
            {
                _connectedPlayersData.Remove(playerData);

                return;
            }
        }
    }

    public void ShutNetworkManagerDown()
    {
        _disposeEvents();
        LobbiesListManager.Instance.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
    } 

    private void _disposeEvents()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= _addLobbyPlayerData;
        NetworkManager.Singleton.OnClientDisconnectCallback -= _triggerOnFailedToJoin;
        NetworkManager.Singleton.OnClientDisconnectCallback -= _removeLobbyPlayerData;
    }

    public override void OnNetworkDespawn()
    {
        _disposeEvents();
    }
}

struct LobbyPlayerData : IEquatable<LobbyPlayerData>, INetworkSerializable
{
    public ulong ClientId;
    public FixedString64Bytes PlayerName;
    public Color CharacterColor;

    public LobbyPlayerData (ulong clientId, Color color, string playerName)
    {
        ClientId = clientId;
        CharacterColor = color;
        PlayerName = playerName;
    }

    public bool Equals(LobbyPlayerData other)
    {
        return ClientId == other.ClientId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterColor);
        serializer.SerializeValue(ref PlayerName);
    }
}