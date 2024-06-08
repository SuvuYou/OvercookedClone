using System;
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

    private State _state;
    [SerializeField] private ProgressTrackerOnNetwork _fryingProgress;
    
    private void Awake ()
    {
        _fryingProgress.SetMaxProgressServerRpc(float.MaxValue);
    }

    private void Update ()
    {
        switch(_state)
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
            _switchState(State.Idle); 
        }

        if (player.IsHoldingItem() && player.GetCurrentItemHeld().GetItemReference().IsFryable()) 
        {
            _fryingProgress.SetProgressServerRpc(0);
            _fryingProgress.SetMaxProgressServerRpc(player.GetCurrentItemHeld().GetItemReference().FryableSO.FryingTimer);   
            _switchState(State.Frying);

            KitchenItemParent.SwapItemsOfTwoOwners(player, this);
        }
        else if (!player.IsHoldingItem()) 
        {
            _fryingProgress.SetProgressServerRpc(0);
            _switchState(State.Idle); 

            KitchenItemParent.SwapItemsOfTwoOwners(player, this);
        }
    }

    private void _switchState(State newState)
    {
        _state = newState;

        if (newState == State.Frying || newState == State.Fried) OnStoveOn?.Invoke();
        else OnStoveOff?.Invoke();
    }

    private void _processFrying()
    {
        _fryingProgress.SetProgressServerRpc(_fryingProgress.Progress + Time.deltaTime);

        if (_fryingProgress.Progress >= _fryingProgress.MaxProgress)
        {
            KitchenItemSO friedItem = this.GetCurrentItemHeld().GetItemReference().FryableSO.FriedPrefab.GetItemReference();
            
            this.DestroyCurrentItemHeld();
            this.SpawnKitchenItem(friedItem);

            if (friedItem.IsFryable())
            {
                _fryingProgress.SetMaxProgressServerRpc(friedItem.FryableSO.FryingTimer); 
                _fryingProgress.SetProgressServerRpc(0);
                _switchState(State.Fried);
            }
            else
            {
                _fryingProgress.SetProgressServerRpc(0);
                _switchState(State.Burned);
            }
        }
    }


    public State GetCurrentState() => _state;
}
