using System;
using UnityEngine;

public class KitchenItemParent : MonoBehaviour
{
    [SerializeField] private Transform _itemSpawnPlaceholder;

    public event Action OnItemPickup;
    public void TriggerOnItemPickup () => OnItemPickup?.Invoke();

    public event Action OnItemDrop;
    public void TriggerOnItemDrop () => OnItemDrop?.Invoke();

    private KitchenItem _currentItemHeld;

    public KitchenItem GetCurrentItemHeld() => _currentItemHeld;
    public bool IsHoldingItem() => _currentItemHeld != null;

    public void SetCurrentItemHeld(KitchenItem newItem) 
    {
        _currentItemHeld = newItem;
        
        if (newItem != null)
        {
            _currentItemHeld.transform.parent = _itemSpawnPlaceholder;
            _currentItemHeld.transform.position = _itemSpawnPlaceholder.position;

            TriggerOnItemPickup();
        }
        else
        {
            TriggerOnItemDrop();
        }
    } 

    public void DestroyCurrentItemHeld()
    {
        if (_currentItemHeld == null) return;

        Destroy(_currentItemHeld.gameObject);
        SetCurrentItemHeld(null);
    }

    public static void SwapItemsOfTwoOwners(KitchenItemParent parent1, KitchenItemParent parent2)
    {  
        KitchenItem tempItem = parent1.GetCurrentItemHeld();
        parent1.SetCurrentItemHeld(parent2.GetCurrentItemHeld());
        parent2.SetCurrentItemHeld(tempItem);
    }

    public static bool TryAddIngredientToPlate(KitchenItemParent parent1, KitchenItemParent parent2)
    {
        if (!parent1.IsHoldingItem() || !parent2.IsHoldingItem()) return false;

        return (
            _tryAddIngredientToPlateOwner(plateOwner: parent1, ingredientOwner: parent2) || 
            _tryAddIngredientToPlateOwner(plateOwner: parent2, ingredientOwner: parent1)
        );
    }   
    
    private static bool _tryAddIngredientToPlateOwner(KitchenItemParent plateOwner, KitchenItemParent ingredientOwner)
    {
        if (plateOwner.GetCurrentItemHeld().TryGetPlateComponent(out Plate plate)) 
        {
            if (plate.TryAddIngredient(ingredientOwner.GetCurrentItemHeld().GetItemReference()))
            {
                ingredientOwner.DestroyCurrentItemHeld();
                plateOwner.TriggerOnItemPickup();

                return true;
            }
        }
        
        return false;
    }
}
