using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CustomersSpawnerManager : NetworkBehaviour
{
    public event Action<int[]> OnQueueUpdated;

    [SerializeField] private Transform _spawnPosition;
    [SerializeField] private CustomersGroup _customersGroupPrefab;
    [SerializeField] private ServiceTablesManager _serviceTablesManager;
    [SerializeField] private AvailableRecipesListSO _availableRecipesList;

    private List<CustomersGroup> _activeCustomers = new();
    private List<CustomersGroup> _customersQueue = new();

    private const int MAX_GROUPS_WAITING_COUNT = 1; 
    private const float MIN_TIMEOUT_BETWEEN_GROUPS_SPAWNED = 2f; 
    private const float MAX_TIMEOUT_BETWEEN_GROUPS_SPAWNED = 10f; 
    private TimingTimer _spawningGroupTimer = new(minDefaultTimerValue: MIN_TIMEOUT_BETWEEN_GROUPS_SPAWNED, maxDefaultTimerValue: MAX_TIMEOUT_BETWEEN_GROUPS_SPAWNED);

    private void Start()
    {
        GameManager.Instance.OnEndDay += _resetGroups;
    }

    public override void OnDestroy()
    {
        GameManager.Instance.OnEndDay -= _resetGroups;
    }

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

    private void _resetGroups()
    {
        _serviceTablesManager.ClearAllTables(exitPosition: _spawnPosition.position);

        _activeCustomers.Clear();
        _customersQueue.Clear();
    }

    private void _handleSpawningGroupsForQueue()
    {
        _spawningGroupTimer.SubtractTime(Time.deltaTime);

        if (_spawningGroupTimer.IsTimerUp() && _customersQueue.Count < MAX_GROUPS_WAITING_COUNT)
        {
            _spawningGroupTimer.ResetTimer();

            var placedMapItems = TileMapGrid.Instance.GetAllPlacedItems().Select(item => item.PurchasableItemReference).ToList();
            var availableRecipes = _availableRecipesList.GetAvailableRecipes(placedMapItems);

            if (availableRecipes.Count <= 0) return;

            CustomersGroup group = Instantiate(_customersGroupPrefab, _spawnPosition);
            group.InitGroupSize();

            _customersQueue.Add(group);
            _triggerUpdateQueueEvent();

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
            if (_customersQueue.Contains(group)) 
            {
                _customersQueue.Remove(group);
                _triggerUpdateQueueEvent();
            }
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
            serviceTable.FreeTable();

            if (IsServer)
            {
                _activeCustomers.Remove(group);
                _checkInGroupFromQueue();
            }
        };
    }

    private void _triggerUpdateQueueEvent()
    {
        int[] queueOfSizes = new int[MAX_GROUPS_WAITING_COUNT];

        for(int i = 0; i < _customersQueue.Count; i++)
        {
            queueOfSizes[i] = _customersQueue[i].CustomersCount;
        }

        _triggerUpdateQueueEventLocally(queueOfSizes);
        _triggerUpdateQueueEventServerRpc(queueOfSizes);
    }

    [ServerRpc]
    private void _triggerUpdateQueueEventServerRpc(int[] queueOfSizes)
    {
        _triggerUpdateQueueEventClientRpc(queueOfSizes);
    }
    
    [ClientRpc]
    private void _triggerUpdateQueueEventClientRpc(int[] queueOfSizes)
    {
        if (NetworkManager.Singleton.IsServer) return;

        _triggerUpdateQueueEventLocally(queueOfSizes);
    }

    private void _triggerUpdateQueueEventLocally(int[] queueOfSizes)
    {
        OnQueueUpdated?.Invoke(queueOfSizes);
    }
    
}
