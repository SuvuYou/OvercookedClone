using TMPro;
using UnityEngine;

public class LobbyDisplayDataUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _lobbyNameText;
    [SerializeField] private TMP_Text _lobbyJoinCodeText;

    private void Awake()
    {
        _lobbyNameText.text = "Lobby Name: " + LobbiesListManager.Instance.CurrentLobby.Name;
        _lobbyJoinCodeText.text = "Lobby JoinCode: " + LobbiesListManager.Instance.CurrentLobby.LobbyCode;
    }

}
