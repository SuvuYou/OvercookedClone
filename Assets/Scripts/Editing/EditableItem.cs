using Unity.Netcode;
using UnityEngine;

public class EditableItem : NetworkBehaviour
{
    [SerializeField] private PurchasableItemSO _purchasableItemReference;
    public PurchasableItemSO PurchasableItemReference { get => _purchasableItemReference; } 

    [SerializeField] private SelectedObjectsInRangeSO _selectedObjectsInRange;
    [SerializeField] private GameObject _selectedVisualIndicator;
    [SerializeField] private ClientAuthoritativeNetworkTransform _networkTransform;

    public Vector2Int ParentTileCoordinats { get; private set; }
    private Vector2Int DEFAULT_COORDS = new (-1, -1);

    private ulong _currentEditorClientId;
    private const ulong DEFAULT_EDITOR_CLIENT_ID = 999;

    private void Awake()
    {
        _currentEditorClientId = DEFAULT_EDITOR_CLIENT_ID;

        _selectedObjectsInRange.OnSelectSubject += _checkIsItemSelected;
        _selectedObjectsInRange.OnSelectTile += _updateTransformPosition;

        _selectedObjectsInRange.OnStartEditing += _startEditing;
        _selectedObjectsInRange.OnEndEditing += _endEditing;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        TileMapGrid.Instance.RemoveItemFormGrid(ParentTileCoordinats);
    
        _selectedObjectsInRange.OnSelectSubject -= _checkIsItemSelected;
        _selectedObjectsInRange.OnSelectTile -= _updateTransformPosition;

        _selectedObjectsInRange.OnStartEditing -= _startEditing;
        _selectedObjectsInRange.OnEndEditing -= _endEditing;
    }

    private void _startEditing()
    {
        if (_selectedObjectsInRange.SelectedEditingSubject != this) return;

        _setCurrentEditor(editorClientId: NetworkManager.Singleton.LocalClientId);
        AssignCoordinats(DEFAULT_COORDS);
    }

    private void _endEditing()
    {
        if (_selectedObjectsInRange.SelectedEditingSubject != this) return;

        _setCurrentEditor(editorClientId: DEFAULT_EDITOR_CLIENT_ID);
        AssignCoordinats(_selectedObjectsInRange.SelectedGridTile?.Coordinats ?? DEFAULT_COORDS);
    }

    public void Interact() 
    { 
        if (_currentEditorClientId == DEFAULT_EDITOR_CLIENT_ID)
        {
            _selectedObjectsInRange.TriggerOnStartEditing();
            
            return;
        }

        if (_currentEditorClientId != NetworkManager.Singleton.LocalClientId) return;

        if (_selectedObjectsInRange.SelectedShop != null) return;

        if (_selectedObjectsInRange.SelectedGridTile == null || !_selectedObjectsInRange.SelectedGridTile.IsAvailable()) return;
        
        _selectedObjectsInRange.TriggerOnEndEditing();
    }

    public void AssignCoordinats(Vector2Int coords)
    {
        if (coords != DEFAULT_COORDS)
        {
            TileMapGrid.Instance.AddItemToGrid(coords, this);
            _setCoordinats(coords.x, coords.y);
            _networkTransform.SetTargetPosition(TileMapGrid.Instance.GetTileByCoordinats(coords).GetPlacePosition());

            return;
        }

        if (ParentTileCoordinats == DEFAULT_COORDS) return; 

        TileMapGrid.Instance.RemoveItemFormGrid(ParentTileCoordinats);
        _setCoordinats(DEFAULT_COORDS.x, DEFAULT_COORDS.y);
    }

    private void _setCoordinats(int x, int y)
    {
        _setCoordinatsLocally(x, y);
        _setCoordinatsServerRpc(x, y);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _setCoordinatsServerRpc(int x, int y, ServerRpcParams rpcParams = default) 
    {
        _setCoordinatsClientRpc(x, y, senderClientId: rpcParams.Receive.SenderClientId);
    }
    
    [ClientRpc]
    private void _setCoordinatsClientRpc(int x, int y, ulong senderClientId) 
    {
        if (senderClientId == NetworkManager.Singleton.LocalClientId) return;
        
        _setCoordinatsLocally(x, y);
    } 
    
    private void _setCoordinatsLocally(int x, int y)
    {
        ParentTileCoordinats = new (x, y);
    }

    private void _setCurrentEditor(ulong editorClientId)
    {
        _setCurrentEditorLocally(editorClientId);
        _setCurrentEditorServerRpc(editorClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _setCurrentEditorServerRpc(ulong editorClientId, ServerRpcParams rpcParams = default) 
    {
        _setCurrentEditorClientRpc(editorClientId, senderClientId: rpcParams.Receive.SenderClientId);
    }
    
    [ClientRpc]
    private void _setCurrentEditorClientRpc(ulong editorClientId, ulong senderClientId) 
    {
        if (senderClientId == NetworkManager.Singleton.LocalClientId) return;
        
        _setCurrentEditorLocally(editorClientId);
    } 
    
    private void _setCurrentEditorLocally(ulong editorClientId)
    {
        _currentEditorClientId = editorClientId;
    }

    private void _checkIsItemSelected(EditableItem newSelectedSubject)
    {
        _selectedVisualIndicator.SetActive(newSelectedSubject == this);
    }

    private void _updateTransformPosition(GridTile newSelectedTile)
    {
        if (newSelectedTile != null && _currentEditorClientId == NetworkManager.Singleton.LocalClientId) _networkTransform.SetTargetPosition(newSelectedTile.GetPlacePosition());
    }
}
