using System;
using Unity.Netcode;
using UnityEngine;

public class KitchenItemParent : NetworkBehaviour
{
    [SerializeField] private Transform _itemSpawnPlaceholder;

    public event Action OnItemPickup;
    public void TriggerOnItemPickup () => OnItemPickup?.Invoke();

    public event Action OnItemDrop;
    public void TriggerOnItemDrop () => OnItemDrop?.Invoke();

    private KitchenItem _currentItemHeld;

    public KitchenItem GetCurrentItemHeld() => _currentItemHeld;
    public bool IsHoldingItem() => _currentItemHeld != null;

    // TODO: Fix delay issues;
    public void SetCurrentItemHeld(KitchenItem newItem) 
    {
        if (newItem != null)
        {
            _setCurrentItemHeldServerRpc(newItem.GetNetworkObjectReference());
        }
        else
        {
            _resetCurrentItemHeldServerRpc();
        }
    } 

    [ServerRpc(RequireOwnership = false)]
    private void _setCurrentItemHeldServerRpc(NetworkObjectReference kitchenItem) 
    {
        _setCurrentItemHeldClientRpc(kitchenItem);
    } 
    
    [ClientRpc]
    private void _setCurrentItemHeldClientRpc(NetworkObjectReference kitchenItem) 
    {
        if (kitchenItem.TryGet(out NetworkObject netObj) && netObj.TryGetComponent(out KitchenItem item))
        {
            _currentItemHeld = item;
            item.SetTargetToFollow(_itemSpawnPlaceholder);
            TriggerOnItemPickup();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void _resetCurrentItemHeldServerRpc() 
    {
        _resetCurrentItemHeldClientRpc();
    } 
    
    [ClientRpc]
    private void _resetCurrentItemHeldClientRpc() 
    {
        _currentItemHeld = null;
        TriggerOnItemDrop();
    }

    public void DestroyCurrentItemHeld()
    {
        if (_currentItemHeld == null) return;

        _destroyCurrentItemHeldServerRpc();
        _resetCurrentItemHeldServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void _destroyCurrentItemHeldServerRpc() 
    {
        if (_currentItemHeld.TryGetComponent(out NetworkObject netObj))
        {
            netObj.Despawn(destroy: true);
        }
    } 
    
    public NetworkObject GetNetworkObjectReference()
    {
        return NetworkObject;
    }

    public void SpawnKitchenItem(KitchenItemSO kithcenItem)
    {
        SpawnKitchenItemOnKitchenItemParentServerRpc(KitchenItemsList.Instance.GetIndexOfItem(kithcenItem), kitchenItemParentRef: NetworkObject);
    }

    // TODO: maybe make static 
    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenItemOnKitchenItemParentServerRpc(int kitchenItemIndex, NetworkObjectReference kitchenItemParentRef)
    {
        if (kitchenItemParentRef.TryGet(out NetworkObject netObj) && netObj.TryGetComponent(out KitchenItemParent parent))
        {
            KitchenItem item = Instantiate(KitchenItemsList.Instance.Items[kitchenItemIndex].Prefab, Vector3.zero, Quaternion.identity);
            item.GetComponent<NetworkObject>().Spawn();
            parent.SetCurrentItemHeld(item);
        }
    }

    public static void SwapItemsOfTwoOwners(KitchenItemParent parent1, KitchenItemParent parent2)
    {  
        KitchenItem tempItem = parent1.GetCurrentItemHeld();
        parent1.SetCurrentItemHeld(parent2.GetCurrentItemHeld());
        parent2.SetCurrentItemHeld(tempItem);
    }

    public static bool TryAddIngredientToPlate(KitchenItemParent parent1, KitchenItemParent parent2)
    {
        if (!parent1.IsHoldingItem() || !parent2.IsHoldingItem()) return false;

        return (
            _tryAddIngredientToPlateOwner(plateOwner: parent1, ingredient: parent2.GetCurrentItemHeld().GetItemReference()) || 
            _tryAddIngredientToPlateOwner(plateOwner: parent2, ingredient: parent1.GetCurrentItemHeld().GetItemReference())
        );
    }   

    public static bool TryAddIngredientToPlate(KitchenItemParent parent, KitchenItemSO kitchenItem)
    {
        if (!parent.IsHoldingItem()) return false;

        return (
            _tryAddIngredientToPlateOwner(plateOwner: parent, ingredient: kitchenItem)
        );
    }   

    private static bool _tryAddIngredientToPlateOwner(KitchenItemParent plateOwner, KitchenItemSO ingredient)
    {
        if (plateOwner.GetCurrentItemHeld().TryGetPlateComponent(out Plate plate)) 
        {
            if (plate.TryAddIngredientOnNetwork(ingredient))
            {
                plateOwner.TriggerOnItemPickup();

                return true;
            }
        }

        return false;
    }
}
