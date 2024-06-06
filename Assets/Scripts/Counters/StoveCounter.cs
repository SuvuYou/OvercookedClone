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
    [SerializeField] private ProgressTrackerSO _fryingProgress;
    

    private void Awake ()
    {
        _fryingProgress.SetMaxProgress(float.MaxValue);
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
            _fryingProgress.TriggerProgressUpdate(0);
            _switchState(State.Idle); 
        }

        if (player.IsHoldingItem() && player.GetCurrentItemHeld().GetItemReference().IsFryable()) 
        {
            _fryingProgress.TriggerProgressUpdate(0);
            _fryingProgress.SetMaxProgress(player.GetCurrentItemHeld().GetItemReference().FryableSO.FryingTimer);   
            _switchState(State.Frying);

            KitchenItemParent.SwapItemsOfTwoOwners(player, this);
        }
        else if (!player.IsHoldingItem()) 
        {
            _fryingProgress.TriggerProgressUpdate(0);
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
        _fryingProgress.TriggerProgressUpdate(_fryingProgress.Progress + Time.deltaTime);

        if (_fryingProgress.Progress >= _fryingProgress.MaxProgress)
        {
            KitchenItem friedItem = GetCurrentItemHeld().GetItemReference().FryableSO.FriedPrefab;
            
            DestroyCurrentItemHeld();
            SetCurrentItemHeld(Instantiate(friedItem, Vector3.zero, Quaternion.identity));

            if (friedItem.GetItemReference().IsFryable())
            {
                _fryingProgress.SetMaxProgress(friedItem.GetItemReference().FryableSO.FryingTimer); 
                _fryingProgress.TriggerProgressUpdate(0);
                _switchState(State.Fried);
            }
            else
            {
                _fryingProgress.TriggerProgressUpdate(0);
                _switchState(State.Burned);
            }
        }
    }


    public State GetCurrentState() => _state;
}
