using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class PlayerReadyManager : NetworkBehaviour
{
    public static PlayerReadyManager Instance;

    public event Action OnPlayerReady;

    private Dictionary<ulong, bool> _playersReadyStatus = new();

    private void Awake ()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);

            return;
        }

        Instance = this;
    }

    public void ToggleLocalPlayerReady()
    {
        _toggleLocalPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void _toggleLocalPlayerReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        bool prevStatus = IsPlayerReady(clientId);
        _updatePlayersReadyStatusClientRpc(clientId, status: !prevStatus);
        _triggerOnPlayerReadyEventClientRpc();

        if (_areAllPlayersReady())
        {
            SceneLoader.LoadSceneOnNetwork(Scene.Game);
        }
    }

    [ClientRpc]
    private void _updatePlayersReadyStatusClientRpc(ulong clientId, bool status)
    {
        _playersReadyStatus[clientId] = status;
    }

    [ClientRpc]
    private void _triggerOnPlayerReadyEventClientRpc()
    {
        OnPlayerReady?.Invoke();
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return _playersReadyStatus.Keys.Contains(clientId) && _playersReadyStatus[clientId];
    }

    private bool _areAllPlayersReady()
    {
        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            if (!_playersReadyStatus.Keys.Contains(clientId) || !_playersReadyStatus[clientId])
            {
                return false;
            }
        }

        return true;
    }
}
