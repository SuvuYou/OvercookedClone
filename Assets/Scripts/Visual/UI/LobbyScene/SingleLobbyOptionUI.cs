
using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class SingleLobbyOptionUI : MonoBehaviour
{
    [SerializeField] private Button _joinButton;
    [SerializeField] private TMP_Text _lobbyNameText;

    public void Init(Lobby lobby)
    {
        _lobbyNameText.text = lobby.Name;
        _joinButton.onClick.AddListener(() => LobbiesListManager.Instance.JoinLobbyById(lobby.Id));
    }
}
