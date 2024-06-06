using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RecipeTemplateUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _recipeName;
    [SerializeField] private GameObject _iconHolder;
    [SerializeField] private PlateIconTemplateUI _iconTemplate;

    private void Awake()
    {
        _iconTemplate.gameObject.SetActive(false);
    }

    public void SetupRecipeTemplate(string recipeName, List<KitchenItemSO> Ingredients)
    {
        _recipeName.text = recipeName;

        foreach (KitchenItemSO ingredient in Ingredients)
        {
            PlateIconTemplateUI icon = Instantiate(_iconTemplate, _iconHolder.transform);
            icon.SetIconSprite(ingredient.IconSprite);
            icon.gameObject.SetActive(true);
        }
    }
}
