using System.Collections.Generic;
using UnityEngine;

public class PlateIconsUI : MonoBehaviour
{
    [SerializeField] private GameObject _iconTemplate; 

    private IPlate _plate;

    private void Awake()
    {
        _plate = GetComponentInParent<IPlate>();
        _iconTemplate.SetActive(false);
        _plate.OnIngredientsChange += _spawnIngredientIcons;
        _plate.OnDeliverPlate += _hideIngredientIcons;
    }

    private void OnDestroy()
    {
        _plate.OnIngredientsChange -= _spawnIngredientIcons;
        _plate.OnDeliverPlate -= _hideIngredientIcons;
    }

    private void _hideIngredientIcons()
    {
        gameObject.SetActive(false);
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
