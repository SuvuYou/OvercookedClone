using UnityEngine;
using UnityEngine.UI;

public class ConnectionFailedUI : MonoBehaviour
{
    [SerializeField] private GameObject _contentWindow;
    [SerializeField] private Button _closeButton;

    private void Awake()
    {
        _contentWindow.SetActive(false);
    }

    private void Start()
    {
        LobbyManager.Instance.OnFailedToJoin += _show;
        _closeButton.onClick.AddListener(() => _hide());
    }

    private void OnDestroy()
    {
        LobbyManager.Instance.OnFailedToJoin -= _show;
    }

    private void _show () => _contentWindow.SetActive(true);
    private void _hide () => _contentWindow.SetActive(false);
}
