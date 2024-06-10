using UnityEngine;

public struct TimingTimer
{
    public float Timer { get; private set; }
    private float _minDefaultTimerValue;
    private float _maxDefaultTimerValue;

    public TimingTimer (float defaultTimerValue){
        _minDefaultTimerValue = defaultTimerValue;
        _maxDefaultTimerValue = defaultTimerValue;

        Timer = 0f;
        ResetTimer();
    }

    public TimingTimer (float minDefaultTimerValue, float maxDefaultTimerValue){
        _minDefaultTimerValue = minDefaultTimerValue;
        _maxDefaultTimerValue = maxDefaultTimerValue;

        Timer = 0f;
        ResetTimer();
    }

    public void SubtractTime(float timeAmount)
    {
        Timer -= timeAmount;
    }

    public void ResetTimer()
    {
        Timer = GetRandomTimeInRange();
    }

    public float GetRandomTimeInRange()
    {
        return Random.Range(_minDefaultTimerValue, _maxDefaultTimerValue);
    }
}
