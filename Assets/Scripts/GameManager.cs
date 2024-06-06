using System;
using UnityEngine;

public enum GameState 
{
    Waiting,
    Countdown,
    Active,
    GameOver,
    Editing,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public event Action<GameState> OnStateChange;
    public event Action<bool> OnPause;
    public event Action OnStartGame;
    public event Action<float> OnCountdownTimerChange;

    public GameState State { get; private set; } = GameState.Waiting;
    private TimingTimer _countdownTimer = new (defaultTimerValue: 3f);
    public bool IsPaused { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        PlayerInput.Instance.OnPausePressed += _pauseGame;
        PlayerInput.Instance.OnInteractWaitingMode += _startGame;
    }

    private void OnDestroy()
    {
        PlayerInput.Instance.OnPausePressed -= _pauseGame;
        PlayerInput.Instance.OnInteractWaitingMode -= _startGame;
    }   

    private void Update()
    {
        switch(State)
        {
            case GameState.Waiting:
                break;
            case GameState.Countdown:
                _countdownTimer.SubtractTime(Time.deltaTime);
                OnCountdownTimerChange?.Invoke(_countdownTimer.Timer);

                if (_countdownTimer.Timer <= 0f)
                {
                    _changeState(GameState.Active);
                    _countdownTimer.ResetTimer();
                }
                break;
            case GameState.Active:
                break;
            case GameState.GameOver:
                break;
            case GameState.Editing:
                break;
        }
    }

    private void _changeState(GameState newState)
    {
        State = newState;
        OnStateChange?.Invoke(newState);
    }

    private void _pauseGame()
    {
        IsPaused = !IsPaused;

        Time.timeScale = IsPaused ? 0 : 1;
        OnPause?.Invoke(IsPaused);
    }

    private void _startGame()
    {
        _changeState(GameState.Countdown);
        OnStartGame?.Invoke();
    }

    public void UnPauseGame()
    {
        IsPaused = false;
        Time.timeScale = 1;
        OnPause?.Invoke(IsPaused);
    }
}

struct TimingTimer
{
    public float Timer { get; private set; }
    public float DefaultTimerValue;

    public TimingTimer (float defaultTimerValue){
        Timer = defaultTimerValue;
        DefaultTimerValue = defaultTimerValue;
    }

    public void SubtractTime(float timeAmount)
    {
        Timer -= timeAmount;
    }

    public void ResetTimer()
    {
        Timer = DefaultTimerValue;
    }
}
