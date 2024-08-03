using UnityEngine;

[RequireComponent(typeof(FollowTransform))]
public class KitchenItemVisual : MonoBehaviour
{
    FollowTransform _followTransform;

    private void Awake()
    {
        _followTransform = GetComponent<FollowTransform>();
    }

    public void SetTargetToFollow(Transform target)
    {
        _followTransform.SetTargetTransform(target);
    }

    // public bool TryGetPlateComponent(out Plate plate)
    // {
    //     if (this is Plate)
    //     {
    //         plate = this as Plate;

    //         return true;
    //     }
    //     else
    //     {
    //         plate = null;
            
    //         return false;
    //     }
    // }
}
