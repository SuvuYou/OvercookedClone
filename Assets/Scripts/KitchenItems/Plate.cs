using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Plate : KitchenItem
{
    public event Action<List<KitchenItemSO>> OnIngredientsChange;
    static private List<KitchenItemSO> AllowedIngredients = new ();

    [SerializeField] private List<KitchenItemSO> _allowedIngredients;
    public List<KitchenItemSO> Ingredients {get; private set;}
    
    private NetworkList<int> _ingredientsIndices;

    private void Awake()
    {
        if (AllowedIngredients.Count == 0)
        {
            AllowedIngredients = _allowedIngredients;
        }

        Ingredients = new();
        _ingredientsIndices = new NetworkList<int> ();
    }

    private void Update()
    {
        if (IsClient)
        {
            if (_ingredientsIndices.Count != Ingredients.Count)
            {
                Ingredients.Clear();

                foreach (int index in _ingredientsIndices)
                {
                    Ingredients.Add(AllowedIngredients[index]);  
                }

                OnIngredientsChange?.Invoke(Ingredients);
            }
        }
    }
    
    public bool TryAddIngredientOnNetwork(KitchenItemSO ingredient)
    {
        if (!AllowedIngredients.Contains(ingredient) || Ingredients.Contains(ingredient))
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
        KitchenItemSO ingredient = KitchenItemsList.Instance.Items[kitchenItemIndex];
        _ingredientsIndices.Add(AllowedIngredients.IndexOf(ingredient));
        _addIngredientClientRpc();
    }

    [ClientRpc]
    private void _addIngredientClientRpc()
    {
        _triggerOnAddIngredientEvents();
    }

    private void _triggerOnAddIngredientEvents()
    {
        SoundManager.SoundEvents.TriggerOnObjectPickupSound(transform.position);
    }

    public static bool IsIngridientAllowedOnPlate(KitchenItemSO ingredient)
    {
        return AllowedIngredients.Contains(ingredient);
    }
}
