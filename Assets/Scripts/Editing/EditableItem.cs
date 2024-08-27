using Unity.Netcode;
using UnityEngine;

// TODO: generate default map counters using code
public class EditableItem : NetworkBehaviour
{
    [SerializeField] private SelectedEditableItemSO _selectedEditableSubject;
    [SerializeField] private GameObject _selectedVisualIndicator;
    [SerializeField] private ClientAuthoritativeNetworkTransform _networkTransform;

    private Vector2Int _parentTileCoordinats;
    private Vector2Int _defaultCoords;

    private ulong _defaultEditorClientId = 999;
    private ulong _currentEditorClientId;

    private void Start()
    {
        _currentEditorClientId = _defaultEditorClientId;

        _selectedEditableSubject.OnSelectSubject += _checkIsCounterSelected;
        _selectedEditableSubject.OnSelectTile += _updateTransformPosition;

        _selectedEditableSubject.OnStartEditing += _startEditing;
        _selectedEditableSubject.OnEndEditing += _endEditing;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    
        _selectedEditableSubject.OnSelectSubject -= _checkIsCounterSelected;
        _selectedEditableSubject.OnSelectTile -= _updateTransformPosition;

        _selectedEditableSubject.OnStartEditing -= _startEditing;
        _selectedEditableSubject.OnEndEditing -= _endEditing;
    }

    private void _startEditing()
    {
        if (_selectedEditableSubject.SelectedEditingSubject != this) return;

        _setCurrentEditor(editorClientId: NetworkManager.Singleton.LocalClientId);
        _assignCoordinats(_defaultCoords);
    }

    private void _endEditing()
    {
        if (_selectedEditableSubject.SelectedEditingSubject != this) return;

        _setCurrentEditor(editorClientId: _defaultEditorClientId);
        _assignCoordinats(_selectedEditableSubject.SelectedGridTile.Coordinats);
        _updateTransformPosition(_selectedEditableSubject.SelectedGridTile);  
    }

    public void Interact(PlayerController player) 
    { 
        if (_currentEditorClientId == _defaultEditorClientId)
        {
            _selectedEditableSubject.TriggerOnStartEditing();
            
            return;
        }

        if (_currentEditorClientId != NetworkManager.Singleton.LocalClientId) return;

        // if (isTryingToSell)
        // {
        //      destroy + update balance

        //     return;
        // }

        if (_selectedEditableSubject.SelectedGridTile == null || !_selectedEditableSubject.SelectedGridTile.IsAvailable()) return;
        
        _selectedEditableSubject.TriggerOnEndEditing();
    }

    private void _assignCoordinats(Vector2Int coords)
    {
        if (coords != _defaultCoords)
        {
            TileMapGrid.Instance.TakeTile(coords);
            _setCoordinats(coords.x, coords.y);

            return;
        }

        if (_parentTileCoordinats == _defaultCoords) return; 

        TileMapGrid.Instance.FreeTile(_parentTileCoordinats);
        _setCoordinats(_defaultCoords.x, _defaultCoords.y);
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
        _parentTileCoordinats = new (x, y);
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

    private void _checkIsCounterSelected(EditableItem newSelectedSubject)
    {
        if (_selectedVisualIndicator != null) _selectedVisualIndicator.SetActive(newSelectedSubject == this);
    }

    private void _updateTransformPosition(GridTile newSelectedTile)
    {
        if (newSelectedTile != null && _currentEditorClientId == NetworkManager.Singleton.LocalClientId) _networkTransform.SetTargetPosition(newSelectedTile.GetPlacePosition());
    }
}
