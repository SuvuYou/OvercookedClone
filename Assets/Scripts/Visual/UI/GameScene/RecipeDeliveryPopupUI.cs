using UnityEngine;

public class RecipeDeliveryPopupUI : MonoBehaviour
{
    private const string TRIGGER_POPUP = "Popup";

    [SerializeField] private Animator _anim;
    [SerializeField] private GameObject _successfulDeliveryPopup;
    [SerializeField] private GameObject _failedDeliveryPopup;

    private void Start()
    {
        // DeliveryManager.Instance.OnDeliveryFailed += _showFailedDeliveryPopup;
        // DeliveryManager.Instance.OnDeliverySuccess += _showSuccessfulDeliveryPopup;
    }

    private void OnDestroy()
    {
        // DeliveryManager.Instance.OnDeliveryFailed -= _showFailedDeliveryPopup;
        // DeliveryManager.Instance.OnDeliverySuccess -= _showSuccessfulDeliveryPopup;
    }

    private void _showSuccessfulDeliveryPopup()
    {
        _successfulDeliveryPopup.SetActive(true);
        _failedDeliveryPopup.SetActive(false);

        _anim.SetTrigger(TRIGGER_POPUP);
    }

    private void _showFailedDeliveryPopup()
    {
        _successfulDeliveryPopup.SetActive(false);
        _failedDeliveryPopup.SetActive(true);

        _anim.SetTrigger(TRIGGER_POPUP);
    }
}
