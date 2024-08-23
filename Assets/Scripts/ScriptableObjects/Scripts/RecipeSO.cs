using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeSO", menuName = "ScriptableObjects/RecipeSO")]
public class RecipeSO : ScriptableObject
{
    public List<KitchenItemSO> Ingredients; 
    public string RecipeName;
    public int Price;
    public float EatingTime;
}
