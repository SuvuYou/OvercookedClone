using UnityEngine;

public class TutorialUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _tutorialPanel;

    private void Start()
    {
        bool isGameWaiting = GameManager.Instance.State == GameState.Waiting;
        _tutorialPanel.SetActive(isGameWaiting ? true : false);

        GameManager.Instance.OnLocalPlayerReady += _disableUI;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnLocalPlayerReady -= _disableUI;
    }

    private void _disableUI()
    {
        _tutorialPanel.SetActive(false); 
    }
}
