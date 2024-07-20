
using System.Collections;
using UnityEngine;

public class CookingProgressBarUI : ProgressBarUI
{
    private const string IS_FLASHING = "IsFlashing";

    [SerializeField] private StoveCounter _stove;
    [SerializeField] private GameObject _warningSymbol;
    [SerializeField] private Animator _anim;

    private TimingTimer _soundEffectTimer = new(defaultTimerValue: 0.25f);

    private float _warningThreashold = 0.5f;

    protected override void _updateProgressBar(float progressNormalized)
    {
        base._updateProgressBar(progressNormalized);

        if (_stove.GetCurrentState() == StoveCounter.State.Fried && progressNormalized > _warningThreashold)
        {
            _anim.SetBool(IS_FLASHING, true);
            _warningSymbol.SetActive(true);

            _soundEffectTimer.SubtractTime(Time.deltaTime);

            if (_soundEffectTimer.Timer <= 0f)
            {
                SoundManager.SoundEvents.TriggerOnWarningSound(_stove.transform.position);
                _soundEffectTimer.ResetTimer();
            }
        }
        else 
        {
            _soundEffectTimer.ResetTimer();
            _anim.SetBool(IS_FLASHING, false);
            _warningSymbol.SetActive(false);
        }
    }
}
