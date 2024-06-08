using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public enum GameState 
{
    Waiting,
    Countdown,
    Active,
    GameOver,
    Editing,
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public event Action<GameState> OnStateChange;
    public event Action<bool> OnPause;
    public event Action OnStartGame;
    public event Action OnLocalPlayerReady;
    public event Action<float> OnCountdownTimerChange;

    private Dictionary<ulong, bool> _platersReadyStatus = new();
    private NetworkVariable<GameState> _state = new (value: GameState.Waiting);
    public GameState State { get { return _state.Value; } }

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
        PlayerInput.Instance.OnInteractDuringWaitingState += _setLocalPlayerReady;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        PlayerInput.Instance.OnPausePressed -= _pauseGame;
        PlayerInput.Instance.OnInteractDuringWaitingState -= _setLocalPlayerReady;
    }   

    private void Update()
    {
        if (!IsServer) return;

        switch(_state.Value)
        {
            case GameState.Waiting:
                break;
            case GameState.Countdown:
                _countdownTimer.SubtractTime(Time.deltaTime);
                _triggerOnCountdownEventClientRpc(_countdownTimer.Timer);

                if (_countdownTimer.Timer <= 0f)
                {
                    _changeStateServerRpc(GameState.Active);
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

    [ServerRpc]
    private void _changeStateServerRpc(GameState newState)
    {
        _state.Value = newState;
        _triggerOnChangeStateEventClientRpc(newState);
    }

    private void _pauseGame()
    {
        IsPaused = !IsPaused;

        Time.timeScale = IsPaused ? 0 : 1;
        OnPause?.Invoke(IsPaused);
    }

    private void _setLocalPlayerReady()
    {
        _setPlayerReadyServerRpc();

        OnLocalPlayerReady?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    private void _setPlayerReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        _setPlayerReadyClientRpc(rpcParams.Receive.SenderClientId);

        if (_areAllPlayersReady())
        {
            _changeStateServerRpc(GameState.Countdown);
            _triggerOnStartGameEventClientRpc();
        }
    }

    [ClientRpc]
    private void _setPlayerReadyClientRpc(ulong playerId)
    {
        _platersReadyStatus[playerId] = true;
    }

    [ClientRpc]
    private void _triggerOnStartGameEventClientRpc()
    {
        OnStartGame?.Invoke();
    }

    [ClientRpc]
    private void _triggerOnChangeStateEventClientRpc(GameState newState)
    {
        OnStateChange?.Invoke(newState);
    }

    [ClientRpc]
    private void _triggerOnCountdownEventClientRpc(float countdownTime)
    {
        OnCountdownTimerChange?.Invoke(countdownTime);
    }

    private bool _areAllPlayersReady()
    {
        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            if (!_platersReadyStatus.Keys.Contains(clientId) || !_platersReadyStatus[clientId])
            {
                return false;
            }
        }

        return true;
    }

    public void UnPauseGame()
    {
        IsPaused = false;
        Time.timeScale = 1;
        OnPause?.Invoke(IsPaused);
    }
}
