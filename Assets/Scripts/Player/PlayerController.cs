using Unity.Netcode;
using UnityEngine;

public class PlayerController : KitchenItemParent
{
    [SerializeField] private PlayerStateSO _playerState;
    [SerializeField] private SelectedCounterSO _selectedCounter;

    private float _raycastDistance = 1.2f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            PlayerInput.Instance.OnInteract += () => _selectedCounter.SelectedCounter?.Interact(this);
            PlayerInput.Instance.OnInteractAlternative += () => _selectedCounter.SelectedCounter?.InteractAlternative(this);
        }

        OnItemDrop += () => SoundManager.SoundEvents.TriggerOnObjectDropSound(transform.position);
        OnItemPickup += () => SoundManager.SoundEvents.TriggerOnObjectPickupSound(transform.position);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientId) => this.DestroyCurrentItemHeld();
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
