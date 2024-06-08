using UnityEngine;

public class WaitingForPlayersUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _waitingForPlayersPanel;

    private void Start()
    {
        _waitingForPlayersPanel.SetActive(true);

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
