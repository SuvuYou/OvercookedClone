using Unity.Netcode;

public class DeliveryConunter : BaseCounter
{
    public override void Interact(KitchenItemParent player)
    {
        if (player.IsHoldingItem() && player.GetCurrentItemHeld().TryGetPlateComponent(out Plate plate))
        {
            player.DestroyCurrentItemHeld();
            
            if (DeliveryManager.Instance.TryDeliverRecipePlate(plate))
            {
                _triggerSuccessfulSoundEffectServerRpc();
            }
            else
            {
                _triggerFailedSoundEffectServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void _triggerSuccessfulSoundEffectServerRpc()
    {
        _triggerSuccessfulSoundEffectclientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void _triggerFailedSoundEffectServerRpc()
    {
        _triggerFailedSoundEffectClientRpc();
    }

    [ClientRpc]
    private void _triggerSuccessfulSoundEffectclientRpc()
    {
        SoundManager.SoundEvents.TriggerOnDeliverSuccessSound(transform.position);
    }

    [ClientRpc]
    private void _triggerFailedSoundEffectClientRpc()
    {
        SoundManager.SoundEvents.TriggerOnDeliverFailedSound(transform.position);
    }
}
