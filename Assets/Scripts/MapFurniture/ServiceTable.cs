using System.Collections.Generic;
using UnityEngine;

public class ServiceTable : MonoBehaviour
{
    [SerializeField] private Chair[] _allDisabledAndEnabledChairs;

    public List<Chair> ActiveChairs {get; private set; } = new();
    public int ActiveChairsCount { get => ActiveChairs.Count; }
    
    public CustomersGroup CurrentGroup { get; private set; }
    public bool IsAvailable { get => CurrentGroup == null; }

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

    public void TakeTable (CustomersGroup group) => CurrentGroup = group;
    
    public void FreeTable() 
    {
        _clearTable();
        
        CurrentGroup = null;
    } 

    private void _clearTable()
    {
        foreach(Chair chair in ActiveChairs)
        {
            if (chair.IsHoldingItem()) chair.DestroyCurrentItemHeld();
        }
    }
}
