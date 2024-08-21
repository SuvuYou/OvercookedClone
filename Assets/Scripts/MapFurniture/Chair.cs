using UnityEngine;
using Unity.Netcode;

public class Chair : BaseCounter
{
    [SerializeField] private Transform _sittingPlacePosition;
    
    private Customer _currentCustomer;
    public Transform SittingPlacePosition { get => _sittingPlacePosition; }

    public void TakeSit(Customer sitter)
    {
        _currentCustomer = sitter;
        sitter.gameObject.transform.position = _sittingPlacePosition.position;
    }

    public void FinishDish()
    {
        KitchenItemParent.ClearAllIngredientsOffPlate(plateOwner: this);
    }

    public override void Interact(KitchenItemParent player)
    {
        if (player.IsHoldingItem() && player.GetCurrentItemHeld().TryGetPlateComponent(out Plate plate))
        {
            if (_currentCustomer.TryRecieveOrder(plate))
            {
                plate.DeliverPlate();
                KitchenItemParent.SwapItemsOfTwoOwners(player, this);
                _triggerSuccessfulSoundEffectServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void _triggerSuccessfulSoundEffectServerRpc()
    {
        _triggerSuccessfulSoundEffectclientRpc();
    }

    [ClientRpc]
    private void _triggerSuccessfulSoundEffectclientRpc()
    {
        SoundManager.SoundEvents.TriggerOnDeliverSuccessSound(transform.position);
    }
}
