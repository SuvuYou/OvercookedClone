using UnityEngine;

enum Mode {
    Normal,
    Inverted,
    CameraForward,
    CameraForwardInverted
}

public class LookAtCamara : MonoBehaviour
{
    [SerializeField] private Mode _mode;

    void LateUpdate()
    {
        switch(_mode)
        {
            case Mode.Normal:
                transform.LookAt(Camera.main.transform);
                break;
            case Mode.Inverted:
                transform.LookAt(transform.position + (transform.position - Camera.main.transform.position));
                break;
            case Mode.CameraForward:
                transform.forward = Camera.main.transform.forward;
                break;
            case Mode.CameraForwardInverted:
                transform.forward = -Camera.main.transform.forward;
                break;
        }
    }
}
