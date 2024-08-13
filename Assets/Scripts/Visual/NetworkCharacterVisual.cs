using Unity.Netcode;
using UnityEngine;

public class NetworkCharacterVisual : NetworkBehaviour
{
    [SerializeField] private LobbyCharacterVisual _visual;

    public override void OnNetworkSpawn()
    {
        LobbyDataManager.Instance.OnConnectedPlayersDataChange += _updateColor;
        _updateColor();
    }

    private void _updateColor ()
    {
        int index = LobbyDataManager.Instance.GetIndexByClientId(OwnerClientId);
        _visual.AssignColor(LobbyDataManager.Instance.GetPlayerDataByIndex(index).CharacterColor);
    }
}
