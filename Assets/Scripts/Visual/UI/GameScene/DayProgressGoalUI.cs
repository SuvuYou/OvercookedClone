using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayProgressGoalUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _currentProgressText;
    [SerializeField] private TextMeshProUGUI _currentProgressGoalText;
    [SerializeField] private GameObject _holder;

    private void Start()
    {
        GameManager.Instance.OnStateChange += _updateProgressGoalText;
        GameManager.Instance.CurrentDayProgress.OnValueChanged += _updateProgressText;

        _hide();
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnStateChange -= _updateProgressGoalText;
        GameManager.Instance.CurrentDayProgress.OnValueChanged -= _updateProgressText;
    }

    private void _updateProgressText(float prev, float next)
    {
        _currentProgressText.text = _convertBalanceToString(balance: GameManager.Instance.CurrentDayProgress.Value, prefix: "Current: ");
        LayoutRebuilder.ForceRebuildLayoutImmediate(_currentProgressText.rectTransform);
    }

    private void _updateProgressGoalText(GameState state)
    {
        if (state == GameState.Waiting) _hide();
        else _show();

        _currentProgressGoalText.text = _convertBalanceToString(balance: GameManager.Instance.CurrentDayProgressGoal, prefix: "Goal: ");
        LayoutRebuilder.ForceRebuildLayoutImmediate(_currentProgressGoalText.rectTransform);
    }

    private string _convertBalanceToString(float balance, string prefix)
    {
        return prefix + " " + Math.Round(balance).ToString() + "$";
    }

    private void _show() => _holder.SetActive(true);
    private void _hide() => _holder.SetActive(false);
}
