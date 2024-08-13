using TMPro;
using UnityEngine;

public class LobbyActionOverlayUI : MonoBehaviour
{
    private const string CREATING_LOBBY_TEXT = "CREATING LOBBY...";
    private const string JOINING_LOBBY_TEXT = "JOINING LOBBY...";

    [SerializeField] private GameObject _overlay;
    [SerializeField] private TMP_Text _overlayText;

    private void Start()
    {
        _hideOverlay();

        LobbiesListManager.Instance.OnAsyncActionFailed += _hideOverlay;
        LobbiesListManager.Instance.OnLobbyCreated += _hideOverlay;
        LobbiesListManager.Instance.OnLobbyJoined += _hideOverlay;

        LobbiesListManager.Instance.OnLobbyCreate += _showCreateLobby;
        LobbiesListManager.Instance.OnLobbyJoin += _showJoinLobby;
    }

    private void OnDestroy()
    {
        LobbiesListManager.Instance.OnAsyncActionFailed -= _hideOverlay;
        LobbiesListManager.Instance.OnLobbyCreated -= _hideOverlay;
        LobbiesListManager.Instance.OnLobbyJoined -= _hideOverlay;

        LobbiesListManager.Instance.OnLobbyCreate -= _showCreateLobby;
        LobbiesListManager.Instance.OnLobbyJoin -= _showJoinLobby;  
    }

    private void _showOverlay() => _overlay.SetActive(true);
    private void _hideOverlay() => _overlay.SetActive(false);

    private void _showJoinLobby()
    {
        _overlayText.text = JOINING_LOBBY_TEXT;
        _showOverlay();
    }

    private void _showCreateLobby()
    {
        _overlayText.text = CREATING_LOBBY_TEXT;
        _showOverlay();
    }
}
