using System;
using System.Collections.Generic;
using Unity.Netcode;

public class Plate : KitchenItem
{
    public event Action<List<KitchenItemSO>> OnIngredientsChange;
    static private List<KitchenItemSO> AllowedIngredients;
    public List<KitchenItemSO> Ingredients {get; private set;}
    
    private NetworkList<int> _ingredientsIndices;

    private void Awake()
    {
        Ingredients = new();
        _ingredientsIndices = new NetworkList<int> ();
    }

    public static void InitAllowedIngridients(List<KitchenItemSO> allowedIngredients)
    {
        AllowedIngredients = allowedIngredients;
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
    }

    public static bool IsIngridientAllowedOnPlate(KitchenItemSO ingredient)
    {
        return AllowedIngredients.Contains(ingredient);
    }
}
