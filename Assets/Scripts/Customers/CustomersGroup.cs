using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomersGroup : MonoBehaviour
{
    private Dictionary<int, float> _customerCountToPriceMultiplier = new()
    {
        {1, 1.1f},
        {2, 1.2f},
        {3, 1.3f},
        {4, 1.4f},
    };

    public event Action OnGroupFinishedEating;

    private const int MIN_CUSTOMERS_IN_GROUP = 1;
    private const int MAX_CUSTOMERS_IN_GROUP = 4;

    [SerializeField] private Customer _customerPrefab;
    public List<Customer> Customers { get; private set; } = new();
    public int CustomersCount { get; private set; }

    // saves the group as template for easy recreation; saves indecies of each customer's recipe
    private int[] _groupConfig = new int[MAX_CUSTOMERS_IN_GROUP];
    public int[] GroupConfig { get => _groupConfig; }

    public void InitGroupSize(int forcedGroupSize = -1)
    {
        if (forcedGroupSize == -1) CustomersCount = UnityEngine.Random.Range(MIN_CUSTOMERS_IN_GROUP, MAX_CUSTOMERS_IN_GROUP + 1);
        else CustomersCount = forcedGroupSize;
    }

    public void Populate(Transform spawnPosition, int[] groupConfig)
    {
        groupConfig ??= new int[MAX_CUSTOMERS_IN_GROUP] {-1, -1, -1, -1};

        for (int i = 0; i < CustomersCount; i++)
        {
            Customer customer = Instantiate(_customerPrefab, spawnPosition);
            customer.SetPriceMultiplier(priceMutiplier: _customerCountToPriceMultiplier[CustomersCount]);
            Customers.Add(customer);

            int recipeIndex = customer.MakeAnOrder(forceRecipeIndex: groupConfig[i]);
            _groupConfig[i] = recipeIndex;

            customer.OnFinishEating += _checkIsGroupFinishedEating;
            customer.OnCustomerLeaving += _reduceCustomerCount;
        }
    }

    public void AssingTable(ServiceTable table)
    {
        for (int i = 0; i < CustomersCount; i++)
        {
            Customers[i].AssingChair(table.ActiveChairs[i]);
        }
    }

    public void Leave(Vector3 exitPosition)
    {
        for (int i = 0; i < CustomersCount; i++)
        {
            foreach (Customer customer in Customers)
            {
                customer.Leave(exitPosition);
            }
        }
    }

    private void _reduceCustomerCount()
    {
        CustomersCount--;

        if (CustomersCount == 0)
        {
            Destroy(this.gameObject);
        }
    }

    private void _checkIsGroupFinishedEating()
    {
        if (_isGroupFinishedEating())
        {
            OnGroupFinishedEating?.Invoke();
        }
    }

    private bool _isGroupFinishedEating()
    {
        foreach (Customer customer in Customers)
        {
            if (!customer.IsFinishedEating) return false;
        }

        return true;
    }
}
