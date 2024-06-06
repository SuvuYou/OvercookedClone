using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : KitchenItemParent
{
    [SerializeField] private PlayerStateSO _playerState;
    [SerializeField] private SelectedCounterSO _selectedCounter;

    private float _raycastDistance = 1.2f;

    private void Start()
    {
        PlayerInput.Instance.OnInteract += () => _selectedCounter.SelectedCounter?.Interact(this);
        PlayerInput.Instance.OnInteractAlternative += () => _selectedCounter.SelectedCounter?.InteractAlternative(this);
        OnItemDrop += () => SoundManager.SoundEvents.TriggerOnObjectDropSound(transform.position);
        OnItemPickup += () => SoundManager.SoundEvents.TriggerOnObjectPickupSound(transform.position);
    }

    private void Update()
    {     
        if (_playerState.IsWalking) 
        {
            _handleSelectCounter();
        }
    }

    private void _handleSelectCounter()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _raycastDistance);
        BaseCounter closestCounter = null;
        float closestDistance = float.MaxValue;

        foreach(Collider collider in colliders)
        {
            if (collider.transform.TryGetComponent(out BaseCounter counter))
            {
                Vector3 playerPositionWithOffsetToFacingDirection = transform.position + (transform.forward / 5);
                float distance = (collider.transform.position - playerPositionWithOffsetToFacingDirection).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCounter = counter;
                }
            }
        }

        _selectedCounter.TriggerSelectCounterEvent(closestCounter);
    }
}
