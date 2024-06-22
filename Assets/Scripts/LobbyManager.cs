using System;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    [SerializeField] private ColorPickersSO _colorPickers;

    public event Action OnConnectedPlayersCountChange;
    public event Action<int, Color> OnPlayerColorChange;

    private NetworkList<LobbyPlayerData> _connectedPlayersData;

    private void Awake ()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);

            return;
        }

        _connectedPlayersData = new();
        Instance = this;

        _connectedPlayersData.OnListChanged += (NetworkListEvent<LobbyPlayerData> eventArgs) => OnConnectedPlayersCountChange?.Invoke();
    }

    public int GetConnectedPlayersCount()
    {
        return _connectedPlayersData.Count;
    }

    public ulong GetClientIdByIndex(int index)
    {
        return _connectedPlayersData[index].ClientId;
    }

    public Color GetClientColorByIndex(int index)
    {
        return _connectedPlayersData[index].CharacterColor;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            // add host
            _addLobbyPlayerData(clientId: 0);

            NetworkManager.Singleton.OnClientConnectedCallback += _addLobbyPlayerData;
            NetworkManager.Singleton.OnClientDisconnectCallback += _removeLobbyPlayerData;
        }
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
        _connectedPlayersData.Add(new LobbyPlayerData(clientId, color: _getFirstAvailibleColor()));
    }

    private Color _getFirstAvailibleColor()
    {
        foreach (Color color in _colorPickers.Colors)
        {
            if (IsColorAvailible(color))
            {
                return color;
            }
        }

        return _colorPickers.Colors[0];
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