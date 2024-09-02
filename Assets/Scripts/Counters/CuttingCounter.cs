using System;
using Unity.Netcode;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
    public event Action OnCut;

    [SerializeField] private ProgressTrackerOnNetwork _cuttingProgress;
    [SerializeField] private ProgressBarUI _progressBar;

    private bool _isCutWithoutSwapping;
    private KitchenItemSO _cachedSlicedItem;

    private void Awake()
    {
        _progressBar.Init(progressTracker: _cuttingProgress.ProgressTracker);
    }

    public override void Interact(KitchenItemParent player)
    {        
        if (KitchenItemParent.TryAddIngredientToPlateOwner(player, this))
        {
            _cuttingProgress.SetProgress(0);

            return;
        }

        if (player.IsHoldingItem() && player.GetCurrentItemHeld().GetItemReference().IsSliceable())
        {
            int maxProgress = player.GetCurrentItemHeld().GetItemReference().SliceableSO.CuttingSlicesCount;
            
            if (_trySafeSwapItems(player))
            {
                _cuttingProgress.SetProgress(0);
                _cuttingProgress.SetMaxProgress(maxProgress);
            
                return;
            }   
        }
        
        if (!player.IsHoldingItem())
        {
            if (_trySafeSwapItems(player))
            {
                _cuttingProgress.SetProgress(0);

                return;
            }   
        }
    }

    public override void InteractAlternative(KitchenItemParent player)
    {
        if (this.IsHoldingItem() && this.GetCurrentItemHeld().GetItemReference().IsSliceable() && !_isCutWithoutSwapping)
        {
            float newCuttingProgress = _cuttingProgress.Progress + 1;
            _cuttingProgress.SetProgress(newCuttingProgress);
            _triggerCut();

            if (this.GetCurrentItemHeld().GetItemReference().SliceableSO.CuttingSlicesCount == newCuttingProgress)
            {
                _cachedSlicedItem = this.GetCurrentItemHeld().GetItemReference().SliceableSO.SlicedPrefab.GetItemReference();
                _isCutWithoutSwapping = true;

                this.DestroyCurrentItemHeld();

                this.SpawnKitchenItem(_cachedSlicedItem);
            }
        }
    }

    private bool _trySafeSwapItems(KitchenItemParent other)
    {
        if (GetCurrentItemHeld() != null && GetCurrentItemHeld().GetItemReference() != _cachedSlicedItem && _isCutWithoutSwapping)
        {
            return false;  
        }

        _isCutWithoutSwapping = false;
        KitchenItemParent.SwapItemsOfTwoOwners(other, this);

        return true;  
    }

    private void _triggerCut()
    {
        _triggerCutLocally();
        _triggerOnCutEventsServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void _triggerOnCutEventsServerRpc(ServerRpcParams rpcParams = default)
    {
        _triggerOnCutEventsClientRpc(sender: rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void _triggerOnCutEventsClientRpc(ulong sender)
    {
        if (NetworkManager.Singleton.LocalClientId == sender) return;
        
        _triggerCutLocally();
    }

    private void _triggerCutLocally()
    {
        OnCut?.Invoke();
        SoundManager.SoundEvents.TriggerOnCutSound(transform.position);
    }
}
