using System;
using Unity.Netcode;
using UnityEngine;

public class KitchenArea : MonoBehaviour
{
    public event Action OnKitchenEnter; 
    public event Action OnKitchenExit;

    void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent(out PlayerController player))
        {
            if (player.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                OnKitchenEnter?.Invoke();
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.TryGetComponent(out PlayerController player))
        {
            if (player.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                OnKitchenExit?.Invoke();
            }
        }
    }
}
