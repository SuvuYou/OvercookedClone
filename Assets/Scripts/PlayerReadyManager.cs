using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class PlayerReadyManager : NetworkBehaviour
{
    public static PlayerReadyManager Instance;

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

    public void SetLocalPlayerReady()
    {
        _setLocalPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void _setLocalPlayerReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        _playersReadyStatus[rpcParams.Receive.SenderClientId] = true;

        if (_areAllPlayersReady())
        {
            SceneLoader.LoadSceneOnNetwork(Scene.Game);
        }
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
