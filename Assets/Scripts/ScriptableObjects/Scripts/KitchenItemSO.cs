using UnityEngine;

[CreateAssetMenu(fileName = "KitchenItemSO", menuName = "ScriptableObjects/KitchenItemSO")]
public class KitchenItemSO : ScriptableObject
{
    public KitchenItem Prefab;
    public KitchenItemVisual VisualFakeItem;
    public Sprite IconSprite;
    public string ItemName;

    public SliceableFoodSO SliceableSO;
    public FryableFoodSO FryableSO;

    public bool IsSliceable()
    {
        return SliceableSO != null;
    }

    public bool IsFryable()
    {
        return FryableSO != null;
    }
}
