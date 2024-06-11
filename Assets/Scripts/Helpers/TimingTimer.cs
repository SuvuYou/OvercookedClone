
using System;

public struct TimingTimer
{
    public float Timer { get; private set; }
    private float _minDefaultTimerValue;
    private float _maxDefaultTimerValue;
    private Random _random;

    public TimingTimer (float defaultTimerValue){
        _minDefaultTimerValue = defaultTimerValue;
        _maxDefaultTimerValue = defaultTimerValue;
        _random = new Random();

        Timer = 0f;
        ResetTimer();
    }

    public TimingTimer (float minDefaultTimerValue, float maxDefaultTimerValue){
        _minDefaultTimerValue = minDefaultTimerValue;
        _maxDefaultTimerValue = maxDefaultTimerValue;
        _random = new Random();

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
        return ((float)_random.NextDouble() * (_maxDefaultTimerValue - _minDefaultTimerValue)) + _minDefaultTimerValue;
    }
}
