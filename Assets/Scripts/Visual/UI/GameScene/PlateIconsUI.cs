using System.Collections.Generic;
using UnityEngine;

public class PlateIconsUI : MonoBehaviour
{
    [SerializeField] private Plate _plate;
    [SerializeField] private GameObject _iconTemplate; 

    private void Awake()
    {
        _iconTemplate.SetActive(false);
        _plate.OnIngredientsChange += _spawnIngredientIcons;
    }

    private void OnDestroy()
    {
        _plate.OnIngredientsChange -= _spawnIngredientIcons;
    }

    private void _spawnIngredientIcons(List<KitchenItemSO> ingredients)
    {
        foreach (Transform child in transform)
        {
            if (child != _iconTemplate.transform)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (KitchenItemSO ingredient in ingredients)
        {
            GameObject icon = Instantiate(_iconTemplate, transform);
            icon.GetComponent<PlateIconTemplateUI>().SetIconSprite(ingredient.IconSprite);
            icon.SetActive(true);
        }
    }
}
