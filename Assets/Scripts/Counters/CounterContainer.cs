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
            _triggerEventOnNetworkServerRpc();
        }

        if (player.IsHoldingItem()) 
        {
            if (KitchenItemParent.TryAddIngredientToPlateOwner(player, _kitchenItemToSpawn))
            {
                _triggerEventOnNetworkServerRpc();
            }
        };
    }

    [ServerRpc(RequireOwnership = false)]
    private void _triggerEventOnNetworkServerRpc()
    {
        _triggerEventOnNetworkClientRpc();
    }

    [ClientRpc]
    private void _triggerEventOnNetworkClientRpc()
    {
        OnContainerOpen?.Invoke();
    }
}
