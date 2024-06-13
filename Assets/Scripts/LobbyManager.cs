using System;
using System.Diagnostics;
using Unity.Netcode;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    public event Action OnConnectedPlayersCountChange;

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

    public bool IsPlayerIndexConnected(int index)
    {
        return _connectedPlayersData.Count > index;
    }

    public ulong GetClientIdByIndex(int index)
    {
        return _connectedPlayersData[index].ClientId;
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

    private void _addLobbyPlayerData(ulong clientId)
    {
        _connectedPlayersData.Add(new LobbyPlayerData(clientId));
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

    public LobbyPlayerData (ulong clientId)
    {
        ClientId = clientId;
    }

    public bool Equals(LobbyPlayerData other)
    {
        return ClientId == other.ClientId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
    }
}