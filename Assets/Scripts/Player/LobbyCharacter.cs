using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LobbyCharacter : MonoBehaviour
{
    [SerializeField] private int _playerIndex;
    [SerializeField] private GameObject _playerReadyText;    
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private LobbyCharacterVisual _visual;
    [SerializeField] private ColorPickersSO _colorPickers;

    private void Start()
    {
        LobbyManager.Instance.OnConnectedPlayersCountChange += _updateVisibilityBasedOnConnectionStatus;
        LobbyManager.Instance.OnConnectedPlayersCountChange += _updateIsReadyTextByClientId;
        LobbyManager.Instance.OnPlayerColorChange += _syncVisualsColor;
        PlayerReadyManager.Instance.OnPlayerReady += _updateIsReadyTextByClientId;

        _updateIsReadyText(isReady: false);
        _updateVisibilityBasedOnConnectionStatus();

        foreach(SingleColorPickerUI picker in _colorPickers.ColorPickersUI)
        {
            picker.OnColorSelect += (Color color) => _handleSelectColor(color);
        }
    }

    private void _syncVisualsColor(int playerIndex, Color color)
    {
        if (_playerIndex != playerIndex)
        {
            return;
        }

        if (_isPLayerLocal())
        {
            return;
        }

        _visual.AssignColor(color);
    }

    private void _handleSelectColor(Color color)
    {
        if (!_isPLayerConnected())
        {
            return;
        }

        if (!_isPLayerLocal())
        {
            return;
        }

        if (!LobbyManager.Instance.IsColorAvailible(color))
        {
            return;
        }

        LobbyManager.Instance.SetPlayerColor(_playerIndex, color);
        
        SelectCharacterColor(color);
    }

    public void SelectCharacterColor(Color color)
    {
        _visual.AssignColor(color);
        _colorPickers.SelectPickerByColor(color);
    }

    private void _updateVisibilityBasedOnConnectionStatus()
    {
        if (_isPLayerConnected())
        {
            _show();
            Color charColor = LobbyManager.Instance.GetClientColorByIndex(_playerIndex);
            string playerName = LobbyManager.Instance.GetPlayerName();
            _setPlayerName(playerName);

            if (_isPLayerLocal())
            {
                SelectCharacterColor(charColor);
            }
            else
            {
                _visual.AssignColor(charColor);
            }
        }
        else
        {
            _hide();
        }
    }

    private bool _isPLayerConnected() {
        return _playerIndex < LobbyManager.Instance.GetConnectedPlayersCount();
    }

    private bool _isPLayerLocal() {
        return NetworkManager.Singleton.LocalClientId == LobbyManager.Instance.GetClientIdByIndex(_playerIndex);
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
        if (!_isPLayerConnected())
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

    private void _setPlayerName(string playerName)
    {
        _playerNameText.text = playerName;
    }

    private void OnDestroy()
    {
        LobbyManager.Instance.OnConnectedPlayersCountChange -= _updateVisibilityBasedOnConnectionStatus;
        LobbyManager.Instance.OnConnectedPlayersCountChange -= _updateIsReadyTextByClientId;
        LobbyManager.Instance.OnPlayerColorChange -= _syncVisualsColor;
        PlayerReadyManager.Instance.OnPlayerReady -= _updateIsReadyTextByClientId;
    }
}
