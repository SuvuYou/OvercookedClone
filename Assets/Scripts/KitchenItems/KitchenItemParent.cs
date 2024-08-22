using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private KitchenItemFakeVisual _currentItemHeldFakeVisual;
    private KitchenItem _currentItemHeld;
    private NetworkVariable<NetworkObjectReference> _currentItemHeldNetworkReference = new();

    public KitchenItem GetCurrentItemHeld() => _currentItemHeld;
    public NetworkObject GetNetworkObjectReference() => NetworkObject;
    public bool IsHoldingItem() => _currentItemHeld != null;
    public bool IsHoldingFakeItem() => _currentItemHeldFakeVisual != null;

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

    private void _destroyCurrentItemHeldFakeVisual()
    {
        if (_currentItemHeldFakeVisual != null)
        {
            Destroy(_currentItemHeldFakeVisual.gameObject);
            _currentItemHeldFakeVisual = null;
        }
    }

    private void _syncCurrentItemHeldWithNetworkReference(NetworkObjectReference reference)
    {
        _destroyCurrentItemHeldFakeVisual();

        if (reference.TryGet(out NetworkObject netObj) && netObj.TryGetComponent(out KitchenItem item))
        {
            _currentItemHeld = item;

            GameObjectScaleManipulator.Show(item.gameObject);
            item.gameObject.SetActive(true);
            item.SetTargetToFollow(_itemSpawnPlaceholder);
        }
        else
        {
            _currentItemHeld = null;
        } 
    }

    public void SwapFakeItemIntoReal()
    {
        _destroyCurrentItemHeldFakeVisual();

        if (IsHoldingItem())
        {
            GameObjectScaleManipulator.Show(_currentItemHeld.gameObject);
            _currentItemHeld.SetTargetToFollow(_itemSpawnPlaceholder);
        }
    }

    public void SetFakeVisualKitchenItem(KitchenItemFakeVisual visualFakeItem) 
    {
        _destroyCurrentItemHeldFakeVisual();

        if (IsHoldingItem())
        {
            GameObjectScaleManipulator.Hide(_currentItemHeld.gameObject);
        }

        if (visualFakeItem != null)
        {
            _currentItemHeldFakeVisual = Instantiate(visualFakeItem, _itemSpawnPlaceholder.transform.position, Quaternion.identity, _itemSpawnPlaceholder.transform);
        }
    } 

    public void SetFakeVisualPlate(PlateFakeVisual visualFakePlate, List<KitchenItemSO> ingredients = default) 
    {
        _destroyCurrentItemHeldFakeVisual();

        if (IsHoldingItem())
        {
            GameObjectScaleManipulator.Hide(_currentItemHeld.gameObject);
        }

        if (visualFakePlate != null)
        {
            PlateFakeVisual newItem = Instantiate(visualFakePlate, _itemSpawnPlaceholder.transform.position, Quaternion.identity, _itemSpawnPlaceholder.transform);
            if (ingredients != null)
            {
                newItem.AddIngredients(ingredients);
            }

            _currentItemHeldFakeVisual = newItem;
        }
    } 

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
        if (_currentItemHeldFakeVisual != null) return;

        if (kithcenItem.VisualFakeItem is PlateFakeVisual)
        {
            SetFakeVisualPlate(kithcenItem.VisualFakeItem as PlateFakeVisual);
        }
        else
        {
            SetFakeVisualKitchenItem(kithcenItem.VisualFakeItem);
        }
        
        SpawnKitchenItemOnKitchenItemParentServerRpc(KitchenItemsList.Instance.GetIndexOfItem(kithcenItem), kitchenItemParentRef: NetworkObject);
    }

    public void SpawnKitchenItem(KitchenItemSO plate, KitchenItemSO ingredient)
    {
        if (IsHoldingFakeItem()) return;

        int plateIndex = KitchenItemsList.Instance.GetIndexOfItem(plate);
        int kitchenItemIndex = KitchenItemsList.Instance.GetIndexOfItem(ingredient);

        SetFakeVisualPlate(KitchenItemsList.Instance.Items[plateIndex].VisualFakeItem as PlateFakeVisual, new List<KitchenItemSO> { ingredient });
    
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
                KitchenItemSO item = KitchenItemsList.Instance.Items[kitchenItemIndex];

                if (plate.CanAddIngredient(item))
                {
                    plate.AddIngredientOnNetwork(item);
                }
            }
        }
    }

    public static void SwapItemsOfTwoOwners(KitchenItemParent parent1, KitchenItemParent parent2)
    {  
        KitchenItem tempParent1Item = parent1.GetCurrentItemHeld();
        KitchenItem tempParent2Item = parent2.GetCurrentItemHeld();

        if (tempParent1Item == null && tempParent2Item == null) return;

        if (parent1.IsHoldingFakeItem() || parent2.IsHoldingFakeItem()) return;

        static void setFakeItems(ref KitchenItem item,  ref KitchenItemParent parent) 
        {
            if (item == null)
            {
                parent.SetFakeVisualKitchenItem(null);

                return;
            }
            
            if (item.TryGetPlateComponent(out Plate plateItem))
            {
                parent.SetFakeVisualPlate(plateItem.GetItemReference().VisualFakeItem as PlateFakeVisual, plateItem.Ingredients);
            }
            else
            {
                parent.SetFakeVisualKitchenItem(item.GetItemReference().VisualFakeItem);
            }
        }

        setFakeItems(item: ref tempParent2Item, parent: ref parent1);
        setFakeItems(item: ref tempParent1Item, parent: ref parent2);

        parent1.SetCurrentItemHeld(tempParent2Item);
        parent2.SetCurrentItemHeld(tempParent1Item);
    }

    public static bool TryAddIngredientToPlateOwner(KitchenItemParent parent1, KitchenItemParent parent2)
    {
        if (!parent1.IsHoldingItem() || !parent2.IsHoldingItem()) return false;

        if (parent1.IsHoldingFakeItem() || parent2.IsHoldingFakeItem()) return false;

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

        if (parent.IsHoldingFakeItem()) return false;

        return (
            _tryAddIngredientToPlateOwner(plateOwner: parent, ingredient: kitchenItem)
        );
    }   

    private static bool _tryAddIngredientToPlateOwner(KitchenItemParent plateOwner, KitchenItemSO ingredient)
    {
        if (plateOwner.GetCurrentItemHeld().TryGetPlateComponent(out Plate plate)) 
        {
            if (plate.CanAddIngredient(ingredient))
            {
                void syncWithNetwork(List<KitchenItemSO> ings) 
                { 
                    plateOwner.SwapFakeItemIntoReal();
                    plate.OnIngredientsChange -= syncWithNetwork; 
                };

                plate.OnIngredientsChange += syncWithNetwork;

                List<KitchenItemSO> allIngridients = plate.Ingredients.Concat(new List<KitchenItemSO> { ingredient }).ToList();
                plateOwner.SetFakeVisualPlate(plate.GetItemReference().VisualFakeItem as PlateFakeVisual, allIngridients);
                plateOwner.TriggerOnItemPickup();

                plate.AddIngredientOnNetwork(ingredient);

                return true;
            }
        }

        return false;
    }

    public static void ClearAllIngredientsOffPlate(KitchenItemParent plateOwner)
    {
        if (plateOwner.IsHoldingItem() && plateOwner.GetCurrentItemHeld().TryGetPlateComponent(out Plate plate))
        {
            plate.ClearAllIngredientsOnNetwork();
        }
    }
}
