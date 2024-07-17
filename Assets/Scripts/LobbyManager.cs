using System;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

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

        Instance = this;

        _connectedPlayersData = new();
        _connectedPlayersData.OnListChanged += (NetworkListEvent<LobbyPlayerData> eventArgs) => OnConnectedPlayersCountChange?.Invoke();

        DontDestroyOnLoad(this.gameObject);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += _addLobbyPlayerData;
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
        LobbyPlayerData player = _connectedPlayersData[playerIndex];
        player.CharacterColor = newColor;
        _connectedPlayersData[playerIndex] = player;

        _triggerPlayerColorChangeClientRpc(playerIndex, newColor);
    }

    [ClientRpc]
    private void _triggerPlayerColorChangeClientRpc(int playerIndex, Color newColor)
    {
        OnPlayerColorChange?.Invoke(playerIndex, newColor);
    }

    private void _addLobbyPlayerData(ulong clientId)
    {
        LobbyPlayerData data = new (clientId, color: _getFirstAvailibleColor());
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

    public void ShutLobbyDown()
    {
        _disposeEvents();
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
    public Color CharacterColor;

    public LobbyPlayerData (ulong clientId, Color color)
    {
        ClientId = clientId;
        CharacterColor = color;
    }

    public bool Equals(LobbyPlayerData other)
    {
        return ClientId == other.ClientId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterColor);
    }
}