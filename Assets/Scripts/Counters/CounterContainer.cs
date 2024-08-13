using System;
using Unity.Netcode;
using UnityEngine;

public class CounterContainer : BaseCounter
{
    [SerializeField] private KitchenItemSO _kitchenItemToSpawn;
    public event Action OnContainerOpen;

    public override void Interact(KitchenItemParent player)
    {
        if (!player.IsHoldingItem())
        {
            player.SpawnKitchenItem(_kitchenItemToSpawn);
            _triggerEventsLocallyAndOnNetwork();
        }

        if (player.IsHoldingItem()) 
        {
            if (KitchenItemParent.TryAddIngredientToPlateOwner(player, _kitchenItemToSpawn))
            {
                _triggerEventsLocallyAndOnNetwork();
            }
        };
    }

    private void _triggerEventsLocallyAndOnNetwork()
    {
        OnContainerOpen?.Invoke();
        _triggerEventOnNetworkServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void _triggerEventOnNetworkServerRpc(ServerRpcParams rpcParams = default)
    {
        _triggerEventOnNetworkClientRpc(sender: rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void _triggerEventOnNetworkClientRpc(ulong sender)
    {
        if (NetworkManager.Singleton.LocalClientId == sender) return;

        OnContainerOpen?.Invoke();
    }
}
