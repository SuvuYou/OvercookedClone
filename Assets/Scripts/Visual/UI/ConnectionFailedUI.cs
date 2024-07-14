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
        LobbyManager.Instance.OnFailedToJoin += () => _contentWindow.SetActive(true);
        _closeButton.onClick.AddListener(() => _contentWindow.SetActive(false));
    }
}
