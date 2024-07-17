using Unity.Netcode;
using UnityEngine;

public class NetworkCharacterVisual : NetworkBehaviour
{
    [SerializeField] private LobbyCharacterVisual _visual;

    public override void OnNetworkSpawn()
    {
        LobbyManager.Instance.OnConnectedPlayersCountChange += _updateColor;
        _updateColor();
    }

    private void _updateColor ()
    {
        _visual.AssignColor(LobbyManager.Instance.GetClientColorByIndex(LobbyManager.Instance.GetIndexByClientId(OwnerClientId)));
    }
}
