using UnityEngine;

public class GameStateUIManager : MonoBehaviour
{
    [SerializeField] private PauseUIManager _pauseUI;
    [SerializeField] private SettingsUIManager _settingsUI;

    private void Start()
    {
        _pauseUI.gameObject.SetActive(false);
        _settingsUI.gameObject.SetActive(false);

        GameManager.Instance.OnPause += _updateUIOnPause;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPause -= _updateUIOnPause;
    }

    private void _updateUIOnPause(bool isPaused)
    {
        _pauseUI.gameObject.SetActive(isPaused);

        if (_settingsUI.gameObject.activeSelf && !isPaused)
        {
            _settingsUI.gameObject.SetActive(false);
        }

        if (_pauseUI.gameObject.activeSelf && isPaused)
        {
            _pauseUI.gameObject.SetActive(false);
            _settingsUI.gameObject.SetActive(true);
        }
    }
}

