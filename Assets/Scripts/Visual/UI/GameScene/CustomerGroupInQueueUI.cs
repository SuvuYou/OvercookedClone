using TMPro;
using UnityEngine;

public class CustomerGroupInQueueUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _customersCountText;

    public void Init(int groupSize)
    {   
        _setCustomerCount(groupSize);
    }

    private void _setCustomerCount(int numberOfCustomers)
    {
        _customersCountText.text = numberOfCustomers.ToString();
    }
}
