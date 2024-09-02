using System;
using Unity.Netcode;
using UnityEngine;

public class Shop : NetworkBehaviour
{
    public event Action OnShopOpen;

    [SerializeField] private AvailablePurchasableItemsSO _availablePurchasableItems;
    [SerializeField] private SelectedObjectsInRangeSO _selectedObjectsInRange;
    [SerializeField] private GameObject _selectedVisualIndicator;

    private void Start()
    {
        _selectedObjectsInRange.OnSelectShop += _checkIsShopSelected;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        _selectedObjectsInRange.OnSelectShop -= _checkIsShopSelected;
    }

    public void CreateItem(PurchasableItemSO item)
    {
        var itemIndex = _availablePurchasableItems.GetPurchasableItemIndex(item);
        if (itemIndex != -1 && GameManager.Instance.Balance > item.Price) _spawnItemServerRpc(itemIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _spawnItemServerRpc(int itemIndex, ServerRpcParams rpcParams = default)
    {
        var purchasedItem = _availablePurchasableItems.AvailablePurchasableItems[itemIndex];
        var createItem = Instantiate(purchasedItem.ItemPrefab, this.gameObject.transform);
        var networkObject = createItem.GetComponent<NetworkObject>();
        networkObject.Spawn();

        GameManager.Instance.UpdateBalance(decrease: purchasedItem.Price);
        
        _selectItemToEditClientRpc(senderClientId: rpcParams.Receive.SenderClientId, obj: networkObject);
    }

    [ClientRpc]
    private void _selectItemToEditClientRpc(ulong senderClientId, NetworkObjectReference obj)
    {
        if (NetworkManager.Singleton.LocalClientId != senderClientId) return;

        if (obj.TryGet(out NetworkObject netObj) && netObj.TryGetComponent(out EditableItem item))
        {
            _selectedObjectsInRange.TriggerSelectEditingSubject(item);
            _selectedObjectsInRange.TriggerOnStartEditing();  
        }
    }

    public void Interact()
    {
        if (!_selectedObjectsInRange.IsCurrentlyEditing || _selectedObjectsInRange.SelectedEditingSubject == null)
        {
            OnShopOpen?.Invoke();

            return;
        }
       
        var purchasableItem = _availablePurchasableItems.FindPurchasableItemByEditableItem(_selectedObjectsInRange.SelectedEditingSubject);

        if (purchasableItem == null) return;

        _destroyEditingSubjectServerRpc(_selectedObjectsInRange.SelectedEditingSubject.GetComponent<NetworkObject>());
                
        _selectedObjectsInRange.TriggerOnEndEditing();
        _selectedObjectsInRange.TriggerSelectEditingSubject(null);
        
        GameManager.Instance.UpdateBalance(increase: purchasableItem.Price);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _destroyEditingSubjectServerRpc(NetworkObjectReference netObj)
    {
        if (netObj.TryGet(out NetworkObject obj)) 
        {
            Destroy(obj.gameObject);
        }
    }

    private void _checkIsShopSelected(Shop newSelectedShop)
    {
        _selectedVisualIndicator.SetActive(newSelectedShop == this);
    }
}
