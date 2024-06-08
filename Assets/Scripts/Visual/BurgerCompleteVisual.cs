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
        _disableAllVisuals();
        _plate.OnIngredientsChange += _displayIngredient;
    }

    private void OnDestroy()
    {
        _plate.OnIngredientsChange -= _displayIngredient;
    }

    private void _displayIngredient(List<KitchenItemSO> ingredients)
    {
        _disableAllVisuals();

        foreach (KitchenItemSO ingredient in ingredients)
        {
            IngredientKitchenItemSO item = _visuals.FirstOrDefault(v => v.Item == ingredient);
            item.Visual.SetActive(true);
        }
    }

    private void _disableAllVisuals()
    {
        foreach(IngredientKitchenItemSO item in _visuals)
        {
            item.Visual.SetActive(false);
        }
    }
}
