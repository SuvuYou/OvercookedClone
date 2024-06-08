using UnityEngine;

public class TutorialUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _tutorialPanel;

    private void Start()
    {
        _tutorialPanel.SetActive(true);

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
