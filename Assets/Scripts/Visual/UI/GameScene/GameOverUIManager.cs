using UnityEngine;
using UnityEngine.UI;

public class GameOverUIManager : MonoBehaviour
{
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private GameObject _panel;

    private void Start()
    {
        _panel.SetActive(false);

        _mainMenuButton.onClick.AddListener(() => 
        {
            LobbyManager.Instance.ShutNetworkManagerDown();
            GameManager.Instance.UnPauseGame(isDisconecting: true);
            SceneLoader.LoadScene(Scene.MainMenu);
        });

        GameManager.Instance.OnEndDay += _checkIsGameOver;
    }

    private void _checkIsGameOver() 
    {
        if (GameManager.Instance.CurrentDayProgress.Value < GameManager.Instance.CurrentDayProgressGoal)
        {
            DataPersistanceManager.Instance.DeleteSaveFile();
            _panel.SetActive(true);
        }
    }

    private void OnDestroy ()
    {
        _mainMenuButton.onClick.RemoveAllListeners(); 
        GameManager.Instance.OnEndDay -= _checkIsGameOver;
    }
}
