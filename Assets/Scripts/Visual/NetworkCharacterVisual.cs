using Unity.Netcode;
using UnityEngine;

public class NetworkCharacterVisual : NetworkBehaviour
{
    [SerializeField] private LobbyCharacterVisual _visual;

    public override void OnNetworkSpawn()
    {
        _visual.AssignColor(LobbyManager.Instance.GetClientColorByIndex(LobbyManager.Instance.GetIndexByClientId(OwnerClientId)));
    }
}
