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
}
