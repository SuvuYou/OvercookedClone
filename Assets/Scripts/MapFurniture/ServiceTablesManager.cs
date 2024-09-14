using System;
using UnityEngine;

public class ServiceTablesManager : MonoBehaviour
{
    [SerializeField] private ServiceTable[] _tables;

    public ServiceTable FindAvailableTableForGroup(CustomersGroup group)
    {
        ServiceTable mostSuitableTable = null;

        foreach (ServiceTable table in _tables)
        {
            if (!table.IsAvailable) continue;

            if (group.CustomersCount > table.ActiveChairsCount) continue;

            if (mostSuitableTable == null || mostSuitableTable.ActiveChairsCount > table.ActiveChairsCount) mostSuitableTable = table;
        }

        return mostSuitableTable;
    }

    public void ClearAllTables(Vector3 exitPosition)
    {
        foreach (ServiceTable table in _tables)
        {
            if (table.CurrentGroup != null)
            {   
                table.CurrentGroup.Leave(exitPosition);
            }

            table.FreeTable();
        }
    }

    public int GetIndexByTable(ServiceTable table) 
    {
        return Array.IndexOf(array: _tables, value: table);
    }
    
    public ServiceTable GetTableByIndex(int index) 
    {
        return _tables[index];
    }
}
