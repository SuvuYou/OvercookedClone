using UnityEngine;

[CreateAssetMenu(fileName = "PurchasableItemSO", menuName = "ScriptableObjects/PurchasableItemSO")]
public class PurchasableItemSO : ScriptableObject
{
    public EditableItem ItemPrefab; 
    public Sprite ShopSpriteImage;
    public int Price;

    public bool IsMatchingItemPrefab(EditableItem item)
    {
        if (item == null)
        {
            return false;
        }

        return item.PurchasableItemReference.ItemPrefab == ItemPrefab;
    }
}
