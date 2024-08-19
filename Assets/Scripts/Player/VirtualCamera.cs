using UnityEngine;
using Cinemachine;

public class VirtualCamera : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    
    public void SetCameraTarger(Transform target)
    {
        _virtualCamera.m_Follow = target;
        _virtualCamera.m_LookAt = target;
    }
}
