using UnityEngine;

public class KitchenItem : MonoBehaviour
{
    [SerializeField] KitchenItemSO _itemReference;

    public KitchenItemSO GetItemReference()
    {
        return _itemReference;
    }

    public bool TryGetPlateComponent(out Plate plate)
    {
        if (this is Plate)
        {
            plate = this as Plate;

            return true;
        }
        else
        {
            plate = null;
            
            return false;
        }
    }
}
