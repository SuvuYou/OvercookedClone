using UnityEngine;

public class LobbyCharacter : MonoBehaviour
{
    [SerializeField] private int _playerIndex;
    [SerializeField] private GameObject _playerReadyText;

    private void Start()
    {
        _updateIsReadyText(isReady: false);

        LobbyManager.Instance.OnConnectedPlayersCountChange += _updateVisibilityBasedOnConnectionStatus;
        PlayerReadyManager.Instance.OnPlayerReady += _updateIsReadyTextByClientId;

        _updateVisibilityBasedOnConnectionStatus();
    }

    private void _updateVisibilityBasedOnConnectionStatus()
    {
        if (_isPLayerConnected())
        {
            _show();
        }
        else
        {
            _hide();
        }
    }

    private bool _isPLayerConnected() {
        return _playerIndex < LobbyManager.Instance.GetConnectedPlayersCount();
    }

    private void _show()
    {
        gameObject.SetActive(true);
    }

    private void _hide()
    {
        gameObject.SetActive(false);
    }

    private void _updateIsReadyTextByClientId()
    {
        if (!LobbyManager.Instance.IsPlayerIndexConnected(_playerIndex))
        {
            return;
        }

        ulong clientId = LobbyManager.Instance.GetClientIdByIndex(_playerIndex);
        _updateIsReadyText(isReady: PlayerReadyManager.Instance.IsPlayerReady(clientId));
    }

    private void _updateIsReadyText(bool isReady)
    {
        _playerReadyText.SetActive(isReady);
    }

}
