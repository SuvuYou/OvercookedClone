using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeSO", menuName = "ScriptableObjects/RecipeSO")]
public class RecipeSO : ScriptableObject
{
    public List<KitchenItemSO> Ingredients; 
    public List<PurchasableItemSO> RequiredMapItems; 
    public string RecipeName;
    public int Price;
    public float EatingTime;
}
