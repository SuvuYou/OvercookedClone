using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AvailablePurchasableItemsSO", menuName = "ScriptableObjects/AvailablePurchasableItemsSO")]
public class AvailablePurchasableItemsSO : ScriptableObject
{
    public List<PurchasableItemSO> AvailablePurchasableItems; 

    public PurchasableItemSO FindPurchasableItemByEditableItem(EditableItem editableItem)
    {
        foreach (var item in AvailablePurchasableItems)
        {
            if (item.IsMatchingItemPrefab(editableItem)) return item;
        }

        return null;
    }

    public int GetPurchasableItemIndex(PurchasableItemSO editableItem)
    {
        return AvailablePurchasableItems.IndexOf(editableItem);
    }
}
