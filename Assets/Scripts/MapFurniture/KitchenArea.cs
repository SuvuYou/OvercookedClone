using System;
using UnityEngine;

public class KitchenArea : MonoBehaviour
{
    public event Action OnKitchenEnter; 
    public event Action OnKitchenExit;

    void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent(out PlayerController _))
        {
            OnKitchenEnter?.Invoke();
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.TryGetComponent(out PlayerController _))
        {
            OnKitchenExit?.Invoke();
        }
    }
}
