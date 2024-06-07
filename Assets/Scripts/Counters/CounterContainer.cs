using System;
using UnityEngine;

public class CounterContainer : BaseCounter
{
    [SerializeField] private KitchenItemSO _kitchenItemToSpawn;
    public event Action OnContainerOpen;

    public override void Interact(KitchenItemParent player)
    {
        if (!player.IsHoldingItem())
        {
            player.SpawnKitchenItem(KitchenItemsList.Instance.GetIndexOfItem(_kitchenItemToSpawn));
            OnContainerOpen?.Invoke();
        }

        if (player.IsHoldingItem()) 
        {
            if (KitchenItemParent.TryAddIngredientToPlate(player, this))
            {
                OnContainerOpen?.Invoke();
            }
        };
    }
}
