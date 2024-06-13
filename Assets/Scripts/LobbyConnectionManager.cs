using System;
using Unity.Netcode;

public class LobbyConnectionManager : NetworkBehaviour
{
    public static LobbyConnectionManager Instance;

    private void Awake ()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);

            return;
        }

        Instance = this;
    }

    public event Action OnTryingToJoin;
    public event Action OnFailedToJoin;

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        SceneLoader.LoadSceneOnNetwork(Scene.PlayerSelectScene);
    }

    public void StartClient()
    {
        OnTryingToJoin?.Invoke();

        NetworkManager.Singleton.OnClientDisconnectCallback += _triggerOnFailedToJoin;
        NetworkManager.Singleton.StartClient();
    }

    public override void OnDestroy()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= _triggerOnFailedToJoin;
    }

    private void _triggerOnFailedToJoin(ulong clientId)
    {
        OnFailedToJoin?.Invoke();
    }
}
