using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class QuickAccessUI : NetworkBehaviour
{
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _joinButton;
    [SerializeField] private GameObject _holder;

    private void Awake()
    {
        _hostButton.onClick.AddListener(() => _host());
        _joinButton.onClick.AddListener(() => _join());
    }

    private void _host()
    {
        NetworkManager.Singleton.StartHost();
        _holder.SetActive(false);
    }

    private void _join()
    {
        NetworkManager.Singleton.StartClient();
        _holder.SetActive(false);
    }
}
