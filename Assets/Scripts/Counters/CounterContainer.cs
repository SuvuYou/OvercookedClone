using System;
using UnityEngine;

public class CounterContainer : BaseCounter
{
    [SerializeField] private KitchenItemSO _kitchenItemToSpawn;
    public event Action OnContainerOpen;

    public override void Interact(KitchenItemParent player)
    {
        SetCurrentItemHeld(Instantiate(_kitchenItemToSpawn.Prefab, Vector3.zero, Quaternion.identity));

        if (player.IsHoldingItem()) 
        {
            if (!KitchenItemParent.TryAddIngredientToPlate(player, this))
            {
                DestroyCurrentItemHeld();
            }
            else
            {
                OnContainerOpen?.Invoke();
            }

            return;
        };
        
        KitchenItemParent.SwapItemsOfTwoOwners(player, this);

        OnContainerOpen?.Invoke();
    }
}
