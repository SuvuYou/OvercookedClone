

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public float Balance;

    // coordicats of placed items are keys and integers are indecies in list of AvailablePurchasableItems 
    public Dictionary<Vector2Int, int> MapItems;

    public GameData()
    {
        Balance = 0f;
        MapItems = new();
    }
}

