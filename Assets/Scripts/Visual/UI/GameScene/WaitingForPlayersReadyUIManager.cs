using UnityEngine;

public class WaitingForPlayersReadyUIManagery : MonoBehaviour
{
    [SerializeField] private GameObject _waitingForPlayersPanel;

    private void Start()
    {
        bool isGameWaiting = GameManager.Instance.State == GameState.Waiting;
        _waitingForPlayersPanel.SetActive(isGameWaiting ? true : false);

        GameManager.Instance.OnStartGame += _disableUI;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnStartGame -= _disableUI;
    }

    private void _disableUI()
    {
        _waitingForPlayersPanel.SetActive(false); 
    }
}
