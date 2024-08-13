using System;
using Unity.Netcode;
using UnityEngine;

public class PlateCounter : BaseCounter
{
    [SerializeField] private Plate _platePrefab;
    public event Action<int> OnPlateNumberChange;

    private NetworkVariable<int> _platesCount = new (value: 0);
    private const int _platesCountLimit = 4;
    private TimingTimer _plateSpawnTimer = new (defaultTimerValue: 2f);

    public override void OnNetworkSpawn()
    {
        OnPlateNumberChange?.Invoke(_platesCount.Value);
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Active)
        {
            return;
        }

        if (!IsServer)
        {
            return;
        }

        if (_platesCount.Value < _platesCountLimit)
        {
            _plateSpawnTimer.SubtractTime(Time.deltaTime);

            if (_plateSpawnTimer.IsTimerUp())
            {
                _updatePlatesCount(newPlatesCount: _platesCount.Value + 1);
                _plateSpawnTimer.ResetTimer();
            }
        }
    }

    public override void Interact(KitchenItemParent player)
    {
        if (_platesCount.Value <= 0) return;

        if (!player.IsHoldingItem())
        {
            player.SpawnKitchenItem(_platePrefab.GetItemReference());

            _updatePlatesCount(newPlatesCount: _platesCount.Value - 1);

            return;
        }

        KitchenItemSO itemHeldByPlayer = player.GetCurrentItemHeld().GetItemReference();
        
        if (Plate.IsIngridientAllowedOnPlate(itemHeldByPlayer))
        {
            player.DestroyCurrentItemHeld();
            player.SpawnKitchenItem(plate: _platePrefab.GetItemReference(), ingredient: itemHeldByPlayer);
            
            _updatePlatesCount(newPlatesCount: _platesCount.Value - 1);

            return;
        }
    }

    private void _triggerOnPlateNumberChange(int newPlatesCount)
    {
        OnPlateNumberChange?.Invoke(newPlatesCount);
    }

    private void _updatePlatesCount(int newPlatesCount) 
    {
        _triggerOnPlateNumberChange(newPlatesCount);
        _updatePlatesCountServerRpc(newPlatesCount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _updatePlatesCountServerRpc(int newPlatesCount, ServerRpcParams rpcParams = default)
    {
        _platesCount.Value = newPlatesCount;
        _updatePlatesCountClientRpc(newPlatesCount, senderClientId: rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void _updatePlatesCountClientRpc(int newPlatesCount, ulong senderClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == senderClientId) return;

        _triggerOnPlateNumberChange(newPlatesCount);
    }
}
