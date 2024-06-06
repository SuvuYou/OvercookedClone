using UnityEngine;

public class PlateIconsUI : MonoBehaviour
{
    [SerializeField] private Plate _plate;
    [SerializeField] private GameObject _iconTemplate; 

    private void Awake()
    {
        _iconTemplate.SetActive(false);
        _plate.OnAddIngredient += _spawnIngredientIcon;
    }

    private void OnDestroy()
    {
        _plate.OnAddIngredient -= _spawnIngredientIcon;
    }

    private void _spawnIngredientIcon(KitchenItemSO ingredient)
    {
        GameObject icon = Instantiate(_iconTemplate, transform);
        icon.GetComponent<PlateIconTemplateUI>().SetIconSprite(ingredient.IconSprite);
        icon.SetActive(true);
    }
}
