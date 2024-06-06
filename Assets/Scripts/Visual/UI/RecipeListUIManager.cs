using UnityEngine;

public class RecipeListUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _recipeUI;

    private void Start()
    {
        _recipeUI.SetActive(false);

        GameManager.Instance.OnStateChange += _updateUIBasedOnGameState;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnStateChange -= _updateUIBasedOnGameState;
    }

    private void _updateUIBasedOnGameState(GameState newState)
    {
        if (newState == GameState.Active)
        {
            _recipeUI.SetActive(true);
        }
        else
        {
            _recipeUI.SetActive(false);
        }
    }
}
