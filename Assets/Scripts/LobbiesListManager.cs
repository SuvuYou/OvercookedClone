using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class LobbiesListManager : MonoBehaviour
{
    public static LobbiesListManager Instance;

    public Lobby CurrentLobby { get; private set; }

    private const int MAX_PLAYERS = 4;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);

            return;
        }

        Instance = this;

        InitUnityServices();
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
            options.SetProfile(UnityEngine.Random.Range(0, 100).ToString());

            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public void CreateLobby(string lobbyName, bool isPrivate)
    {
        TryCatchWrapper(() => _createLobby(lobbyName, isPrivate));
    }

    private async Task _createLobby(string lobbyName, bool isPrivate)
    {
        CreateLobbyOptions options = new() { IsPrivate = isPrivate };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MAX_PLAYERS, options);

        CurrentLobby = lobby;

        LobbyManager.Instance.StartHost();
    }

    public void JoinLobbyByCode(string lobbyCode)
    {
        TryCatchWrapper(() => _joinLobbyByCode(lobbyCode));
    }

    private async Task _joinLobbyByCode(string lobbyCode)
    {
        JoinLobbyByCodeOptions options = new();

        Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);

        CurrentLobby = lobby;
    }

    public void QuickJoinLobby()
    {
        TryCatchWrapper(() => _quickJoinLobby());
    }

    private async Task _quickJoinLobby()
    {
        QuickJoinLobbyOptions options = new();

        Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);

        CurrentLobby = lobby;

        LobbyManager.Instance.StartClient();
    }

    private async void TryCatchWrapper(Func<Task> func)
    {
        try
        {
            await func();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}