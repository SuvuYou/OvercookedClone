using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AvailableRecipesListSO", menuName = "ScriptableObjects/AvailableRecipesListSO")]
public class AvailableRecipesListSO : ScriptableObject
{
    public List<RecipeSO> AvailableRecipes; 
}
