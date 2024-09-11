using System.Collections.Generic;
using UnityEngine;

public class ServiceTable : MonoBehaviour
{
    [SerializeField] private Chair[] _allDisabledAndEnabledChairs;

    public List<Chair> ActiveChairs {get; private set; } = new();
    public int ActiveChairsCount { get => ActiveChairs.Count; }
    
    private CustomersGroup _currentGroup;
    public bool IsAvailable { get => _currentGroup == null; }

    private void Start()
    {
        foreach(Chair chair in _allDisabledAndEnabledChairs)
        {
            if (chair.isActiveAndEnabled)
            {
                ActiveChairs.Add(chair);
            }
        }
    } 

    public void TakeTable (CustomersGroup group) => _currentGroup = group;
    
    public void FreeTable (Vector3 exitPosition) 
    {
        _clearTable();

        if (_currentGroup == null) return;

        _currentGroup.Leave(exitPosition);
        _currentGroup = null;
    } 

    private void _clearTable()
    {
        foreach(Chair chair in ActiveChairs)
        {
            if (chair.IsHoldingItem()) chair.DestroyCurrentItemHeld();
        }
    }
}
