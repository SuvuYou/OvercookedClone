using System;
using Unity.Netcode;
using UnityEngine;

public class StoveCounter : BaseCounter
{
    public enum FryingState {
        Idle,
        Frying,
        Fried,
        Burned,
    }

    public event Action OnStoveOn;
    public event Action OnStoveOff;

    private NetworkVariable<FryingState> _state = new();
    [SerializeField] private ProgressTrackerOnNetwork _fryingProgress;
    [SerializeField] private ProgressBarUI _progressBar;

    private void Awake()
    {
        _progressBar.Init(progressTracker: _fryingProgress.ProgressTracker);
    }

    private void Update ()
    {
        switch(_state.Value)
        {
            case FryingState.Frying:
            case FryingState.Fried:
                _processFrying();
                break;
            case FryingState.Idle:
            case FryingState.Burned:
                break;
        }
    }

    public override void Interact(KitchenItemParent player)
    {
        if(!player.IsHoldingItem())
        {
            _resetProgress();
            _switchStateServerRpc(FryingState.Idle); 

            KitchenItemParent.SwapItemsOfTwoOwners(player, this);

            return;
        }

        if (KitchenItemParent.TryAddIngredientToPlateOwner(player, this))
        {
            _resetProgress();
            _switchStateServerRpc(FryingState.Idle); 

            return;
        }

        if (player.GetCurrentItemHeld().GetItemReference().IsFryable()) 
        {
            _resetProgress();
            _fryingProgress.SetMaxProgress(player.GetCurrentItemHeld().GetItemReference().FryableSO.FryingTimer);   
            _switchStateServerRpc(player.GetCurrentItemHeld().GetItemReference().FryableSO.State);

            KitchenItemParent.SwapItemsOfTwoOwners(player, this);

            return;
        }
    }

    private void _processFrying()
    {
        if (!IsServer)
        {
            return;
        }

        _fryingProgress.EnableContiniousProgressUpdate();

        if (_fryingProgress.Progress >= _fryingProgress.MaxProgress)
        {
            KitchenItemSO friedItem = this.GetCurrentItemHeld().GetItemReference().FryableSO.FriedPrefab.GetItemReference();
            
            this.DestroyCurrentItemHeld();
            this.SpawnKitchenItem(friedItem);

            _resetProgress();

            if (friedItem.IsFryable())
            {
                _fryingProgress.SetMaxProgress(friedItem.FryableSO.FryingTimer); 
                _switchStateServerRpc(friedItem.FryableSO.State);
            }
            else
            {
                _switchStateServerRpc(FryingState.Burned);
            }
        }
    }

    private void _resetProgress()
    {
        _fryingProgress.SetProgress(0);
        _fryingProgress.DisableContiniousProgressUpdate();
    }

    [ServerRpc(RequireOwnership = false)]
    private void _switchStateServerRpc(FryingState newState)
    {
        _state.Value = newState;
        _triggerStoveEventsOnStateChangeClientRpc(newState);
    }

    [ClientRpc]
    private void _triggerStoveEventsOnStateChangeClientRpc(FryingState newState)
    {
        if (newState == FryingState.Frying || newState == FryingState.Fried) OnStoveOn?.Invoke();
        else OnStoveOff?.Invoke();
    }

    public FryingState GetCurrentState() => _state.Value;
}
