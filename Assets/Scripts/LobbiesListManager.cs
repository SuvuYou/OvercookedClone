using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class LobbiesListManager : MonoBehaviour
{
    public static LobbiesListManager Instance;

    public const int MAX_PLAYERS = 4;
    public const string RELAY_CODE_LOBBY_KEY = "RelayCode";

    public event Action OnLobbyCreate;
    public event Action OnLobbyCreated;

    public event Action OnLobbyJoin;
    public event Action OnLobbyJoined;

    public event Action OnAsyncActionFailed;

    public event Action<List<Lobby>> OnLobbiesQuery;

    public Lobby CurrentLobby { get; private set; }

    private TimingTimer _lobbyHeartbeatTimer = new (defaultTimerValue: 10f); 
    private TimingTimer _queryLobbiesTimer = new (defaultTimerValue: 3f); 

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);

            return;
        }

        Instance = this;

        InitUnityServices();
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        _sendLobbyHeartBeat();
        _intervalQueryLobbies();
    }

    private void _sendLobbyHeartBeat()
    {
        if (CurrentLobby == null || !_isLobbyHost()) return;

        _lobbyHeartbeatTimer.SubtractTime(Time.deltaTime);

        if (_lobbyHeartbeatTimer.IsTimerUp())
        {
            LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            _lobbyHeartbeatTimer.ResetTimer();
        }
    }

    private void _intervalQueryLobbies()
    {
        if (CurrentLobby != null) return;

        _queryLobbiesTimer.SubtractTime(Time.deltaTime);

        if (_queryLobbiesTimer.IsTimerUp())
        {
            _safeQueryLobbies();
            _queryLobbiesTimer.ResetTimer();
        }
    }

    private bool _isLobbyHost()
    {
        return CurrentLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    public void InitUnityServices()
    {
        TryCatchWrapper(() => _initUnityServices());
    }

    private async Task _initUnityServices()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions options = new();
            options.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());

            await UnityServices.InitializeAsync(options);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private async Task<string> _allocateRelay()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections: MAX_PLAYERS - 1);

        string code = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        _setUnityRelayTransport(allocation);

        return code;
    }

    private async Task _joinAllocatedRelay(string code)
    {
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(code);
        _setUnityRelayTransport(allocation);
    }

    private void _setUnityRelayTransport(Allocation allocation)
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
    }

    private void _setUnityRelayTransport(JoinAllocation allocation)
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
    }

    public void CreateLobby(string lobbyName, bool isPrivate)
    {
        TryCatchWrapper(() => _createLobby(lobbyName, isPrivate));
    }

    private async Task _setLobbyRelayCode(string lobbyId, string joinCode)
    {
        UpdateLobbyOptions updateOptions = new () 
        {
            Data = new Dictionary<string, DataObject>() 
            {
                { RELAY_CODE_LOBBY_KEY, new DataObject(visibility: DataObject.VisibilityOptions.Public, value: joinCode) }
            }
        };

        await LobbyService.Instance.UpdateLobbyAsync(lobbyId, updateOptions);
    }

    private async Task _createLobby(string lobbyName, bool isPrivate)
    {
        CreateLobbyOptions options = new() { IsPrivate = isPrivate };

        OnLobbyCreate?.Invoke();

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MAX_PLAYERS, options);

        string joinCode = await _allocateRelay();

        await _setLobbyRelayCode(lobbyId: lobby.Id, joinCode: joinCode);

        OnLobbyCreated?.Invoke();
        CurrentLobby = lobby;

        LobbyManager.Instance.StartHost();
    }

    private async Task _joinLobbyWrapper(Func<Task<Lobby>> joinLobbyFunction)
    {
        OnLobbyJoin?.Invoke();

        Lobby lobby = await joinLobbyFunction();

        await _joinAllocatedRelay(code: lobby.Data[RELAY_CODE_LOBBY_KEY].Value);
        
        OnLobbyJoined?.Invoke();

        CurrentLobby = lobby;

        LobbyManager.Instance.StartClient();
    }

    public void JoinLobbyByCode(string lobbyCode)
    {
        TryCatchWrapper(() => _joinLobbyByCode(lobbyCode));
    }

    private async Task _joinLobbyByCode(string lobbyCode)
    {
        JoinLobbyByCodeOptions options = new();

        await _joinLobbyWrapper(() => LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options));
    }

    public void JoinLobbyById(string lobbyId)
    {
        TryCatchWrapper(() => _joinLobbyById(lobbyId));
    }

    private async Task _joinLobbyById(string lobbyId)
    {
        JoinLobbyByIdOptions options = new();

        await _joinLobbyWrapper(() => LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options));
    }

    public void QuickJoinLobby()
    {
        TryCatchWrapper(() => _quickJoinLobby());
    }

    private async Task _quickJoinLobby()
    {
        QuickJoinLobbyOptions options = new();

        await _joinLobbyWrapper(() => LobbyService.Instance.QuickJoinLobbyAsync(options));
    }

    public void LeaveLobby()
    {
        TryCatchWrapper(() => _leaveLobby());
    }

    private async Task _leaveLobby()
    {
        if (CurrentLobby != null)
        {
            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId);

            CurrentLobby = null;
        }
    }

    public void _safeQueryLobbies()
    {
        TryCatchWrapper(() => _queryLobbies());
    }

    private async Task _queryLobbies()
    {
        QueryLobbiesOptions options = new () 
        {
            Filters = new List<QueryFilter> 
            {
                new (field: QueryFilter.FieldOptions.AvailableSlots, op: QueryFilter.OpOptions.GT, value: "0")
            }
        };

        QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);

        OnLobbiesQuery?.Invoke(response.Results);
    }


    private async void TryCatchWrapper(Func<Task> func)
    {
        try
        {
            await func();
        }
        catch (Exception e)
        {
            OnAsyncActionFailed?.Invoke();
            Debug.Log(e);
        }
    }
}