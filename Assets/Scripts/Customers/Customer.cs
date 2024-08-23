using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Customer : MonoBehaviour
{
    private enum State 
    {
        Walking,
        WaitingForOrder,
        Eating,
        Idle,
    }

    private const float SITTING_DISTANCE_THRESHOLD = 0.05f;
    private const float EXIT_DISTANCE_THRESHOLD = 0.5f;

    public event Action OnSitDown;
    public event Action OnRecieveOrder;
    public event Action OnFinishEating;
    public event Action OnCustomerLeaving;

    public bool IsFinishedEating = false;
    public float PriceMutiplier { get; private set; } = 1f;

    private State _currentState = State.Idle;

    private Vector3 _lastPositionBeforeSittingDown;

    [SerializeField] private AvailableRecipesListSO _availableRecipesList;
    public RecipeSO Order { get; private set; }
    public Chair AssingedChair { get; private set; }

    private TimingTimer _eatingTimer = new();
    private ProgressTracker _eatingProgressTracker = new();
    public ProgressTracker EatingProgressTracker { get => _eatingProgressTracker; }
    private NavMeshAgent _navMehAgent;

    private void Awake()
    {
        _navMehAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.Walking:
                _checkIsCloseToDestination();
                break;
            case State.Eating:
                _updateEatingTimer();
                break;
            default:
                break;    
        } 
    }

    public bool IsWalking () => _navMehAgent.isActiveAndEnabled && !_navMehAgent.isStopped;
    
    public void SetPriceMultiplier(float priceMutiplier) => PriceMutiplier = priceMutiplier;

    private void _checkIsCloseToDestination()
    {
        if (!IsFinishedEating && _navMehAgent.remainingDistance < SITTING_DISTANCE_THRESHOLD)
        {
            _sitDown();
            OnSitDown?.Invoke();
        }

        if (IsFinishedEating && _navMehAgent.remainingDistance < EXIT_DISTANCE_THRESHOLD)
        {
            OnCustomerLeaving?.Invoke();
            Destroy(this.gameObject);
        }
    }

    private void _updateEatingTimer()
    {
        _eatingTimer.SubtractTime(Time.deltaTime);
        _eatingProgressTracker.TriggerProgressUpdate(newProgress: _eatingProgressTracker.Progress + Time.deltaTime);

        if (_eatingTimer.IsTimerUp())
        {
            _eatingTimer.ResetTimer();
            _eatingProgressTracker.SetMaxProgress(_eatingTimer.Time);
            _eatingProgressTracker.TriggerProgressUpdate(newProgress: 0);

            AssingedChair.FinishDish();
            _switchState(State.Idle);

            IsFinishedEating = true;
            OnFinishEating?.Invoke();
        }
    }

    public int MakeAnOrder(int forceRecipeIndex = -1)
    {
        int recipeIndex = UnityEngine.Random.Range(0, _availableRecipesList.AvailableRecipes.Count);

        if (forceRecipeIndex != -1) recipeIndex = forceRecipeIndex;
        
        Order = _availableRecipesList.AvailableRecipes[recipeIndex];
        _eatingTimer.SetDefaultTimerTime(defaultTimerValue: Order.EatingTime);
        _eatingProgressTracker.SetMaxProgress(Order.EatingTime);

        return recipeIndex;
    } 

    public void Leave(Vector3 exitPosition)
    {
        gameObject.transform.position = _lastPositionBeforeSittingDown;
        _startAgent(destination: exitPosition);
        _switchState(State.Walking);
    }

    public bool CanRecieveOrder(Plate plate)
    {
        if (_currentState != State.WaitingForOrder) return false;

        if (!Order.Ingredients.OrderBy(ing => ing.ItemName).SequenceEqual(plate.Ingredients.OrderBy(ing => ing.ItemName))) return false;
        
        return true;
    } 

    public void RecieveOrder()
    {  
        _switchState(State.Eating);
        GameManager.Instance.UpdateBalance(increase: Order.Price * PriceMutiplier);
        OnRecieveOrder?.Invoke();
    } 

    public void AssingChair(Chair chair)
    {
        AssingedChair = chair;
        _startAgent(destination: chair.gameObject.transform.position);
    }

    private void _sitDown()
    {
        _stopAgent();
        _lastPositionBeforeSittingDown = gameObject.transform.position;
        AssingedChair.TakeSit(sitter: this);
        _switchState(State.WaitingForOrder);
    }

    private void _stopAgent()
    {
        _switchState(State.Idle);
        _navMehAgent.ResetPath();
        _navMehAgent.isStopped = true;
        _navMehAgent.enabled = false;
    }

    private void _startAgent(Vector3 destination)
    {
        _switchState(State.Walking);
        _navMehAgent.enabled = true;
        _navMehAgent.isStopped = false;
        _navMehAgent.SetDestination(destination);
    }

    private void _switchState(State newState)
    {
        _currentState = newState;
    }
}
