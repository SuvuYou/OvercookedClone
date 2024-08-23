using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BalanceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _balanceText;
    [SerializeField] private GameObject _holder;

    private void Start()
    {
        GameManager.Instance.OnBalanceUpdated += _updateBalanceText;
        GameManager.Instance.OnStateChange += _updateVisibilityBasedOnGameState;

        _hide();
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnBalanceUpdated -= _updateBalanceText;
        GameManager.Instance.OnStateChange -= _updateVisibilityBasedOnGameState;
    }

    private void _updateVisibilityBasedOnGameState(GameState state)
    {
        if (state == GameState.Waiting) _hide();
        else _show();
    }

    private void _updateBalanceText(float balance)
    {
        _balanceText.text = _convertBalanceToString(balance);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_balanceText.rectTransform);
    }

    private string _convertBalanceToString(float balance)
    {
        return "Balance " + Math.Round(balance).ToString() + "$";
    }

    private void _show() => _holder.SetActive(true);
    private void _hide() => _holder.SetActive(false);
}
