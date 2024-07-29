using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostOrJoinUI : MonoBehaviour
{
    [SerializeField] private Button _createLobbyButton;
    [SerializeField] private Button _quickJoinButton;
    [SerializeField] private Button _joinByCodeButton;
    [SerializeField] private TMP_InputField _codeInputField;
    [SerializeField] private TMP_InputField _playerNameInputField;
    [SerializeField] private Button _closeWindowButton;
    [SerializeField] private GameObject _createLobbyWindow;

    private void Start()
    {
        _createLobbyButton.onClick.AddListener(() => _showCreateLobby());
        _quickJoinButton.onClick.AddListener(() => LobbiesListManager.Instance.QuickJoinLobby());
        _joinByCodeButton.onClick.AddListener(() => _joinLobbyUsingCode());
        _closeWindowButton.onClick.AddListener(() => _hideCreateLobby());

        _hideCreateLobby();

        _playerNameInputField.text = LobbyDataManager.Instance.GetLocalPlayerName();
        _playerNameInputField.onValueChanged.AddListener((string value) => LobbyDataManager.Instance.SetPlayerName(value));
    }

    private void _showCreateLobby() => _createLobbyWindow.SetActive(true);
    
    private void _hideCreateLobby() => _createLobbyWindow.SetActive(false);
    
    private void _joinLobbyUsingCode()
    {
        string code = _codeInputField.text;

        if (code.Length > 0)
        {
            LobbiesListManager.Instance.JoinLobbyByCode(code);
        }
    }
}
