using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomersGroup : MonoBehaviour
{
    public event Action OnGroupFinishedEating;

    private const int MIN_CUSTOMERS_IN_GROUP = 1;
    private const int MAX_CUSTOMERS_IN_GROUP = 4;

    [SerializeField] private Customer _customerPrefab;
    public List<Customer> Customers { get; private set; } = new();
    public int CustomersCount { get; private set; }

    private void Awake()
    {
        CustomersCount = UnityEngine.Random.Range(MIN_CUSTOMERS_IN_GROUP, MAX_CUSTOMERS_IN_GROUP + 1);
    }

    public void Populate(Transform spawnPosition)
    {
        for (int i = 0; i < CustomersCount; i++)
        {
            Customer customer = Instantiate(_customerPrefab, spawnPosition);
            customer.MakeAnOrder();
            Customers.Add(customer);

            customer.OnFinishEating += _checkIsGroupFinishedEating;
            customer.OnCustomerLeaving += _checkIsGroupLeft;
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

    private void _checkIsGroupLeft()
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
