using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AllowedIngridientsListSO", menuName = "ScriptableObjects/AllowedIngridientsListSO")]
public class AllowedIngridientsListSO : ScriptableObject
{
    public List<RecipeSO> AvailableRecipes; 
}
