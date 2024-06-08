using System;
using UnityEngine;

public class PlateCounter : BaseCounter
{
    [SerializeField] private Plate _platePrefab;
    public event Action<int> OnPlateNumberChange;

    private int _platesCount = 0;
    private const int _platesCountLimit = 4;
    private float _plateSpawnTimer;
    private const float _plateSpawnTime = 2f;

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Active)
        {
            return;
        }

        if (_platesCount < _platesCountLimit)
        {
            _plateSpawnTimer += Time.deltaTime;

            if (_plateSpawnTimer >= _plateSpawnTime)
            {
                _updatePlatesCount(newPlatesCount: _platesCount + 1);
                _plateSpawnTimer = 0f;
            }
        }
    }

    public override void Interact(KitchenItemParent player)
    {
        if (_platesCount > 0)
        {
            if (!player.IsHoldingItem())
            {
                player.SpawnKitchenItem(_platePrefab.GetItemReference());

                _updatePlatesCount(newPlatesCount: _platesCount - 1);
            }
            // else if (Plate.IsIngridientAllowedOnPlate(player.GetCurrentItemHeld().GetItemReference()))
            // {
                // TODO: Potentialy allow player to pick up plate with ingredient in hand
            // }
        }
    }

    private void _updatePlatesCount(int newPlatesCount)
    {
        _platesCount = newPlatesCount;
        OnPlateNumberChange?.Invoke(newPlatesCount);
    }
}
