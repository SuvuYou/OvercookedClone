using Unity.Netcode;
using UnityEngine;

public class MainMenuCleanup : MonoBehaviour
{
    private void Awake()
    {
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }

        if (LobbyManager.Instance != null)
        {
            Destroy(LobbyManager.Instance.gameObject);
        }

        if (LobbiesListManager.Instance != null)
        {
            Destroy(LobbiesListManager.Instance.gameObject);
        }

        if (DataPersistanceManager.Instance != null)
        {
            Destroy(DataPersistanceManager.Instance.gameObject);
        }
    }
}
