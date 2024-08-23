using TMPro;
using UnityEngine;

public class CustomerPayingPopupUI : MonoBehaviour
{
    private const string TRIGGER_POPUP = "Popup";

    [SerializeField] private Customer _customer;
    [SerializeField] private Animator _anim;
    [SerializeField] private TextMeshProUGUI _moneyAmountText;

    private void Start()
    {
        _anim.gameObject.SetActive(false);
        _customer.OnRecieveOrder += _triggerPopupMoneyPayment;
    }

    private void OnDestroy()
    {
        _customer.OnRecieveOrder -= _triggerPopupMoneyPayment;
    }

    private void _triggerPopupMoneyPayment()
    {
        _anim.gameObject.SetActive(true);
        _moneyAmountText.text = _getMoneyAmountText();

        _anim.SetTrigger(TRIGGER_POPUP);
    }

    private string _getMoneyAmountText()
    {
        return "+" + _customer.Order.Price + " x" + _customer.PriceMutiplier;
    }

}
