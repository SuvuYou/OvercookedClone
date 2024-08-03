using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class KitchenItemParent : NetworkBehaviour
{
    [SerializeField] private Transform _itemSpawnPlaceholder;

    // TODO: fix double sound effects
    public event Action OnItemPickup;
    public void TriggerOnItemPickup () 
    {
        _triggerOnPickupServerRpc();
    }

    public event Action OnItemDrop;
    public void TriggerOnItemDrop ()
    {
        _triggerOnDropServerRpc();
    } 

    private KitchenItemVisual _currentItemHeldFakeVisual;
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

    private void _onCurrentItemHeldReferenceChange(NetworkObjectReference prev, NetworkObjectReference next)
    {
        _syncCurrentItemHeldWithNetworkReference(next);
    }

    private void _syncCurrentItemHeldWithNetworkReference(NetworkObjectReference reference)
    {
        if (!reference.TryGet(out NetworkObject netObj) || !netObj.TryGetComponent(out KitchenItem item))
        {
            _currentItemHeld = null;

            if (_currentItemHeldFakeVisual != null)
            {
                Destroy(_currentItemHeldFakeVisual.gameObject);
                _currentItemHeldFakeVisual = null;
            }
        
            return;
        }

        _currentItemHeld = item;
        _currentItemHeld.gameObject.SetActive(true);

        item.SetTargetToFollow(_itemSpawnPlaceholder);
        if (_currentItemHeldFakeVisual != null)
        {
            Destroy(_currentItemHeldFakeVisual.gameObject);
            _currentItemHeldFakeVisual = null;
        }
    }

    public void SetFakeVisualKitchenItem(KitchenItemVisual visualFakeItem) 
    {
        if (_currentItemHeldFakeVisual != null)
        {
            Destroy(_currentItemHeldFakeVisual.gameObject);
            _currentItemHeldFakeVisual = null;
        }

        if (IsHoldingItem())
        {
            _currentItemHeld.gameObject.SetActive(false);
        }

        if (visualFakeItem != null)
        {
            _currentItemHeldFakeVisual = Instantiate(visualFakeItem, _itemSpawnPlaceholder.transform.position, Quaternion.identity, _itemSpawnPlaceholder.transform);
        }
    } 

    // TODO: Chech if delay issues are noticable;
    // They are
    public void SetCurrentItemHeld(KitchenItem newItem) 
    {
        if (newItem != null)
        {
            _setCurrentItemHeldServerRpc(newItem.GetNetworkObjectReference());

            return;
        }

        if (IsHoldingItem())
        {
            _triggerOnDropServerRpc();
        }

        _resetCurrentItemHeldServerRpc();
    } 

    [ServerRpc(RequireOwnership = false)]
    private void _setCurrentItemHeldServerRpc(NetworkObjectReference kitchenItem) 
    {
        _currentItemHeldNetworkReference.Value = kitchenItem;
        _triggerOnPickupClientRpc();
    } 

    [ServerRpc(RequireOwnership = false)]
    private void _triggerOnPickupServerRpc() 
    {
        _triggerOnPickupClientRpc();
    }

    [ClientRpc]
    private void _triggerOnPickupClientRpc() 
    {
        OnItemPickup?.Invoke();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void _triggerOnDropServerRpc() 
    {
        _triggerOnDropClientRpc();
    }

    [ClientRpc]
    private void _triggerOnDropClientRpc() 
    {
        OnItemDrop?.Invoke();
    }

    public void DestroyCurrentItemHeld()
    {
        if (!IsHoldingItem()) return;

        SetFakeVisualKitchenItem(null);
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

    [ServerRpc(RequireOwnership = false)]
    private void _resetCurrentItemHeldServerRpc() 
    {
        _currentItemHeldNetworkReference.Value = default;
    } 
    
    public void SpawnKitchenItem(KitchenItemSO kithcenItem)
    {
        SpawnKitchenItemOnKitchenItemParentServerRpc(KitchenItemsList.Instance.GetIndexOfItem(kithcenItem), kitchenItemParentRef: NetworkObject);
        SetFakeVisualKitchenItem(kithcenItem.VisualFakeItem);
    }

    public void SpawnKitchenItem(KitchenItemSO plate, KitchenItemSO ingredient)
    {
        int plateIndex = KitchenItemsList.Instance.GetIndexOfItem(plate);
        int kitchenItemIndex = KitchenItemsList.Instance.GetIndexOfItem(ingredient);

        SetFakeVisualKitchenItem(KitchenItemsList.Instance.Items[kitchenItemIndex].VisualFakeItem);
        SpawnPlateWithIngredientOnKitchenItemParentServerRpc(plateIndex, kitchenItemIndex, kitchenItemParentRef: NetworkObject);
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

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlateWithIngredientOnKitchenItemParentServerRpc(int plateIndex, int kitchenItemIndex, NetworkObjectReference kitchenItemParentRef)
    {
        if (kitchenItemParentRef.TryGet(out NetworkObject netObj) && netObj.TryGetComponent(out KitchenItemParent parent))
        {
            KitchenItem plateItem = Instantiate(KitchenItemsList.Instance.Items[plateIndex].Prefab, parent.transform.position, Quaternion.identity);
            plateItem.GetComponent<NetworkObject>().Spawn();
            parent.SetCurrentItemHeld(plateItem);

            if (plateItem.TryGetPlateComponent(out Plate plate))
            {
                plate.TryAddIngredientOnNetwork(KitchenItemsList.Instance.Items[kitchenItemIndex]);
            }
        }
    }

    public static void SwapItemsOfTwoOwners(KitchenItemParent parent1, KitchenItemParent parent2)
    {  
        KitchenItem tempParent1Item = parent1.GetCurrentItemHeld();
        KitchenItem tempParent2Item = parent2.GetCurrentItemHeld();

        if (tempParent1Item == null && tempParent2Item == null) return;

        parent1.SetCurrentItemHeld(tempParent2Item);
        if (tempParent2Item != null)
        {
            parent1.SetFakeVisualKitchenItem(parent2.GetCurrentItemHeld().GetItemReference().VisualFakeItem);
        }
        else
        {
            parent1.SetFakeVisualKitchenItem(null);
        }
        
        parent2.SetCurrentItemHeld(tempParent1Item);
        if (tempParent1Item != null)
        {
            parent2.SetFakeVisualKitchenItem(tempParent1Item.GetItemReference().VisualFakeItem);
        }
        else        
        {
            parent2.SetFakeVisualKitchenItem(null);
        }
    }

    public static bool TryAddIngredientToPlateOwner(KitchenItemParent parent1, KitchenItemParent parent2)
    {
        if (!parent1.IsHoldingItem() || !parent2.IsHoldingItem()) return false;

        if (_tryAddIngredientToPlateOwner(plateOwner: parent1, ingredient: parent2.GetCurrentItemHeld().GetItemReference()))
        {
            parent2.DestroyCurrentItemHeld();
            parent2.TriggerOnItemDrop();
            
            return true;
        }

        if (_tryAddIngredientToPlateOwner(plateOwner: parent2, ingredient: parent1.GetCurrentItemHeld().GetItemReference()))
        {
            parent1.DestroyCurrentItemHeld();
            parent1.TriggerOnItemDrop();

            return true;
        }

        return false;
    }   

    public static bool TryAddIngredientToPlateOwner(KitchenItemParent parent, KitchenItemSO kitchenItem)
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
