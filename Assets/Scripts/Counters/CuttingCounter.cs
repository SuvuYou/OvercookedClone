using System;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
    [SerializeField] private ProgressTrackerSO _cuttingProgress;

    public event Action OnCut;

    public override void Interact(KitchenItemParent player)
    {
        if (KitchenItemParent.TryAddIngredientToPlate(player, this))
        {
            _cuttingProgress.TriggerProgressUpdate(0);
        }

        if (player.IsHoldingItem() && player.GetCurrentItemHeld().GetItemReference().IsSliceable())
        {
            _cuttingProgress.TriggerProgressUpdate(0);
            _cuttingProgress.SetMaxProgress(player.GetCurrentItemHeld().GetItemReference().SliceableSO.CuttingSlicesCount);
            KitchenItemParent.SwapItemsOfTwoOwners(player, this);
        }
        else if (!player.IsHoldingItem())
        {
            _cuttingProgress.TriggerProgressUpdate(0);
            KitchenItemParent.SwapItemsOfTwoOwners(player, this);
        }
    }

    public override void InteractAlternative(KitchenItemParent player)
    {
        if (IsHoldingItem() && GetCurrentItemHeld().GetItemReference().IsSliceable())
        {
            _cuttingProgress.TriggerProgressUpdate(_cuttingProgress.Progress + 1);
            OnCut?.Invoke();
            SoundManager.SoundEvents.TriggerOnCutSound(transform.position);

            if (GetCurrentItemHeld().GetItemReference().SliceableSO.CuttingSlicesCount == _cuttingProgress.Progress)
            {
                KitchenItem slicedItem = GetCurrentItemHeld().GetItemReference().SliceableSO.SlicedPrefab;
                DestroyCurrentItemHeld();

                SetCurrentItemHeld(Instantiate(slicedItem, Vector3.zero, Quaternion.identity));
            }
        }
    }
}
