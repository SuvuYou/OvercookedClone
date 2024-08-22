using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;

public class CustomersSpawnerManager : NetworkBehaviour
{
    [SerializeField] private Transform _spawnPosition;
    [SerializeField] private CustomersGroup _customersGroupPrefab;
    [SerializeField] private ServiceTablesManager _serviceTablesManager;

    private List<CustomersGroup> _activeCustomers = new();
    private List<CustomersGroup> _customersQueue = new();

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

        if (IsServer)
        {
            _handleSpawningGroupsForQueue();
        }
    }

    private void _handleSpawningGroupsForQueue()
    {
        _spawningGroupTimer.SubtractTime(Time.deltaTime);

        if (_spawningGroupTimer.IsTimerUp() && _customersQueue.Count < MAX_GROUPS_WAITING_COUNT)
        {
            _spawningGroupTimer.ResetTimer();

            CustomersGroup group = Instantiate(_customersGroupPrefab, _spawnPosition);
            _customersQueue.Add(group);
            group.InitGroupSize();

            _checkInGroupFromQueue();
        }
    }

    private void _checkInGroupFromQueue()
    {
        foreach (CustomersGroup group in _customersQueue)
        {
            ServiceTable serviceTable = _serviceTablesManager.FindAvailableTableForGroup(group);
            
            if (serviceTable == null) continue;

            _activeCustomers.Add(group);

            _initiateGroupLocally(group, serviceTable);
            _initiateGroupOnNetworkServerRpc(serviceTableIndex: _serviceTablesManager.GetIndexByTable(serviceTable), groupConfig: group.GroupConfig, groupSize: group.CustomersCount);
        }

        foreach (CustomersGroup group in _activeCustomers)
        {
            if (_customersQueue.Contains(group)) _customersQueue.Remove(group);
        }
    }

    [ServerRpc]
    private void _initiateGroupOnNetworkServerRpc(int serviceTableIndex, int groupSize, int[] groupConfig)
    {
        _initiateGroupOnNetworkClientRpc(serviceTableIndex, groupSize, groupConfig);
    }
    
    [ClientRpc]
    private void _initiateGroupOnNetworkClientRpc(int serviceTableIndex, int groupSize, int[] groupConfig)
    {
        if (NetworkManager.Singleton.IsServer) return;

        CustomersGroup group = Instantiate(_customersGroupPrefab, _spawnPosition);
        group.InitGroupSize(forcedGroupSize: groupSize);

        _initiateGroupLocally(group, serviceTable: _serviceTablesManager.GetTableByIndex(serviceTableIndex), groupConfig: groupConfig);
    }

    private void _initiateGroupLocally(CustomersGroup group, ServiceTable serviceTable, int[] groupConfig = null)
    {
        group.Populate(spawnPosition: _spawnPosition, groupConfig);
        group.AssingTable(serviceTable);
        serviceTable.TakeTable(group);

        group.OnGroupFinishedEating += () => 
        {
            group.Leave(exitPosition: _spawnPosition.position);
            serviceTable.ClearTable();
            serviceTable.FreeTable();
            _activeCustomers.Remove(group);
            _checkInGroupFromQueue();
        };
    }
}
