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
            SetCurrentItemHeldServerRpc(newItem.GetNetworkObjectReference());
        }
        else
        {
            ResetCurrentItemHeldServerRpc();
        }
    } 

    [ServerRpc(RequireOwnership = false)]
    public void SetCurrentItemHeldServerRpc(NetworkObjectReference kitchenItem) 
    {
        SetCurrentItemHeldClientRpc(kitchenItem);
    } 
    
    [ClientRpc]
    public void SetCurrentItemHeldClientRpc(NetworkObjectReference kitchenItem) 
    {
        if (kitchenItem.TryGet(out NetworkObject netObj) && netObj.TryGetComponent(out KitchenItem item))
        {
            _currentItemHeld = item;
            item.SetTargetToFollow(_itemSpawnPlaceholder);
            TriggerOnItemPickup();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetCurrentItemHeldServerRpc() 
    {
        ResetCurrentItemHeldClientRpc();
    } 
    
    [ClientRpc]
    public void ResetCurrentItemHeldClientRpc() 
    {
        _currentItemHeld = null;
        TriggerOnItemDrop();
    }

    public void DestroyCurrentItemHeld()
    {
        if (_currentItemHeld == null) return;

        Destroy(_currentItemHeld.gameObject);
        SetCurrentItemHeld(null);
    }

    public NetworkObject GetNetworkObjectReference()
    {
        return NetworkObject;
    }

    public void SpawnKitchenItem(int kithcenItemIndex)
    {
        SpawnKitchenItemOnKitchenItemParentServerRpc(kithcenItemIndex, kitchenItemParentRef: NetworkObject);
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
            _tryAddIngredientToPlateOwner(plateOwner: parent1, ingredientOwner: parent2) || 
            _tryAddIngredientToPlateOwner(plateOwner: parent2, ingredientOwner: parent1)
        );
    }   
    
    private static bool _tryAddIngredientToPlateOwner(KitchenItemParent plateOwner, KitchenItemParent ingredientOwner)
    {
        if (plateOwner.GetCurrentItemHeld().TryGetPlateComponent(out Plate plate)) 
        {
            if (plate.TryAddIngredient(ingredientOwner.GetCurrentItemHeld().GetItemReference()))
            {
                ingredientOwner.DestroyCurrentItemHeld();
                plateOwner.TriggerOnItemPickup();

                return true;
            }
        }
        
        return false;
    }
}
