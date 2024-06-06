using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
struct IngredientKitchenItemSO {
    public KitchenItemSO Item;
    public GameObject Visual;
}

public class BurgerCompleteVisual : MonoBehaviour
{
    [SerializeField] Plate _plate;
    [SerializeField] List<IngredientKitchenItemSO> _visuals;

    private void Awake()
    {
        _initialDisableAllVisuals();
        _plate.OnAddIngredient += _displayIngredient;
    }

    private void OnDestroy()
    {
        _plate.OnAddIngredient -= _displayIngredient;
    }

    private void _displayIngredient(KitchenItemSO ingredient)
    {
        IngredientKitchenItemSO item = _visuals.FirstOrDefault(v => v.Item == ingredient);
        item.Visual.SetActive(true);
    }

    private void _initialDisableAllVisuals()
    {
        foreach(IngredientKitchenItemSO item in _visuals)
        {
            item.Visual.SetActive(false);
        }
    }
}
