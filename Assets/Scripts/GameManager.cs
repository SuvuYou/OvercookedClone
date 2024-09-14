using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

// TODO LIST:
// Head textures and more colors

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

    private const float PROGRESS_GOAL_PER_DAY = 400f;

    public event Action<GameState> OnStateChange;
    public event Action<bool> OnPause;
    public event Action OnStartGame;
    public event Action OnLocalPlayerReady;
    public event Action<bool> OnLocalPlayerPause;
    public event Action<float> OnCountdownTimerChange;
    public event Action<float> OnBalanceUpdated;
    public event Action<int, float> OnStartDay;
    public event Action OnEndDay;

    private Dictionary<ulong, bool> _playersReadyStatus = new();
    private Dictionary<ulong, bool> _playersPauseStatus = new();
    private NetworkVariable<GameState> _state = new (value: GameState.Waiting);
    public GameState State { get { return _state.Value; } }

    public int CurrentDay { get; private set; } = 1;

    private NetworkVariable<float> _balance = new();
    public float Balance { get => _balance.Value; }

    public NetworkVariable<float> CurrentDayProgress { get; private set; } = new();

    private float _currentDayProgressGoal;
    public float CurrentDayProgressGoal { get => _currentDayProgressGoal; }

    private TimingTimer _countdownTimer = new (defaultTimerValue: 3f);
    private float _currentCountdownNumber;
    private TimingTimer _dayTimer = new (defaultTimerValue: 90f); 
    public bool IsPaused { get; private set; } = false;
    public bool IsLocalPaused { get; private set; } = false;

    [SerializeField] private PlayerController _playerPrefab;
    [SerializeField] private List<KitchenItemSO> _allowedIngredients;
    [SerializeField] private VirtualCamera _virtualCamera;   

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Plate.InitAllowedIngridients(_allowedIngredients);
            _balance.OnValueChanged += (float prevValue, float newValue) => OnBalanceUpdated?.Invoke(newValue);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        PlayerInput.Instance.OnPausePressed += _setLocalPlayerPause;
        PlayerInput.Instance.OnInteractDuringWaitingState += _setLocalPlayerReady;  

        if (State != GameState.Waiting)
        {
            _setLocalPlayerReady();
        }

        if (IsServer)
        {
            DataPersistanceManager.Instance.OnSaveGameData += _onSaveData;

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += _spawnPlayersOnLoad;

            // handle spawn player on late join
            NetworkManager.Singleton.OnClientConnectedCallback += _spawnPlayer; 

            NetworkManager.Singleton.OnClientDisconnectCallback += (ulong disconnectedClientId) => {
                _playersPauseStatus[disconnectedClientId] = false;
                _setIsGamePausedClientRpc(_arePlayersPaused());
            };

            DataPersistanceManager.Instance.OnLoadGameData += (GameData data) => _balance.Value = data.Balance;

            OnEndDay += _destroyAllKitchenItemParentsItems; 
            OnEndDay += () => DataPersistanceManager.Instance.SaveData();
        }
    }

    private void _onSaveData(GameData currentData, Action<GameData> saveData) 
    {
        currentData.Balance = Balance; 
        currentData.CurrentDay = CurrentDay; 
        saveData(currentData);
    }

    [ClientRpc]
    private void _setCameraTargetClientRpc(ulong clientId, NetworkObjectReference localPlayerNetworkObject)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        if (localPlayerNetworkObject.TryGet(out NetworkObject netObj) && netObj.TryGetComponent(out Transform playerTransform))
        {
            _virtualCamera.SetCameraTarger(target: playerTransform);
        }
    } 

    private void _spawnPlayersOnLoad(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            _spawnPlayer(clientId);
        }

        DataPersistanceManager.Instance.LoadData();  
    }

    private void _spawnPlayer(ulong clientId)
    {
        PlayerController player = Instantiate(_playerPrefab);
        NetworkObject playerNetworkObject = player.GetComponent<NetworkObject>();
        playerNetworkObject.SpawnAsPlayerObject(clientId, destroyWithScene: true);

        _setCameraTargetClientRpc(clientId, playerNetworkObject);
    }

    public override void OnNetworkDespawn()
    {
        PlayerInput.Instance.OnPausePressed -= _setLocalPlayerPause;
        PlayerInput.Instance.OnInteractDuringWaitingState -= _setLocalPlayerReady;
        OnEndDay -= _destroyAllKitchenItemParentsItems; 
    }   

    private void Update()
    {
        if (!IsServer) return;

        switch(State)
        {
            case GameState.Waiting:
                break;
            case GameState.Countdown:
                _countdownTimer.SubtractTime(Time.deltaTime);

                if (_currentCountdownNumber != Math.Ceiling(_countdownTimer.Time))
                {
                    _triggerOnCountdownEventClientRpc(_countdownTimer.Time);
                    _currentCountdownNumber = (float) Math.Ceiling(_countdownTimer.Time);
                }
                
                if (_countdownTimer.IsTimerUp())
                {
                    _countdownTimer.ResetTimer();
                    _changeStateServerRpc(GameState.Active);
                }
                break;
            case GameState.Active:
                _dayTimer.SubtractTime(Time.deltaTime);

                if (_dayTimer.IsTimerUp())
                {
                    _dayTimer.ResetTimer();
                    _changeStateServerRpc(GameState.Editing);
                }
                break;
            case GameState.GameOver:
                break;
            case GameState.Editing:
                break;
        }
    }

    public void StartNewDay(int newDayCount)
    {
        CurrentDay = newDayCount;
        _triggerStartDayServerRpc(daysCount: newDayCount, timer: _dayTimer.Time);
        _setCurrentDayProgressGoalServerRpc(newDayCount * PROGRESS_GOAL_PER_DAY);
        _resetCurrentDayProgressServerRpc();
        _changeStateServerRpc(GameState.Active);
    }

    [ServerRpc]
    private void _triggerStartDayServerRpc(int daysCount, float timer) => _triggerStartDayClientRpc(daysCount, timer);

    [ClientRpc]
    private void _triggerStartDayClientRpc(int daysCount, float timer) => OnStartDay?.Invoke(daysCount, timer);

    [ServerRpc]
    private void _changeStateServerRpc(GameState newState)
    {
        if (_state.Value == GameState.Active) _triggerOnEndDayEventClientRpc();
        
        _state.Value = newState;
        _triggerOnChangeStateEventClientRpc(newState);
    }

    private void _setLocalPlayerPause()
    {
        IsLocalPaused = !IsLocalPaused;

        OnLocalPlayerPause?.Invoke(IsLocalPaused);

        _setPlayerPauseServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void _setPlayerPauseServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong playerId = rpcParams.Receive.SenderClientId; 

        if (!_playersPauseStatus.Keys.Contains(playerId)) _playersPauseStatus[playerId] = true;
        else _playersPauseStatus[playerId] = !_playersPauseStatus[playerId];

        bool newIsGamePaused = _arePlayersPaused();

        _setIsGamePausedClientRpc(newIsGamePaused);
    }

    [ClientRpc]
    private void _setIsGamePausedClientRpc(bool newIsGamePaused)
    {
        if (IsPaused != newIsGamePaused)
        {
            Time.timeScale = newIsGamePaused ? 0 : 1;
        }

        OnPause?.Invoke(newIsGamePaused);
        IsPaused = newIsGamePaused;
    }

    public void UnPauseGame(bool isDisconecting = false)
    {
        _setLocalPlayerPause();

        if (isDisconecting)
        {
            Time.timeScale = 1;
        }
    }

    private void _setLocalPlayerReady()
    {
        _setPlayerReadyServerRpc();

        OnLocalPlayerReady?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    private void _setPlayerReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        _playersReadyStatus[rpcParams.Receive.SenderClientId] = true;

        if (_areAllPlayersReady())
        {
            if (State == GameState.Waiting)
            {
                StartNewDay(newDayCount: 1);
                _triggerOnStartGameEventClientRpc();
            }
        }
    }

    [ClientRpc]
    private void _triggerOnStartGameEventClientRpc() => OnStartGame?.Invoke();
    
    [ClientRpc]
    private void _triggerOnChangeStateEventClientRpc(GameState newState) => OnStateChange?.Invoke(newState);

    [ClientRpc]
    private void _triggerOnEndDayEventClientRpc() => OnEndDay?.Invoke();

    [ClientRpc]
    private void _triggerOnCountdownEventClientRpc(float countdownTime)
    {
        OnCountdownTimerChange?.Invoke(countdownTime);
    }

    private bool _areAllPlayersReady()
    {
        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            if (!_playersReadyStatus.Keys.Contains(clientId) || !_playersReadyStatus[clientId])
            {
                return false;
            }
        }

        return true;
    }

    private bool _arePlayersPaused()
    {
        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            if (_playersPauseStatus.Keys.Contains(clientId) && _playersPauseStatus[clientId])
            {
                return true;
            }
        }

        return false;
    }

    public void RecordCurrentDayProgress(float increase)
    {
        if (IsServer)
        {
            CurrentDayProgress.Value += increase;
        }
    }  

    [ServerRpc(RequireOwnership = false)]
    private void _resetCurrentDayProgressServerRpc() => CurrentDayProgress.Value = 0;

    [ServerRpc(RequireOwnership = false)]
    private void _setCurrentDayProgressGoalServerRpc(float goal) => _setCurrentDayProgressGoalClientRpc(goal);

    [ClientRpc]
    private void _setCurrentDayProgressGoalClientRpc(float goal) => _currentDayProgressGoal = goal;

    public void UpdateBalance(float increase = 0, float decrease = 0)
    {
        float balanceDifference = increase - decrease;

        _setBalanceServerRpc(balanceDifference);
    }  

    [ServerRpc(RequireOwnership = false)]
    private void _setBalanceServerRpc(float balanceDifference) => _balance.Value += balanceDifference;

    private void _destroyAllKitchenItemParentsItems()
    {
        var kitchenItemParents = FindObjectsOfType<KitchenItemParent>();

        foreach (KitchenItemParent kitchenItemParent in kitchenItemParents)
        {
            if (kitchenItemParent.IsHoldingItem())
            {
                kitchenItemParent.DestroyCurrentItemHeld();
            }
        }
    }
}
