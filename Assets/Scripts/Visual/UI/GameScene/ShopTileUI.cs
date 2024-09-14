using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopTileUI : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private Button _button;

    public void Init(PurchasableItemSO item, Action<PurchasableItemSO> onClick)
    {
        _setupTile(item);

        _button.onClick.AddListener(() => onClick(item));
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveAllListeners();
    }

    private void _setupTile(PurchasableItemSO item)
    {
        _priceText.text = item.Price.ToString() + "$";
        _image.sprite = item.ShopSpriteImage;
        _button.interactable = GameManager.Instance.Balance >= item.Price;
    }
}
