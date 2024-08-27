using Unity.Netcode;
using UnityEngine;

public class PlayerController : KitchenItemParent
{
    [SerializeField] private PlayerStateSO _playerState;
    [SerializeField] private SelectedCounterSO _selectedCounter;
    [SerializeField] private SelectedEditableItemSO _selectedEditableItem;

    private float _raycastDistance = 2f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            var playerMovementComponent = GetComponent<PlayerMovement>();
            var networkCharacterVisualComponent = GetComponent<NetworkCharacterVisual>();

            PlayerInput.Instance.OnInteract += () => _selectedCounter.SelectedCounter?.Interact(this);
            PlayerInput.Instance.OnInteractAlternative += () => _selectedCounter.SelectedCounter?.InteractAlternative(this);
            PlayerInput.Instance.OnInteractDuringEditing += () => _selectedEditableItem.SelectedEditingSubject?.Interact(this);
            GameManager.Instance.OnStateChange += _clearSelectedItems;

            _selectedEditableItem.OnStartEditing += () => 
            {
                playerMovementComponent.DisableCountersCollision();
                networkCharacterVisualComponent.RenderTransparent();
            };

            _selectedEditableItem.OnEndEditing += () => 
            {
                playerMovementComponent.EnableCountersCollision();
                networkCharacterVisualComponent.RenderFilled();
            };
            
        }

        OnItemDrop += () => SoundManager.SoundEvents.TriggerOnObjectDropSound(transform.position);
        OnItemPickup += () => SoundManager.SoundEvents.TriggerOnObjectPickupSound(transform.position);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientId) => this.DestroyCurrentItemHeld();
        }
    }

    public override void OnDestroy()
    {
        if (IsOwner)
        {
            GameManager.Instance.OnStateChange -= _clearSelectedItems;
        }
    }

    private void Update()
    {     
        if (!IsOwner)
        {
            return;
        }

        if (_playerState.IsWalking) 
        {
            if (GameManager.Instance.State == GameState.Active) _handleSelectCounter();
            if (GameManager.Instance.State == GameState.Editing) _handleSelectEditableObject(); _handleSelectClosestTile();
        }
    }

    private void _clearSelectedItems(GameState _)
    {
        _selectedCounter.TriggerSelectCounterEvent(null);
        _selectedEditableItem.TriggerSelectEditingSubject(null);
    }

    private void _handleSelectCounter()
    {
        BaseCounter closestCounter = _getClosestObject<BaseCounter>();
        _selectedCounter.TriggerSelectCounterEvent(closestCounter);
    }

    private void _handleSelectEditableObject()
    {
        EditableItem closestEditableItem = _getClosestObject<EditableItem>();
        _selectedEditableItem.TriggerSelectEditingSubject(closestEditableItem);
    }
        
    private void _handleSelectClosestTile()
    {
        GridTile closestTile = _getClosestObject<GridTile>(raycastOffset: transform.forward * 4);
        _selectedEditableItem.TriggerSelectGridTile(closestTile);
    }

    private T _getClosestObject<T>(Vector3 raycastOffset = default)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position + raycastOffset, _raycastDistance);
        T closestObject = default(T);
        float closestDistance = float.MaxValue;

        foreach(Collider collider in colliders)
        {
            if (collider.transform.TryGetComponent(out T objct))
            {
                Vector3 playerPositionWithOffsetToFacingDirection = transform.position + (transform.forward / 5);
                float distance = (collider.transform.position - playerPositionWithOffsetToFacingDirection).sqrMagnitude;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = objct;
                }
            }
        }

        return closestObject;
    }
}
