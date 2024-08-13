using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(FollowTransform))]
public class KitchenItem : NetworkBehaviour
{
    [SerializeField] KitchenItemSO _itemReference;
    FollowTransform _followTransform;

    public override void OnNetworkSpawn()
    {
        _followTransform = GetComponent<FollowTransform>();
    }

    public void SetTargetToFollow(Transform target)
    {
        _followTransform.SetTargetTransform(target);
    }

    public KitchenItemSO GetItemReference() => _itemReference;

    public NetworkObject GetNetworkObjectReference() => NetworkObject;

    public bool TryGetPlateComponent(out Plate plate)
    {
        if (this is Plate)
        {
            plate = this as Plate;

            return true;
        }
        else
        {
            plate = null;
            
            return false;
        }
    }
}
