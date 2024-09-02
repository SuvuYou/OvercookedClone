using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private Image _progressBar;

    private ProgressTracker _progressTracker;

    private void Start()
    {
        _hide();
    }

    private void OnDestroy()
    {
        if (_progressTracker == null) return;

        _progressTracker.OnUpdateProgress -= _updateProgressBar;
    }

    public void Init(ProgressTracker progressTracker)
    {
        _progressTracker = progressTracker;
        _progressTracker.OnUpdateProgress += _updateProgressBar;
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
