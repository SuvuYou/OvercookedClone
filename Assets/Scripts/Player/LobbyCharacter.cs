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
        LobbyDataManager.Instance.OnConnectedPlayersDataChange += _updateVisibilityBasedOnConnectionStatus;
        LobbyDataManager.Instance.OnPlayerColorChange += _syncVisualsColor;
        PlayerReadyManager.Instance.OnPlayerReady += _updateIsReadyText;

        _setIsReadyTextVisibility(isReady: false);
        _updateVisibilityBasedOnConnectionStatus();

        foreach(SingleColorPickerUI picker in _colorPickers.ColorPickersUI)
        {
            picker.OnColorSelect += (Color color) => _handleSelectColor(color);
        }
    }

    private void _syncVisualsColor(int playerIndex, Color color)
    {
        if (_playerIndex != playerIndex || _isPLayerLocal())
        {
            return;
        }

        _visual.AssignColor(color);
    }

    private void _handleSelectColor(Color color)
    {
        if (
            !_isPLayerConnected() || 
            !_isPLayerLocal() ||
            !LobbyDataManager.Instance.IsColorAvailable(color)
            )
        {
            return;
        }

        LobbyDataManager.Instance.SetPlayerColor(_playerIndex, color);
        
        _visual.AssignColor(color);
        _colorPickers.SelectPickerByColor(color);
    }

    private void _updateVisibilityBasedOnConnectionStatus()
    {
        if (_isPLayerConnected())
        {
            _show();
            _updateCharacterVisual();
            _updateIsReadyText();
        }
        else
        {
            _hide();
        }
    }

    private void _updateCharacterVisual()
    {
        LobbyPlayerData playerData = LobbyDataManager.Instance.GetPlayerDataByIndex(_playerIndex);

        _setPlayerName(playerData.GetPlayerName());

        _visual.AssignColor(playerData.CharacterColor);

        if (_isPLayerLocal())
        {
            _colorPickers.SelectPickerByColor(playerData.CharacterColor);
        }
    }

    private bool _isPLayerConnected() {
        return _playerIndex < LobbyDataManager.Instance.GetConnectedPlayersCount();
    }

    private bool _isPLayerLocal() {
        return NetworkManager.Singleton.LocalClientId == LobbyDataManager.Instance.GetPlayerDataByIndex(_playerIndex).ClientId;
    }

    private void _show()
    {
        gameObject.SetActive(true);
    }

    private void _hide()
    {
        gameObject.SetActive(false);
    }

    private void _updateIsReadyText()
    {
        if (!_isPLayerConnected())
        {
            return;
        }

        ulong clientId = LobbyDataManager.Instance.GetPlayerDataByIndex(_playerIndex).ClientId;
        _setIsReadyTextVisibility(isReady: PlayerReadyManager.Instance.IsPlayerReady(clientId));
    }

    private void _setIsReadyTextVisibility(bool isReady) => _playerReadyText.SetActive(isReady);
    private void _setPlayerName(string playerName) => _playerNameText.text = playerName;
    
    private void OnDestroy()
    {
        LobbyDataManager.Instance.OnConnectedPlayersDataChange -= _updateVisibilityBasedOnConnectionStatus;
        LobbyDataManager.Instance.OnPlayerColorChange -= _syncVisualsColor;
        PlayerReadyManager.Instance.OnPlayerReady -= _updateIsReadyText;
    }
}
