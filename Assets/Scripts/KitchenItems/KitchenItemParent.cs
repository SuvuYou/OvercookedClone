using System;
using System.Collections;
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
    private NetworkVariable<NetworkObjectReference> _currentItemHeldNetworkReference = new();

    public KitchenItem GetCurrentItemHeld() => _currentItemHeld;
    public NetworkObject GetNetworkObjectReference() => NetworkObject;
    public bool IsHoldingItem() => _currentItemHeld != null;

    public override void OnNetworkSpawn()
    {
        _currentItemHeldNetworkReference.OnValueChanged += _onCurrentItemHeldReferenceChange;

        StartCoroutine(_afterNetworkSpawn());
    }

    public override void OnNetworkDespawn()
    {
        _currentItemHeldNetworkReference.OnValueChanged -= _onCurrentItemHeldReferenceChange;
    }

    private IEnumerator _afterNetworkSpawn()
    {
        // Wait until the end of the frame to ensure _currentItemHeldNetworkReference NetworkVariable initialization is done
        yield return new WaitForEndOfFrame();

        _syncCurrentItemHeldWithNetworkReference(_currentItemHeldNetworkReference.Value);
    }

    private void _onCurrentItemHeldReferenceChange (NetworkObjectReference prev, NetworkObjectReference next)
    {
        _syncCurrentItemHeldWithNetworkReference(next);
    }

    private void _syncCurrentItemHeldWithNetworkReference (NetworkObjectReference reference)
    {
        if (!reference.TryGet(out NetworkObject netObj) || !netObj.TryGetComponent(out KitchenItem item))
        {
            _currentItemHeld = null;

            return;
        }

        _currentItemHeld = item;
        item.SetTargetToFollow(_itemSpawnPlaceholder);
    }

    // TODO: Fix delay issues;
    // TODO: Fix Grogress bar AABA;
    // TODO: Fix Sound;
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
        _currentItemHeldNetworkReference.Value = kitchenItem;
        _triggerOnSetCurrentItemEventsClientRpc();
    } 

    [ServerRpc(RequireOwnership = false)]
    private void _resetCurrentItemHeldServerRpc() 
    {
        _currentItemHeldNetworkReference.Value = default;
        _triggerOnReetCurrentItemEventsClientRpc();
    } 

    [ClientRpc]
    private void _triggerOnSetCurrentItemEventsClientRpc() 
    {
        this.TriggerOnItemPickup();
    }
    
    [ClientRpc]
    private void _triggerOnReetCurrentItemEventsClientRpc() 
    {
        this.TriggerOnItemDrop();
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
        if (_currentItemHeldNetworkReference.Value.TryGet(out NetworkObject netObj))
        {
            netObj.Despawn(destroy: true);
        }
    } 
    
    public void SpawnKitchenItem(KitchenItemSO kithcenItem)
    {
        SpawnKitchenItemOnKitchenItemParentServerRpc(KitchenItemsList.Instance.GetIndexOfItem(kithcenItem), kitchenItemParentRef: NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenItemOnKitchenItemParentServerRpc(int kitchenItemIndex, NetworkObjectReference kitchenItemParentRef)
    {
        if (kitchenItemParentRef.TryGet(out NetworkObject netObj) && netObj.TryGetComponent(out KitchenItemParent parent))
        {
            KitchenItem item = Instantiate(KitchenItemsList.Instance.Items[kitchenItemIndex].Prefab, parent.transform.position, Quaternion.identity);
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

        if (_tryAddIngredientToPlateOwner(plateOwner: parent1, ingredient: parent2.GetCurrentItemHeld().GetItemReference()))
        {
            parent2.DestroyCurrentItemHeld();
            return true;
        }

        if (_tryAddIngredientToPlateOwner(plateOwner: parent2, ingredient: parent1.GetCurrentItemHeld().GetItemReference()))
        {
            parent1.DestroyCurrentItemHeld();
            return true;
        }

        return false;
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
