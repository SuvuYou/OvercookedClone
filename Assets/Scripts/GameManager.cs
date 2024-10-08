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
    public event Action<bool> OnLocalPlayerPause;
    public event Action<float> OnCountdownTimerChange;

    private Dictionary<ulong, bool> _playersReadyStatus = new();
    private Dictionary<ulong, bool> _playersPauseStatus = new();
    private NetworkVariable<GameState> _state = new (value: GameState.Waiting);
    public GameState State { get { return _state.Value; } }

    private TimingTimer _countdownTimer = new (defaultTimerValue: 3f);
    public bool IsPaused { get; private set; } = false;
    public bool IsLocalPaused { get; private set; } = false;

    [SerializeField] private PlayerController _playerPrefab;
    [SerializeField] private List<KitchenItemSO> _allowedIngredients;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Plate.InitAllowedIngridients(_allowedIngredients);
        }
        else
        {
            Destroy(this);
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
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += _spawnPlayersOnLoad;

            NetworkManager.Singleton.OnClientConnectedCallback += _spawnPlayersOnLateJoin; 

            NetworkManager.Singleton.OnClientDisconnectCallback += (ulong disconnectedClientId) => {
                _playersPauseStatus[disconnectedClientId] = false;
                _setIsGamePausedClientRpc(_arePlayersPaused());
            };
        }
    }

    private void _spawnPlayersOnLoad(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            PlayerController player = Instantiate(_playerPrefab);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, destroyWithScene: true);
        }
    }

    private void _spawnPlayersOnLateJoin(ulong clientId)
    {
        PlayerController player = Instantiate(_playerPrefab);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, destroyWithScene: true);
    }

    public override void OnNetworkDespawn()
    {
        PlayerInput.Instance.OnPausePressed -= _setLocalPlayerPause;
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

                if (_countdownTimer.IsTimerUp())
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
                _changeStateServerRpc(GameState.Countdown);
                _triggerOnStartGameEventClientRpc();
            }

            if (State == GameState.Active)
            {
                _changeStateServerRpc(GameState.Active);
            }
        }
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
}
