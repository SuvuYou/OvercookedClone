using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HostDisconectedUIManager : MonoBehaviour
{
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private GameObject _panel;
    [SerializeField] private bool _isLobby = false;

    private void Start()
    {
        _panel.SetActive(false);

        _mainMenuButton.onClick.AddListener(() => 
        {
            LobbyManager.Instance.ShutNetworkManagerDown();
            if (!_isLobby)
            {
                GameManager.Instance.UnPauseGame(isDisconecting: true);
            }
            SceneLoader.LoadScene(Scene.MainMenu);
        });

        NetworkManager.Singleton.OnClientDisconnectCallback += _displayOnDisconnect;
    }

    private void _displayOnDisconnect(ulong disconectedClientId) 
    {
        if (disconectedClientId == NetworkManager.ServerClientId)
        {
            _panel.SetActive(true);
        }
    }

    private void OnDestroy ()
    {
        _mainMenuButton.onClick.RemoveAllListeners(); 
        NetworkManager.Singleton.OnClientDisconnectCallback -= _displayOnDisconnect;
    }
}
