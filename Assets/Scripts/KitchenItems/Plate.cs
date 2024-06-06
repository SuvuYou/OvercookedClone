using System;
using System.Collections.Generic;
using UnityEngine;

public class Plate : KitchenItem
{
    public event Action<KitchenItemSO> OnAddIngredient;
    static private List<KitchenItemSO> ProhibitedIngredients = new ();

    [SerializeField] private List<KitchenItemSO> _prohibitedIngredients;
    public List<KitchenItemSO> Ingredients {get; private set;} = new();

    private void Awake()
    {
        if (ProhibitedIngredients.Count == 0)
        {
            ProhibitedIngredients = _prohibitedIngredients;
        }
    }

    public bool TryAddIngredient(KitchenItemSO ingredient)
    {
        if (_prohibitedIngredients.Contains(ingredient) || Ingredients.Contains(ingredient))
        {
            return false;
        }
        else
        {
            SoundManager.SoundEvents.TriggerOnObjectPickupSound(transform.position);
            Ingredients.Add(ingredient);
            OnAddIngredient?.Invoke(ingredient);

            return true;
        }
    }

    public static bool IsIngridientAllowedOnPlate(KitchenItemSO ingredient)
    {
        return !ProhibitedIngredients.Contains(ingredient);
    }
}
