using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class TileMapGrid : NetworkBehaviour
{
    public static TileMapGrid Instance;

    [SerializeField] private Vector2Int _size;
    [SerializeField] private SelectedObjectsInRangeSO _selectedObjectsInRange;
    [SerializeField] private AvailablePurchasableItemsSO _availablePurchasableItems;

    private TileMapGridData _gridData = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);

            return;
        }
 
        Instance = this;

        _gridData.InitTiles(tilesHolderTransform: transform, size: _size);
    }
    
    private void Start()
    {
        _selectedObjectsInRange.OnStartEditing += () => _gridData.SetIndicators(isVisible: true);
        _selectedObjectsInRange.OnEndEditing += () => _gridData.SetIndicators(isVisible: false);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        
        DataPersistanceManager.Instance.OnLoadGameData += _loadMapFromSaveFile;
        DataPersistanceManager.Instance.OnSaveGameData += _saveMapToSaveFile;
    }

    private void _loadMapFromSaveFile(GameData data) 
    {
        foreach (var item in _gridData.PlacedItemsByCoordinats.Values.ToList()) Destroy(item.gameObject); 

        _gridData.ClearMap();

        foreach (var item in data.MapItems)
        {
            var prefab = _availablePurchasableItems.AvailablePurchasableItems[item.Value].ItemPrefab;
            var createdItem = SpawnMapItem(prefab, _gridData.TilesByCoordinats[item.Key]);
            createdItem.AssignCoordinats(item.Key);
        }
    }

    private void _saveMapToSaveFile(GameData data, Action<GameData> saveData) 
    {
        Dictionary<Vector2Int, int> placedItemsIndeciesByCoordinats = new();

        foreach (var coords in _gridData.PlacedItemsByCoordinats)
        {
            placedItemsIndeciesByCoordinats[coords.Key] = _availablePurchasableItems.GetIndexByEditableItem(coords.Value);
        }

        data.MapItems = placedItemsIndeciesByCoordinats;
        saveData(data);
    }

    public EditableItem SpawnMapItem(EditableItem itemToSpawn, GridTile tile)
    {
        if (!IsServer) return null;

        var createdItem = Instantiate(itemToSpawn, tile.GetPlacePosition(), Quaternion.Euler(0, 180, 0));
        var networkObject = createdItem.GetComponent<NetworkObject>();
        networkObject.Spawn();

        return createdItem;
    }

    public void AddItemToGrid(Vector2Int coords, EditableItem item)
    {
        if (!_gridData.TilesByCoordinats[coords].IsAvailable()) return;

        SetTileData(coords, item);
    }
    
    public void RemoveItemFormGrid(Vector2Int coords) => SetTileData(coords, editableItem: null);
    
    public bool IsTileAvailable(Vector2Int coords) => _gridData.IsTileAvailable(coords);

    public GridTile GetTileByCoordinats(Vector2Int coords) => _gridData.TilesByCoordinats[coords];

    public List<EditableItem> GetAllPlacedItems() => _gridData.PlacedItemsByCoordinats.Values.ToList();
    
    public void SetTileData(Vector2Int coords, EditableItem editableItem)
    {
        if (editableItem && editableItem.TryGetComponent(out NetworkObject netObj))
        {
            _setTileDataServerRpc(coords.x, coords.y, netObjRef: netObj);
        }
        else
        {
            _setTileDataServerRpc(coords.x, coords.y);
        }
        
        _setTileDataLocally(coords, editableItem);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _setTileDataServerRpc(int x, int y, NetworkObjectReference netObjRef, ServerRpcParams rpcParams = default)
    {
        _setTileDataClientRpc(x, y, netObjRef, senderClientId: rpcParams.Receive.SenderClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _setTileDataServerRpc(int x, int y, ServerRpcParams rpcParams = default)
    {
        _setTileDataClientRpc(x, y, senderClientId: rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void _setTileDataClientRpc(int x, int y, NetworkObjectReference netObjRef, ulong senderClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == senderClientId) return;

        if (netObjRef.TryGet(out NetworkObject netObj) && netObj.TryGetComponent(out EditableItem editableItem))
        {
            _setTileDataLocally(coords: new (x, y), editableItem: editableItem);
        }
    }

    [ClientRpc]
    private void _setTileDataClientRpc(int x, int y, ulong senderClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == senderClientId) return;

        _setTileDataLocally(coords: new (x, y), editableItem: null);
    }

    private void _setTileDataLocally(Vector2Int coords, EditableItem editableItem)
    {
        _gridData.SetLocalTileData(coords, editableItem);
    }
}    