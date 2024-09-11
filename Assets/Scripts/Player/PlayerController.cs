using Unity.Netcode;
using UnityEngine;

public class PlayerController : KitchenItemParent
{
    [SerializeField] private PlayerStateSO _playerState;
    [SerializeField] private SelectedObjectsInRangeSO _selectedObjectsInRange;

    private const float DEFAULT_RAYCAST_RADIUS = 2f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            var playerMovementComponent = GetComponent<PlayerMovement>();
            var networkCharacterVisualComponent = GetComponent<NetworkCharacterVisual>();

            PlayerInput.Instance.OnInteract += () => _selectedObjectsInRange.SelectedCounter?.Interact(this);
            PlayerInput.Instance.OnInteractAlternative += () => _selectedObjectsInRange.SelectedCounter?.InteractAlternative(this);
            PlayerInput.Instance.OnInteractDuringEditing += () => _selectedObjectsInRange.SelectedEditingSubject?.Interact();
            PlayerInput.Instance.OnInteractDuringEditing += () => _selectedObjectsInRange.SelectedShop?.Interact();
            GameManager.Instance.OnStateChange += _clearSelectedItems;

            _selectedObjectsInRange.OnStartEditing += () => 
            {
                playerMovementComponent.DisableCountersCollision();
                networkCharacterVisualComponent.RenderTransparent();
            };

            _selectedObjectsInRange.OnEndEditing += () => 
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
            if (GameManager.Instance.State == GameState.Active) 
            {
                var colliders = _raycastSphere();
                _handleSelectCounter(colliders);
            }

            if (GameManager.Instance.State == GameState.Editing) 
            {
                _handleSelectEditableObject(_raycastSphere()); 
                _handleSelectClosestTile(_raycastSphere(raycastOffset: transform.forward * 4));
                _handleSelectShop(_raycastSphere(raycastOffset: transform.forward * 1, raycastRadius: 0.25f));
            }
        }
    }

    private void _clearSelectedItems(GameState _)
    {
        _selectedObjectsInRange.TriggerSelectCounter(null);
        _selectedObjectsInRange.TriggerSelectEditingSubject(null);
        _selectedObjectsInRange.TriggerSelectShop(null);
    }

    private void _handleSelectCounter(Collider[] colliders)
    {
        BaseCounter closestCounter = _getClosestObject<BaseCounter>(colliders);
        _selectedObjectsInRange.TriggerSelectCounter(closestCounter);
    }

    private void _handleSelectEditableObject(Collider[] colliders)
    {
        EditableItem closestEditableItem = _getClosestObject<EditableItem>(colliders);
        _selectedObjectsInRange.TriggerSelectEditingSubject(closestEditableItem);
    }

    private void _handleSelectShop(Collider[] colliders)
    {
        Shop closestShop = _getClosestObject<Shop>(colliders);
        _selectedObjectsInRange.TriggerSelectShop(closestShop);
    }
        
    private void _handleSelectClosestTile(Collider[] colliders)
    {
        GridTile closestTile = _getClosestObject<GridTile>(colliders);
        _selectedObjectsInRange.TriggerSelectGridTile(closestTile);
    }

    private Collider[] _raycastSphere(Vector3 raycastOffset = default, float raycastRadius = DEFAULT_RAYCAST_RADIUS)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position + raycastOffset, raycastRadius);

        return colliders;
    }

    private T _getClosestObject<T>(Collider[] colliders)
    {
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
