using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileMapGridData
{
    public Dictionary<Vector2Int, bool> AvailableTilesGrid { get; private set; } =  new();
    public Dictionary<Vector2Int, GridTile> TilesByCoordinats { get; private set; } =  new();
    public Dictionary<Vector2Int, EditableItem> PlacedItemsByCoordinats { get; private set; } = new();

    public bool IsTileAvailable(Vector2Int coords) => AvailableTilesGrid[coords];

    public void SetIndicators(bool isVisible) => IterateTiles((GridTile tile) => tile.SetIsIndicatorVisible(isVisible));

    public void IterateTiles(Action<GridTile> func)
    {
        foreach(var tile in TilesByCoordinats.Values) func(tile);
    }
    
    public void ClearMap()
    {
        foreach (var tile in AvailableTilesGrid.Keys.ToList()) AvailableTilesGrid[tile] = true;
        PlacedItemsByCoordinats.Clear();
    }
    
    public void InitTiles(Transform tilesHolderTransform, Vector2Int size)
    {
        int childIndex = 0;
        foreach (Transform child in tilesHolderTransform)
        {
            if (child.TryGetComponent(out GridTile tile))
            {
                int x = childIndex / size.y;
                int y = childIndex % size.y;
                var coords = new Vector2Int(x, y);

                tile.InitCoordinats(coords);
                TilesByCoordinats[coords] = tile;
                AvailableTilesGrid[coords] = true;
                
                childIndex++;
            }
        }
    }

    public void SetLocalTileData(Vector2Int coords, EditableItem editableItem)
    {
        if (editableItem == null)
        {
            PlacedItemsByCoordinats.Remove(coords); 
            AvailableTilesGrid[coords] = true;
        }
        else
        {
            PlacedItemsByCoordinats[coords] = editableItem;
            AvailableTilesGrid[coords] = false;
        }
    }
}    