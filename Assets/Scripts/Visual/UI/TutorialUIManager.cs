using UnityEngine;

public class TutorialUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _tutorialPanel;

    private void Start()
    {
        _tutorialPanel.SetActive(true);

        GameManager.Instance.OnStartGame += _disableUI;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnStartGame -= _disableUI;
    }

    private void _disableUI()
    {
        _tutorialPanel.SetActive(false); 
    }
}
