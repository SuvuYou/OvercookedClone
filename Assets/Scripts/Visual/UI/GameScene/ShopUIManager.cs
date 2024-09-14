using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    [SerializeField] private Shop _shop;
    [SerializeField] private Button _exitButton;
    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private GameObject _listHolder;
    [SerializeField] private ShopTileUI _tilePrefab;
    [SerializeField] private AvailablePurchasableItemsSO _availablePurchasableItems;

    private List<ShopTileUI> _tiles = new();

    private void Start()
    {
        _hide();

        _exitButton.onClick.AddListener(() => _hide());

        _shop.OnShopOpen += _show;
    }

    private void OnDestroy()
    {
        _shop.OnShopOpen -= _show;
        _exitButton.onClick.RemoveAllListeners(); 
    }

    private void _generateTilesList()
    {
        foreach(var item in _availablePurchasableItems.AvailablePurchasableItems)
        {
            var tile = Instantiate(_tilePrefab, _listHolder.gameObject.transform);
            tile.Init(item, onClick: _handlePurchaseItem);
            _tiles.Add(tile);
        }
    }

    private void _clearTiles()
    {
        foreach(var tile in _tiles) Destroy(tile.gameObject);

        _tiles.Clear();
    }

    private void _handlePurchaseItem(PurchasableItemSO purchasableItem)
    {
        _shop.CreateItem(purchasableItem);
        _hide();
    }

    private void _show() 
    {
        _clearTiles();
        _generateTilesList();
        _shopPanel.gameObject.SetActive(true);
    }

    private void _hide() 
    {
        _shopPanel.gameObject.SetActive(false);
        _clearTiles();
    }
}
