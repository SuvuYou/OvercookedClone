using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HostDisconectedUIManager : MonoBehaviour
{
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private GameObject _panel;

    private void Start()
    {
        _panel.SetActive(false);

        _mainMenuButton.onClick.AddListener(() => 
        {
            NetworkManager.Singleton.Shutdown();
            GameManager.Instance.UnPauseGame(isDisconecting: true);
            SceneLoader.LoadScene(Scene.MainMenu);
        });

        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong disconectedClientId) => {
            if (disconectedClientId == NetworkManager.ServerClientId)
            {
                _panel.SetActive(true);
            }
        };
    }

    private void OnDestroy ()
    {
        _mainMenuButton.onClick.RemoveAllListeners(); 
    }
}
