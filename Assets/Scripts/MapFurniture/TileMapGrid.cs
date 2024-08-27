using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TileMapGrid : NetworkBehaviour
{
    public static TileMapGrid Instance;

    [SerializeField] private Vector2Int _size;
    [SerializeField] private GridTile _tilePrefab;
    [SerializeField] private SelectedEditableItemSO _selectedEditableItem;

    private Dictionary<Vector2Int, bool> _availableTilesGrid = new();

    private List<GridTile> _tiles = new();
    private List<Vector2Int> _coordinats = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);

            return;
        }
 
        Instance = this;

        _initTileCoordinats();
        _assignTiles();
    }

    private void Start()
    {
        _selectedEditableItem.OnStartEditing += () => _setIndicators(true);
        _selectedEditableItem.OnEndEditing += () => _setIndicators(false);
    }

    public bool IsPlaceAvailable(Vector2Int coords) => _availableTilesGrid[coords];

    public void TakeTile(Vector2Int coords) => _setTileAvailability(coords, availability: false);
       
    public void FreeTile(Vector2Int coords) => _setTileAvailability(coords, availability: true);

    private void _setTileAvailability(Vector2Int coords, bool availability)
    {
        _setTileAvailabilityServerRpc(coords.x, coords.y, availability);
        _setTileAvailabilityLocally(coords, availability);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _setTileAvailabilityServerRpc(int x, int y, bool availability, ServerRpcParams rpcParams = default)
    {
        _setTileAvailabilityClientRpc(x, y, availability, senderClientId: rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void _setTileAvailabilityClientRpc(int x, int y, bool availability, ulong senderClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == senderClientId) return;

        _setTileAvailabilityLocally(coords: new (x, y), availability: availability);
    }

    private void _setTileAvailabilityLocally(Vector2Int coords, bool availability)
    {
        _availableTilesGrid[coords] = availability;
    }

    private void _setIndicators(bool isVisible) => _iterateTiles((GridTile tile) => tile.SetIsIndicatorVisible(isVisible));

    private void _iterateTiles(Action<GridTile> func)
    {
        foreach(var tile in _tiles)
        {
            func(tile);
        }
    } 
    
    private void _assignTiles()
    {
        int childIndex = 0;
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out GridTile tile))
            {
                var coords = _coordinats[childIndex];

                tile.InitCoordinats(coords);
                _tiles.Add(tile);
                _availableTilesGrid[coords] = true;
                
                childIndex++;
            }
        }
    }

    private void _initTileCoordinats()
    {
        for (int i = 0; i < _size.x; i++)
        {
            for (int j = 0; j < _size.y; j++)
            {
                _coordinats.Add(new (i, j));
            }
        }
    }
}    