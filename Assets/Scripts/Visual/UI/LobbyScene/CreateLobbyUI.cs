using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyUI : MonoBehaviour
{
    [SerializeField] private Button _createPrivateLobbyButton;
    [SerializeField] private Button _createPublicLobbyButton;
    [SerializeField] private TMP_InputField _lobbyNameInputField;

    private void Awake()
    {
        _createPrivateLobbyButton.onClick.AddListener(() => _createLobby(isPrivate: true));
        _createPublicLobbyButton.onClick.AddListener(() => _createLobby(isPrivate: false));
    }

    private void _createLobby(bool isPrivate)
    {
        string lobbyName = _lobbyNameInputField.text;

        if (lobbyName.Length > 0)
        {
            LobbiesListManager.Instance.CreateLobby(lobbyName, isPrivate);
        }
    }
}
