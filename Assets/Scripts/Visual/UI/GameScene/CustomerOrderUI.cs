using System.Collections.Generic;
using UnityEngine;

public class CustomerOrderUI : MonoBehaviour
{
    [SerializeField] private GameObject _iconTemplate; 
    [SerializeField] private Customer _customer;

    private void Start()
    {
        _customer.OnSitDown += _enableIngredientsIcons;
        _customer.OnRecieveOrder += _disableIngredientsIcons;
        _customer.OnCustomerLeaving += _disableIngredientsIcons;

        _spawnIngredientIcons(ingredients: _customer.Order.Ingredients);
        _disableIngredientsIcons();
    }

    private void OnDestroy()
    {
        _customer.OnSitDown -= _enableIngredientsIcons;
        _customer.OnRecieveOrder -= _disableIngredientsIcons;
        _customer.OnCustomerLeaving -= _disableIngredientsIcons;
    }

    private void _enableIngredientsIcons() 
    {
        gameObject.SetActive(true);
    }

    private void _disableIngredientsIcons() 
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
