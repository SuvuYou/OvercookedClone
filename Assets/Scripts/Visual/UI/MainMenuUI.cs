using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _quitButton;

    private void Awake()
    {
        _startButton.Select();
        _startButton.onClick.AddListener(() => SceneLoader.LoadScene(Scene.Game));
        _quitButton.onClick.AddListener(() => Application.Quit());
    }
}
