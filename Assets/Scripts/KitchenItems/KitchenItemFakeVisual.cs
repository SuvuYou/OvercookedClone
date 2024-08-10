using UnityEngine;

[RequireComponent(typeof(FollowTransform))]
public class KitchenItemFakeVisual : MonoBehaviour
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

    public bool TryGetPlateComponent(out KitchenItemFakeVisual plate)
    {
        if (this is KitchenItemFakeVisual)
        {
            plate = this;

            return true;
        }
        else
        {
            plate = null;
            
            return false;
        }
    }
}
