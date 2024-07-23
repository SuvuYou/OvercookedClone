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
                _updatePlatesCountServerRpc(newPlatesCount: _platesCount.Value + 1);
                _plateSpawnTimer.ResetTimer();
            }
        }
    }

    public override void Interact(KitchenItemParent player)
    {
        if (_platesCount.Value > 0)
        {
            if (!player.IsHoldingItem())
            {
                player.SpawnKitchenItem(_platePrefab.GetItemReference());

                _updatePlatesCountServerRpc(newPlatesCount: _platesCount.Value - 1);
            }
            else if (Plate.IsIngridientAllowedOnPlate(player.GetCurrentItemHeld().GetItemReference()))
            {
                KitchenItemSO item = player.GetCurrentItemHeld().GetItemReference();
                player.DestroyCurrentItemHeld();
                player.SpawnKitchenItem(plate: _platePrefab.GetItemReference(), ingredient: item);
                
                _updatePlatesCountServerRpc(newPlatesCount: _platesCount.Value - 1);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void _updatePlatesCountServerRpc(int newPlatesCount)
    {
        _platesCount.Value = newPlatesCount;
        _triggerOnPlateNumberChangeClientRpc(newPlatesCount);
    }

    [ClientRpc]
    private void _triggerOnPlateNumberChangeClientRpc(int newPlatesCount)
    {
        OnPlateNumberChange?.Invoke(newPlatesCount);
    }
}
