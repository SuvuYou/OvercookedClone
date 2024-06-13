using UnityEngine;

public class ConnectingUI : MonoBehaviour
{
    [SerializeField] private GameObject _contentWindow;

    private void Awake()
    {
        _contentWindow.SetActive(false);
    }

    private void Start()
    {
        LobbyConnectionManager.Instance.OnTryingToJoin += () => _contentWindow.SetActive(true);
        LobbyConnectionManager.Instance.OnFailedToJoin += () => _contentWindow.SetActive(false);
    }
}
