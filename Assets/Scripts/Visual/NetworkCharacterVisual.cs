using Unity.Netcode;
using UnityEngine;

public class NetworkCharacterVisual : NetworkBehaviour
{
    private const float FILLED_COLOR_ALPHA = 1f;
    private const float TRANSPARENT_COLOR_ALPHA = 0.6f;

    [SerializeField] private LobbyCharacterVisual _visual;

    public override void OnNetworkSpawn()
    {
        LobbyDataManager.Instance.OnConnectedPlayersDataChange += _updateColor;
        _updateColor();
    }

    private void _updateColor()
    {
        int index = LobbyDataManager.Instance.GetIndexByClientId(OwnerClientId);
        _visual.AssignColor(LobbyDataManager.Instance.GetPlayerDataByIndex(index).CharacterColor);
        _assignColorAlphaLocally(colorAlpha: FILLED_COLOR_ALPHA);
    }

    public void RenderFilled()
    {
       _assignColorAlpha(colorAlpha: FILLED_COLOR_ALPHA);
    }

    public void RenderTransparent()
    {
       _assignColorAlpha(colorAlpha: TRANSPARENT_COLOR_ALPHA);
    }

    private void _assignColorAlpha(float colorAlpha)
    {
        _assignColorAlphaServerRpc(colorAlpha);
        _assignColorAlphaLocally(colorAlpha);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _assignColorAlphaServerRpc(float colorAlpha, ServerRpcParams rpcParams = default)
    {
        _assignColorAlphaClientRpc(colorAlpha, senderClientId: rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void _assignColorAlphaClientRpc(float colorAlpha, ulong senderClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == senderClientId) return;

        _assignColorAlphaLocally(colorAlpha);
    }

    private void _assignColorAlphaLocally(float colorAlpha)
    {
        _visual.AssignColorAlpha(newAlpha: colorAlpha);
    }
}
