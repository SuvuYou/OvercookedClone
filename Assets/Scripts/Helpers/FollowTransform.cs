using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    private Transform _targetTransform;

    void LateUpdate()
    {
        if (_targetTransform != null)
        {
            transform.position = _targetTransform.position;
        }
    }

    public void SetTargetTransform (Transform target)
    {
        _targetTransform = target;
    }
}
