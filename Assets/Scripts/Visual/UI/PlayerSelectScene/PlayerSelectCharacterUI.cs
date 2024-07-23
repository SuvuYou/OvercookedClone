using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectCharacterUI : MonoBehaviour
{
    [SerializeField] private Button _readyButton;
    [SerializeField] private Button _mainMenuButton;

    private void Start()
    {
        _readyButton.onClick.AddListener(() => PlayerReadyManager.Instance.ToggleLocalPlayerReady());
        _mainMenuButton.onClick.AddListener(() => _loadMainScene());
    }

    private void _loadMainScene()
    {
        LobbyManager.Instance.ShutNetworkManagerDown();
        SceneLoader.LoadScene(Scene.MainMenu);
    }
}
