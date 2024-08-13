using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyListUI : MonoBehaviour
{
    [SerializeField] private SingleLobbyOptionUI _lobbyOptionPrefab;
    [SerializeField] private GameObject _lobbiesContainer;
    [SerializeField] private AutoSetHeightListUI _listHeightManager;
    private const float GAP_SIZE = 12f;

    private void Start()
    {
        LobbiesListManager.Instance.OnLobbiesQuery += _displayLobbies;
    }

    private void OnDestroy()
    {
        LobbiesListManager.Instance.OnLobbiesQuery -= _displayLobbies;
    }

    private void _displayLobbies(List<Lobby> lobbies)
    {
        _destroyChildren();
        _instantiateLobbyItems(lobbies);

        if (_listHeightManager != null && _lobbyOptionPrefab.TryGetComponent(out RectTransform childRect))
        {
            _listHeightManager.SetHeight(childElement: childRect, elementsCount: lobbies.Count, gapSize: GAP_SIZE);
        }
    }

    private void _destroyChildren()
    {
        foreach (Transform child in _lobbiesContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void _instantiateLobbyItems(List<Lobby> lobbies)
    {
        foreach (Lobby lobby in lobbies)
        {
            SingleLobbyOptionUI lobbyUI = Instantiate(_lobbyOptionPrefab, _lobbiesContainer.transform); 
            lobbyUI.Init(lobby);
        }
    }
}
