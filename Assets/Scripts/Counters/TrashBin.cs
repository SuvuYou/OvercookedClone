public class TrashBin : BaseCounter
{
    public override void Interact(KitchenItemParent player)
    {
        if (!player.IsHoldingItem()) return;

        SoundManager.SoundEvents.TriggerOnTrashSound(transform.position);
        player.DestroyCurrentItemHeld();
    }
}
