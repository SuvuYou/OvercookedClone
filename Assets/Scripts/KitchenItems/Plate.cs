using System;
using System.Collections.Generic;
using Unity.Netcode;
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
    
    public bool TryAddIngredientOnNetwork(KitchenItemSO ingredient)
    {
        if (_prohibitedIngredients.Contains(ingredient) || Ingredients.Contains(ingredient))
        {
            return false;
        }
        else
        {
            _addIngredientServerRpc(KitchenItemsList.Instance.GetIndexOfItem(ingredient));

            return true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void _addIngredientServerRpc(int kitchenItemIndex)
    {
        _addIngredientClientRpc(kitchenItemIndex);
    }

    [ClientRpc]
    private void _addIngredientClientRpc(int kitchenItemIndex)
    {
        _addIngredientByKitchenItemIndex(kitchenItemIndex);
    }

    private void _addIngredientByKitchenItemIndex(int kitchenItemIndex)
    {
        KitchenItemSO ingredient = KitchenItemsList.Instance.Items[kitchenItemIndex];
        SoundManager.SoundEvents.TriggerOnObjectPickupSound(transform.position);
        Ingredients.Add(ingredient);
        OnAddIngredient?.Invoke(ingredient);
    }

    public static bool IsIngridientAllowedOnPlate(KitchenItemSO ingredient)
    {
        return !ProhibitedIngredients.Contains(ingredient);
    }
}
