using System;
using System.Collections.Generic;

public class PlateFakeVisual : KitchenItemFakeVisual, IPlate
{
    public event Action<List<KitchenItemSO>> OnIngredientsChange;
    public event Action OnDeliverPlate;
    private List<KitchenItemSO> _ingredients = new();

    public void AddIngredients(List<KitchenItemSO> ingredients)
    {
        foreach(KitchenItemSO ingredient in ingredients)
        {
            _ingredients.Add(ingredient);
        }
        
        OnIngredientsChange?.Invoke(_ingredients);
    }

    public void DeliverPlate()
    {
        OnDeliverPlate?.Invoke();
    }
}
