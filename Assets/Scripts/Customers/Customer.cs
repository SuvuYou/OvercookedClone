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
    private const float MIN_TIME_FOR_EATING = 2f;
    private const float MAX_TIME_FOR_EATING = 4f;

    public event Action OnSitDown;
    public event Action OnRecieveOrder;
    public event Action OnFinishEating;
    public event Action OnCustomerLeaving;

    public bool IsFinishedEating = false;

    private State _currentState = State.Idle;

    [SerializeField] private AvailableRecipesListSO _availableRecipesList;
    public RecipeSO Order { get; private set; }
    public Chair AssingedChair { get; private set; }

    private TimingTimer _eatingTimer = new(minDefaultTimerValue: MIN_TIME_FOR_EATING, maxDefaultTimerValue: MAX_TIME_FOR_EATING);
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

    private void _checkIsCloseToDestination()
    {
        if (!IsFinishedEating && _navMehAgent.remainingDistance < SITTING_DISTANCE_THRESHOLD)
        {
            SitDown();
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

        if (_eatingTimer.IsTimerUp())
        {
            _eatingTimer.ResetTimer();

            AssingedChair.FinishDish();
            _switchState(State.Idle);

            IsFinishedEating = true;
            OnFinishEating?.Invoke();
        }
    }

    public void MakeAnOrder()
    {
        int randomIndex = UnityEngine.Random.Range(0, _availableRecipesList.AvailableRecipes.Count);

        Order = _availableRecipesList.AvailableRecipes[randomIndex];
    } 

    public void Leave(Vector3 exitPosition)
    {
        _startAgent(destination: exitPosition);
        _switchState(State.Walking);
    }

    public bool TryRecieveOrder(Plate plate)
    {
        if (_currentState != State.WaitingForOrder) return false;

        if (!Order.Ingredients.OrderBy(ing => ing.ItemName).SequenceEqual(plate.Ingredients.OrderBy(ing => ing.ItemName))) return false;
        
        _switchState(State.Eating);
        OnRecieveOrder?.Invoke();
        
        return true;
    } 

    public void AssingChair(Chair chair)
    {
        AssingedChair = chair;
        _startAgent(destination: chair.gameObject.transform.position);
    }

    private void SitDown()
    {
        _stopAgent();
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
