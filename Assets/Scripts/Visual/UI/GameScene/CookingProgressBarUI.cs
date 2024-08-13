
using System.Collections;
using UnityEngine;

public class CookingProgressBarUI : ProgressBarUI
{
    private const string IS_FLASHING = "IsFlashing";
    private const float WARNING_THREAHOLD = 0.5f;

    [SerializeField] private StoveCounter _stove;
    [SerializeField] private GameObject _warningSymbol;
    [SerializeField] private Animator _anim;

    private TimingTimer _soundEffectTimer = new(defaultTimerValue: 0.25f);

    protected override void _updateProgressBar(float progressNormalized)
    {
        base._updateProgressBar(progressNormalized);
        if (_stove.GetCurrentState() == StoveCounter.FryingState.Fried && progressNormalized > WARNING_THREAHOLD)
        {
            _setIsAnimationActive(isActive: true);

            _soundEffectTimer.SubtractTime(Time.deltaTime);

            if (_soundEffectTimer.IsTimerUp())
            {
                SoundManager.SoundEvents.TriggerOnWarningSound(_stove.transform.position);

                _soundEffectTimer.ResetTimer();
            }
        }
        else 
        {
            _soundEffectTimer.ResetTimer();
            _setIsAnimationActive(isActive: false);
        }
    }

    private void _setIsAnimationActive(bool isActive)
    {
        _anim.SetBool(IS_FLASHING, isActive);
        _warningSymbol.SetActive(isActive);
    }
}
