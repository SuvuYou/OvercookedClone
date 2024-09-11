using UnityEngine;

public class CustomersQueueUI : MonoBehaviour
{
    [SerializeField] private CustomersSpawnerManager _customersSpawner;
    [SerializeField] private CustomerGroupInQueueUI _singleGroupUIPrefab;
    [SerializeField] private Transform _listHolder;

    private void Start()
    {       
        _customersSpawner.OnQueueUpdated += _displayQueueList;
    }

    private void OnDestroy()
    {       
        _customersSpawner.OnQueueUpdated -= _displayQueueList;
    }

    private void _displayQueueList(int[] queue)
    {
        foreach(Transform child in _listHolder)
        {
            Destroy(child.gameObject);
        }

        bool isEmpty = true;

        for (int i = 0; i < queue.Length; i++)
        {
            if (queue[i] <= 0) continue;

            isEmpty = false;
            CustomerGroupInQueueUI UIelem = Instantiate(_singleGroupUIPrefab, _listHolder);
            UIelem.Init(groupSize: queue[i]);
        }

        if (isEmpty && this.isActiveAndEnabled) this.gameObject.SetActive(false);
        else this.gameObject.SetActive(true);
    }
}
