using UnityEngine;

public class WaitingForPlayersPauseUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _waitingForPlayersPanel;

    private void Start()
    {
        _waitingForPlayersPanel.SetActive(false);

        GameManager.Instance.OnPause += _updateUI;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPause -= _updateUI;
    }

    private void _updateUI(bool isPaused)
    {
        _waitingForPlayersPanel.SetActive(isPaused); 
    }
}
