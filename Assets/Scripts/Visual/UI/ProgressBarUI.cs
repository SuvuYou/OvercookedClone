using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private ProgressTrackerSO _progress;
    [SerializeField] private Image _progressBar;

    private void Start()
    {
        _progress.OnUpdateProgress += _updateProgressBar;
        _hide();
    }

    private void OnDestroy()
    {
        _progress.OnUpdateProgress -= _updateProgressBar;
    }

    protected virtual void _updateProgressBar(float progressNormalized)
    {
        _progressBar.fillAmount = progressNormalized;

        if (progressNormalized == 0 || progressNormalized == 1) _hide();
        else _show();
    }

    private void _show() => gameObject.SetActive(true);
    private void _hide() => gameObject.SetActive(false);
}
