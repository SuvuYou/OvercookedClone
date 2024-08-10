using System;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    public event Action OnTryingToJoin;
    public event Action OnFailedToJoin;

    private TimingTimer _joinTimer = new (defaultTimerValue: 5f);
    private bool _isTryingToJoin = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);

            return;
        }

        Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (_isTryingToJoin) 
        {
            _joinTimeout();
        }
    }

    private void _joinTimeout()
    {
        _joinTimer.SubtractTime(Time.deltaTime);

        if (_joinTimer.IsTimerUp())
        {
            _joinTimer.ResetTimer();
            _isTryingToJoin = false;

            if (!NetworkManager.IsConnectedClient)
            {
                _triggerOnFailedToJoin();
                ShutNetworkManagerDown();
            }
        }   
    }
    
    public void StartHost()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += LobbyDataManager.Instance.AddLobbyPlayerData;
        NetworkManager.Singleton.OnClientDisconnectCallback += LobbyDataManager.Instance.RemoveLobbyPlayerData;

        NetworkManager.Singleton.StartHost();
        SceneLoader.LoadSceneOnNetwork(Scene.PlayerSelectScene);
    }

    public void StartClient()
    {
        OnTryingToJoin?.Invoke();

        NetworkManager.Singleton.OnClientDisconnectCallback += _triggerOnFailedToJoin;
        NetworkManager.Singleton.StartClient();
        _isTryingToJoin = true;
    }

    private void _triggerOnFailedToJoin(ulong _ = 0)
    {
        _isTryingToJoin = false;
        OnFailedToJoin?.Invoke();
    }

    public void ShutNetworkManagerDown()
    {
        _disposeEvents();
        LobbiesListManager.Instance.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
    } 

    private void _disposeEvents()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= _triggerOnFailedToJoin;
        NetworkManager.Singleton.OnClientConnectedCallback -= LobbyDataManager.Instance.AddLobbyPlayerData;
        NetworkManager.Singleton.OnClientDisconnectCallback -= LobbyDataManager.Instance.RemoveLobbyPlayerData;
    }

    public override void OnNetworkDespawn()
    {
        _disposeEvents();
    }
}