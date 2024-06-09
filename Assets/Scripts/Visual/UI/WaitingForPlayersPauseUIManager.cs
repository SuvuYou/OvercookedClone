using UnityEngine;

public class WaitingForPlayersPauseUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _waitingForPlayersPanel;

    private void Start()
    {
        _waitingForPlayersPanel.SetActive(false);

        GameManager.Instance.OnPause += _updateUI;
        GameManager.Instance.OnLocalPlayerPause += _updateUIOnLocalPause;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPause -= _updateUI;
        GameManager.Instance.OnLocalPlayerPause += _updateUIOnLocalPause;
    }

    private void _updateUI(bool isPaused)
    {
        _waitingForPlayersPanel.SetActive(isPaused && !GameManager.Instance.IsLocalPaused); 
    }

    private void _updateUIOnLocalPause(bool isLocalPaused)
    {
        _waitingForPlayersPanel.SetActive(GameManager.Instance.IsPaused && !isLocalPaused); 
    }
}
