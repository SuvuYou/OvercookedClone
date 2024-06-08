using System;
using Unity.Netcode;
using UnityEngine;

public class StoveCounter : BaseCounter
{
    public enum State {
        Idle,
        Frying,
        Fried,
        Burned,
    }

    public event Action OnStoveOn;
    public event Action OnStoveOff;

    private NetworkVariable<State> _state = new();
    [SerializeField] private ProgressTrackerOnNetwork _fryingProgress;
    
    private void Awake ()
    {
        _fryingProgress.SetMaxProgressServerRpc(float.MaxValue);
    }

    private void Update ()
    {
        switch(_state.Value)
        {
            case State.Frying:
            case State.Fried:
                _processFrying();
                break;
            case State.Idle:
            case State.Burned:
                break;
        }
    }

    public override void Interact(KitchenItemParent player)
    {
        if (KitchenItemParent.TryAddIngredientToPlate(player, this))
        {
            _fryingProgress.SetProgressServerRpc(0);
            _switchStateServerRpc(State.Idle); 
        }

        if (player.IsHoldingItem() && player.GetCurrentItemHeld().GetItemReference().IsFryable()) 
        {
            _fryingProgress.SetProgressServerRpc(0);
            _fryingProgress.SetMaxProgressServerRpc(player.GetCurrentItemHeld().GetItemReference().FryableSO.FryingTimer);   
            _switchStateServerRpc(State.Frying);

            KitchenItemParent.SwapItemsOfTwoOwners(player, this);
        }
        else if (!player.IsHoldingItem()) 
        {
            _fryingProgress.SetProgressServerRpc(0);
            _switchStateServerRpc(State.Idle); 

            KitchenItemParent.SwapItemsOfTwoOwners(player, this);
        }
    }

    private void _processFrying()
    {
        if (!IsServer)
        {
            return;
        }

        float newProgress = _fryingProgress.Progress + Time.deltaTime;
        _fryingProgress.SetProgressServerRpc(newProgress);

        if (newProgress >= _fryingProgress.MaxProgress)
        {
            KitchenItemSO friedItem = this.GetCurrentItemHeld().GetItemReference().FryableSO.FriedPrefab.GetItemReference();
            
            this.DestroyCurrentItemHeld();
            this.SpawnKitchenItem(friedItem);

            if (friedItem.IsFryable())
            {
                _fryingProgress.SetMaxProgressServerRpc(friedItem.FryableSO.FryingTimer); 
                _fryingProgress.SetProgressServerRpc(0);
                _switchStateServerRpc(State.Fried);
            }
            else
            {
                _fryingProgress.SetProgressServerRpc(0);
                _switchStateServerRpc(State.Burned);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void _switchStateServerRpc(State newState)
    {
        _state.Value = newState;
        _triggerStoveEventsOnStateChangeClientRpc(newState);
    }

    [ClientRpc]
    private void _triggerStoveEventsOnStateChangeClientRpc(State newState)
    {
        if (newState == State.Frying || newState == State.Fried) OnStoveOn?.Invoke();
        else OnStoveOff?.Invoke();
    }

    public State GetCurrentState() => _state.Value;
}
