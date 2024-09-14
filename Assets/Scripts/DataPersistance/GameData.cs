

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public float Balance;
    public int CurrentDay;

    // coordicats of placed items are keys and integers are indecies in list of AvailablePurchasableItems 
    public Dictionary<Vector2Int, int> MapItems;

    public GameData(Dictionary<Vector2Int, int> defaultMapItems = default)
    {
        Balance = 0f;

        if (defaultMapItems != default)
            MapItems = new(defaultMapItems);
        else    
            MapItems = new();
    }
}