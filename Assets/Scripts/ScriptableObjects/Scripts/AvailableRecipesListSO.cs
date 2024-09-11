using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AvailableRecipesListSO", menuName = "ScriptableObjects/AvailableRecipesListSO")]
public class AvailableRecipesListSO : ScriptableObject
{
    public List<RecipeSO> AllAvailableRecipes; 

    public List<RecipeSO> GetAvailableRecipes(List<PurchasableItemSO> placedMapItems)
    {
        List<RecipeSO> list = new();

        foreach (var recipe in AllAvailableRecipes) 
        {
            if (recipe.RequiredMapItems.All(requiredItem => placedMapItems.Contains(requiredItem)))
            {
                list.Add(recipe);
            }
        }

        return list;
    }
}