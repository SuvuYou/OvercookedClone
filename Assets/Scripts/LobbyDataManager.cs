using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LobbyDataManager : NetworkBehaviour
{
    public static LobbyDataManager Instance;

    private const string PLAYER_NAME_PLAYER_PREF = "PlayerNameMultiplayer";

    private string _localPlayerName;
    private int _playersJoinedCount;

    [SerializeField]
    private ColorPickersSO _colorPickers;

    public event Action OnConnectedPlayersDataChange;
    public event Action<int, Color> OnPlayerColorChange;

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
        _connectedPlayersData.OnListChanged += (NetworkListEvent<LobbyPlayerData> eventArgs) => OnConnectedPlayersDataChange?.Invoke();

        _localPlayerName = PlayerPrefs.GetString(PLAYER_NAME_PLAYER_PREF, "Player Name");

        DontDestroyOnLoad(this.gameObject);
    }


    public string GetLocalPlayerName () => _localPlayerName;
    public void SetPlayerName (string name)
    {   
        _localPlayerName = name;
        PlayerPrefs.SetString(PLAYER_NAME_PLAYER_PREF, name);
    } 
    
    public int GetConnectedPlayersCount()
    {
        return _connectedPlayersData.Count;
    }

    public int GetIndexByClientId(ulong clientId)
    {
        for (int i = 0; i < _connectedPlayersData.Count; i++)
        {
            if (_connectedPlayersData[i].ClientId == clientId) 
            {
                return i;
            }
        }
            
        return 0;    
    }

    public LobbyPlayerData GetPlayerDataByIndex(int index)
    {
        return _connectedPlayersData[index];
    }

    public void SetPlayerColor(int playerIndex, Color newColor)
    {
        _triggerPlayerColorChangeServerRpc(playerIndex, newColor);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _triggerPlayerColorChangeServerRpc(int playerIndex, Color newColor)
    {
        _setPlayerColorServerRpc(playerIndex, newColor);
        _triggerPlayerColorChangeClientRpc(playerIndex, newColor);
    }

    [ClientRpc]
    private void _initializeJoinedClientPlayerNameClientRpc(int playerIndex, ulong joinedClientId)
    {
        if (joinedClientId != NetworkManager.Singleton.LocalClientId) return;
    
        string playerName = GetLocalPlayerName();

        _setPlayerNameServerRpc(playerIndex, playerName); 
    }

    [ServerRpc(RequireOwnership = false)]
    private void _setPlayerColorServerRpc(int playerIndex, Color newColor)
    {
        LobbyPlayerData data = _connectedPlayersData[playerIndex];
        data.CharacterColor = newColor;
        _connectedPlayersData[playerIndex] = data;
    }

    [ServerRpc(RequireOwnership = false)]
    private void _setPlayerNameServerRpc(int playerIndex, FixedString64Bytes playerName)
    {
        LobbyPlayerData data = _connectedPlayersData[playerIndex];
        data.PlayerName = playerName;
        _connectedPlayersData[playerIndex] = data;
    }

    [ClientRpc]
    private void _triggerPlayerColorChangeClientRpc(int playerIndex, Color newColor)
    {
        OnPlayerColorChange?.Invoke(playerIndex, newColor);
    }

    public void AddLobbyPlayerData(ulong clientId)
    {
        _playersJoinedCount++;
        LobbyPlayerData data = new (clientId, color: _getFirstAvailibleColor(), playerName: "");
        _connectedPlayersData.Add(data);

        _initializeJoinedClientPlayerNameClientRpc(playerIndex: _playersJoinedCount - 1, joinedClientId: clientId);
    }

    public void RemoveLobbyPlayerData(ulong clientId)
    {
        _playersJoinedCount--;

        foreach (LobbyPlayerData playerData in _connectedPlayersData)
        {
            if (playerData.ClientId == clientId)
            {  
                _connectedPlayersData.Remove(playerData);

                return;
            }
        }
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
}

public struct LobbyPlayerData : IEquatable<LobbyPlayerData>, INetworkSerializable
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

    public string GetPlayerName() => PlayerName.ToString();

    public readonly bool Equals(LobbyPlayerData other)
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