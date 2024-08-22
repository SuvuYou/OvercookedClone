using UnityEngine;
using Unity.Netcode;

public class Chair : BaseCounter
{
    [SerializeField] private Transform _sittingPlacePosition;
    
    private Customer _currentCustomer;
    public Transform SittingPlacePosition { get => _sittingPlacePosition; }

    private bool _shouldRecievedOrderWhenCustomerSitsDown = false;

    private void Update()
    {
        if (_shouldRecievedOrderWhenCustomerSitsDown && _currentCustomer != null)
        {
            _revieveOrderLocally();
            _shouldRecievedOrderWhenCustomerSitsDown = false;
        }
    }

    public void TakeSit(Customer sitter)
    {
        _currentCustomer = sitter;
        sitter.gameObject.transform.position = _sittingPlacePosition.position;
    }

    public void FinishDish()
    {
        if (IsServer)
        {
            KitchenItemParent.ClearAllIngredientsOffPlate(plateOwner: this);
        }
    }

    public override void Interact(KitchenItemParent player)
    {
        if (player.IsHoldingItem() && player.GetCurrentItemHeld().TryGetPlateComponent(out Plate plate))
        {
            if (_currentCustomer != null && _currentCustomer.CanRecieveOrder(plate))
            {
                KitchenItemParent.SwapItemsOfTwoOwners(player, this);
                plate.DeliverPlate();
                _recieveOrderOnNetwork();
                _triggerSuccessfulSoundEffectServerRpc();  
            }
        }
    }

    private void _recieveOrderOnNetwork()
    {
        _revieveOrderLocally();
        _recieveOrderOnNetworkServerRpc();
    }

    private void _revieveOrderLocally()
    {
        _currentCustomer.RecieveOrder();
    }

    [ServerRpc(RequireOwnership = false)]
    private void _triggerSuccessfulSoundEffectServerRpc()
    {
        _triggerSuccessfulSoundEffectclientRpc();
    }

    [ClientRpc]
    private void _triggerSuccessfulSoundEffectclientRpc()
    {
        SoundManager.SoundEvents.TriggerOnDeliverSuccessSound(transform.position);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _recieveOrderOnNetworkServerRpc(ServerRpcParams rpcParams = default)
    {
        _recieveOrderOnNetworkClientRpc(senderClientId: rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void _recieveOrderOnNetworkClientRpc(ulong senderClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == senderClientId) return;

        if (_currentCustomer != null) _revieveOrderLocally();
        else _shouldRecievedOrderWhenCustomerSitsDown = true;
    }
}
