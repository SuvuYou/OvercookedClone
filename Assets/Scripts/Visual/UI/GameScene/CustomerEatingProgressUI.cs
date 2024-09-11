using UnityEngine;
using UnityEngine.UI;


public class CustomerEatingProgressUI : MonoBehaviour
{
    [SerializeField] private Image _progressCircle;
    [SerializeField] private Customer _customer;

    private ProgressTracker _progressTracker;

    private void Start()
    {
        _progressTracker = _customer.EatingProgressTracker;
        _progressTracker.OnUpdateProgress += _updateProgressCircle;

        _customer.OnRecieveOrder += _show;
        _customer.OnFinishEating += _hide;

        _hide();
    }

    private void OnDestroy()
    {
        _progressTracker.OnUpdateProgress -= _updateProgressCircle;
        _customer.OnRecieveOrder -= _show;
        _customer.OnFinishEating -= _hide;
    }

    private void _updateProgressCircle(float progressNormalized)
    {
        _progressCircle.fillAmount = progressNormalized;

        if (progressNormalized == 0 || progressNormalized == 1) _hide();
        else _show();
    }

    private void _show() => gameObject.SetActive(true);
    private void _hide() => gameObject.SetActive(false);
}
