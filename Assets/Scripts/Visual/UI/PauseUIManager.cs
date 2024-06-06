using UnityEngine;
using UnityEngine.UI;

public class PauseUIManager : MonoBehaviour
{
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private SettingsUIManager _settingsUIManager;

    private void Start()
    {
        _pausePanel.SetActive(false);

        _resumeButton.onClick.AddListener(() => 
        {
            _pausePanel.SetActive(false);
            GameManager.Instance.UnPauseGame();
        });
        _settingsButton.onClick.AddListener(() => 
        {
            _settingsUIManager.DisplaySettingsUI();
            _pausePanel.SetActive(false);
        });
        _mainMenuButton.onClick.AddListener(() => 
        {
            GameManager.Instance.UnPauseGame();
            SceneLoader.LoadScene(Scene.MainMenu);
        });

        PlayerInput.Instance.OnPausePressed += _handleVisibility;
    }

    private void OnDestroy ()
    {
        _resumeButton.onClick.RemoveAllListeners(); 
        _settingsButton.onClick.RemoveAllListeners(); 
        _mainMenuButton.onClick.RemoveAllListeners(); 

        PlayerInput.Instance.OnPausePressed -= _handleVisibility;
    }

    private void _handleVisibility()
    {
        if (_pausePanel.activeSelf)
        {
            _pausePanel.SetActive(false);
        }
        else
        {
            DisplayPauseUI();
        }
    }

    public void DisplayPauseUI()
    {
        _pausePanel.SetActive(true);
        _resumeButton.Select();
    }
}
