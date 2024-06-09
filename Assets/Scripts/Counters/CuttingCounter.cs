using System;
using Unity.Netcode;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
    [SerializeField] private ProgressTrackerOnNetwork _cuttingProgress;

    public event Action OnCut;

    public override void Interact(KitchenItemParent player)
    {
        if (KitchenItemParent.TryAddIngredientToPlateOwner(player, this))
        {
            _cuttingProgress.SetProgressServerRpc(0);
        }

        if (player.IsHoldingItem() && player.GetCurrentItemHeld().GetItemReference().IsSliceable())
        {
            _cuttingProgress.SetProgressServerRpc(0);
            _cuttingProgress.SetMaxProgressServerRpc(player.GetCurrentItemHeld().GetItemReference().SliceableSO.CuttingSlicesCount);
            KitchenItemParent.SwapItemsOfTwoOwners(player, this);
        }
        else if (!player.IsHoldingItem())
        {
            _cuttingProgress.SetProgressServerRpc(0);
            KitchenItemParent.SwapItemsOfTwoOwners(player, this);
        }
    }

    public override void InteractAlternative(KitchenItemParent player)
    {
        if (this.IsHoldingItem() && this.GetCurrentItemHeld().GetItemReference().IsSliceable())
        {
            float newCuttingProgress = _cuttingProgress.Progress + 1;
            _cuttingProgress.SetProgressServerRpc(newCuttingProgress);
            _triggerOnCutEventsServerRpc();

            if (this.GetCurrentItemHeld().GetItemReference().SliceableSO.CuttingSlicesCount == newCuttingProgress)
            {
                KitchenItemSO slicedItem = this.GetCurrentItemHeld().GetItemReference().SliceableSO.SlicedPrefab.GetItemReference();
                this.DestroyCurrentItemHeld();

                this.SpawnKitchenItem(slicedItem);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void _triggerOnCutEventsServerRpc()
    {
        _triggerOnCutEventsClientRpc();
    }

    [ClientRpc]
    private void _triggerOnCutEventsClientRpc()
    {
        OnCut?.Invoke();
        SoundManager.SoundEvents.TriggerOnCutSound(transform.position);
    }
}
