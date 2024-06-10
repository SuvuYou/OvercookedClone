using Unity.Netcode;

public class TrashBin : BaseCounter
{
    public override void Interact(KitchenItemParent player)
    {
        if (!player.IsHoldingItem()) return;

        _triggerTrashSoundServerRpc();

        player.DestroyCurrentItemHeld();
    }

    [ServerRpc (RequireOwnership = false)]
    private void _triggerTrashSoundServerRpc()
    {
        _triggerTrashSoundClientRpc();
    }

    [ClientRpc]
    private void _triggerTrashSoundClientRpc()
    {
        SoundManager.SoundEvents.TriggerOnTrashSound(transform.position);
    }
}
