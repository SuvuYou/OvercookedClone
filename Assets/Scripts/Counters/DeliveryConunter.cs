public class DeliveryConunter : BaseCounter
{
    public override void Interact(KitchenItemParent player)
    {
        if (player.IsHoldingItem() && player.GetCurrentItemHeld().TryGetPlateComponent(out Plate plate))
        {
            player.DestroyCurrentItemHeld();
            
            if (DeliveryManager.Instance.TryDeliverRecipePlate(plate))
            {
                SoundManager.SoundEvents.TriggerOnDeliverSuccessSound(transform.position);
            }
            else
            {
                SoundManager.SoundEvents.TriggerOnDeliverFailedSound(transform.position);
            }
        }
    }
}
