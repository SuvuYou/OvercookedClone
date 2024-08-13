using System;
using UnityEngine;

public class CountdownUIManager : MonoBehaviour
{
    [SerializeField] private CountdownNumberAnimation _countdownNumber;
    [SerializeField] private GameObject _countdownPanel;

    private void Start()
    {
        _countdownPanel.SetActive(false);

        GameManager.Instance.OnStateChange += _updateUIBasedOnGameState;
        GameManager.Instance.OnCountdownTimerChange += _updateCoundownNumber;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnStateChange -= _updateUIBasedOnGameState;
        GameManager.Instance.OnCountdownTimerChange -= _updateCoundownNumber;
    }

    private void _updateUIBasedOnGameState(GameState newState)
    {
        _countdownPanel.SetActive(newState == GameState.Countdown); 
    }

    private void _updateCoundownNumber(float newTime)
    {
        _countdownNumber.SetNumberText(Math.Ceiling(newTime).ToString());
    }
}
