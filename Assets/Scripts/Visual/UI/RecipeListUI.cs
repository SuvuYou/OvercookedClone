using UnityEngine;

public class RecipeListUI : MonoBehaviour
{
    [SerializeField] private RecipeTemplateUI _recipeTemplate;

    private void Awake()
    {
        _recipeTemplate.gameObject.SetActive(false);
        DeliveryManager.Instance.OnAddRecipe += _updateRecipeListUI;
        DeliveryManager.Instance.OnRemoveRecipe += _updateRecipeListUI;
    }

    private void OnDestroy()
    {
        DeliveryManager.Instance.OnAddRecipe -= _updateRecipeListUI;
        DeliveryManager.Instance.OnRemoveRecipe -= _updateRecipeListUI;
    }

    private void _updateRecipeListUI()
    {
        foreach (Transform child in transform)
        {
            if (child == _recipeTemplate.transform) continue;

            Destroy(child.gameObject);
        }

        foreach (RecipeSO recipe in DeliveryManager.Instance.GetCurrentWaitingRecipeList())
        {
            RecipeTemplateUI recipeUI = Instantiate(_recipeTemplate, transform);
            recipeUI.SetupRecipeTemplate(recipe.RecipeName, recipe.Ingredients);

            recipeUI.gameObject.SetActive(true);
        }
    }
}
