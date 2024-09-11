using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DayCountUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _dayNumberText;
    [SerializeField] private GameObject _panel;
    [SerializeField] private Image _progressCircle;
    [SerializeField] private Button _startDayButton;

    private TimingTimer _dayPassingTimer = new(defaultTimerValue: 0);

    private void Start()
    {
        GameManager.Instance.OnStartDay += _onDayStart;
        GameManager.Instance.OnStateChange += _updateVisibilityBasedOnGameState;

        _startDayButton.onClick.AddListener(() => GameManager.Instance.StartNewDay(GameManager.Instance.CurrentDay + 1));

        _hide();
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnStartDay -= _onDayStart;
        GameManager.Instance.OnStateChange -= _updateVisibilityBasedOnGameState;
    }

    private void Update()
    {
        if (_dayPassingTimer.IsActive)
        {
            _updateProgressVisualization(maxTime: _dayPassingTimer.MaxTime, currentTime: _dayPassingTimer.Time);
            _dayPassingTimer.SubtractTime(Time.deltaTime);

            if (_dayPassingTimer.IsTimerUp())
            {
                _dayPassingTimer.Deactivate();
                _dayPassingTimer.ResetTimer();
            }
        }
    }

    private void _updateVisibilityBasedOnGameState(GameState state)
    {
        if (state == GameState.Waiting) _hide();
        else _show();

        if (state == GameState.Editing && NetworkManager.Singleton.IsServer)_startDayButton.gameObject.SetActive(true);
        else _startDayButton.gameObject.SetActive(false);
    }

    private void _updateProgressVisualization(float maxTime, float currentTime)
    {
        if (maxTime == 0 || currentTime == 0) 
        {
            _progressCircle.fillAmount = 0; 
            return;
        }

        _progressCircle.fillAmount = currentTime / maxTime;
    }

    private void _onDayStart(int currentDay, float timer)
    {
        _updateDayText(currentDay);
        _setupTimer(timer);
    }

    private void _setupTimer(float time)
    {
        _dayPassingTimer = new(defaultTimerValue: time);
        _dayPassingTimer.ResetTimer();
        _dayPassingTimer.Activate();
    }

    private void _updateDayText(int currentDay)
    {
        _dayNumberText.text = _convertDayCountToString(currentDay);
    }

    private string _convertDayCountToString(float dayNumber)
    {
        return "DAY " + Math.Round(dayNumber).ToString();
    }

    private void _show() => _panel.SetActive(true);
    private void _hide() => _panel.SetActive(false);
}
