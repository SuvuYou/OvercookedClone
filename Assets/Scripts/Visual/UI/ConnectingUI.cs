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
        LobbyManager.Instance.OnTryingToJoin += _show;
        LobbyManager.Instance.OnFailedToJoin += _hide;
    }

    private void OnDestroy()
    {
        LobbyManager.Instance.OnTryingToJoin -= _show;
        LobbyManager.Instance.OnFailedToJoin -= _hide;
    }

    private void _show () => _contentWindow.SetActive(true);
    private void _hide () => _contentWindow.SetActive(false);
}
