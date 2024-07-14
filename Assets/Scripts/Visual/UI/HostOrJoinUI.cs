using UnityEngine;
using UnityEngine.UI;

public class HostOrJoinUI : MonoBehaviour
{
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _joinButton;

    private void Awake()
    {
        _hostButton.onClick.AddListener(LobbyManager.Instance.StartHost);
        _joinButton.onClick.AddListener(LobbyManager.Instance.StartClient);
    }
}
