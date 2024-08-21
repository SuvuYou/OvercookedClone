using System.Collections.Generic;
using UnityEngine;

public class CustomersSpawnerManager : MonoBehaviour
{
    [SerializeField] private Transform _spawnPosition;
    [SerializeField] private CustomersGroup _customersGroupPrefab;
    [SerializeField] private ServiceTablesManager _serviceTablesManager;

    private List<CustomersGroup> _activeCustomersGroups = new();
    private List<CustomersGroup> _waitingCustomersGroups = new();

    private const int MAX_GROUPS_WAITING_COUNT = 4; 
    private const float MIN_TIMEOUT_BETWEEN_GROUPS_SPAWNED = 1f; 
    private const float MAX_TIMEOUT_BETWEEN_GROUPS_SPAWNED = 5f; 
    private TimingTimer _spawningGroupTimer = new(minDefaultTimerValue: MIN_TIMEOUT_BETWEEN_GROUPS_SPAWNED, maxDefaultTimerValue: MAX_TIMEOUT_BETWEEN_GROUPS_SPAWNED);

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Active)
        {
            return;
        }

        // TODO: only on server (isServer)
        _handleSpawningWaitingGroups();
    }

    private void _handleSpawningWaitingGroups()
    {
        _spawningGroupTimer.SubtractTime(Time.deltaTime);

        if (_spawningGroupTimer.IsTimerUp() && _waitingCustomersGroups.Count < MAX_GROUPS_WAITING_COUNT)
        {
            _spawningGroupTimer.ResetTimer();

            CustomersGroup group = Instantiate(_customersGroupPrefab, _spawnPosition);
            _waitingCustomersGroups.Add(group);

            CheckInWaitingGroupInside();
        }
    }

    public void CheckInWaitingGroupInside()
    {
        foreach (CustomersGroup group in _waitingCustomersGroups)
        {
            ServiceTable serviceTable = _serviceTablesManager.FindAvailableTableForGroup(group);
            
            if (serviceTable == null) continue;

            _activeCustomersGroups.Add(group);

            group.Populate(spawnPosition: _spawnPosition);
            group.AssingTable(serviceTable);
            serviceTable.TakeTable(group);

            group.OnGroupFinishedEating += () => 
            {
                group.Leave(exitPosition: _spawnPosition.position);
                serviceTable.ClearTable();
                serviceTable.FreeTable();
                _activeCustomersGroups.Remove(group);
                CheckInWaitingGroupInside();
            };
        }

        foreach (CustomersGroup group in _activeCustomersGroups)
        {
            if (_waitingCustomersGroups.Contains(group)) _waitingCustomersGroups.Remove(group);
        }
    }
}
