
using System;

public struct TimingTimer
{
    public float Time { get; private set; }
    private float _minDefaultTimerValue;
    private float _maxDefaultTimerValue;
    private Random _random;

    public TimingTimer (float defaultTimerValue = 0f){
        _minDefaultTimerValue = defaultTimerValue;
        _maxDefaultTimerValue = defaultTimerValue;
        _random = new Random();
        
        Time = 0f;
        ResetTimer();
    }

    public TimingTimer (float minDefaultTimerValue, float maxDefaultTimerValue){
        _minDefaultTimerValue = minDefaultTimerValue;
        _maxDefaultTimerValue = maxDefaultTimerValue;
        _random = new Random();

        Time = 0f;
        ResetTimer();
    }

    public void SetDefaultTimerTime (float defaultTimerValue){
        _minDefaultTimerValue = defaultTimerValue;
        _maxDefaultTimerValue = defaultTimerValue;
        _random = new Random();

        Time = 0f;
        ResetTimer();
    }

    public void SetDefaultTimerTime (float minDefaultTimerValue, float maxDefaultTimerValue){
        _minDefaultTimerValue = minDefaultTimerValue;
        _maxDefaultTimerValue = maxDefaultTimerValue;
        _random = new Random();

        Time = 0f;
        ResetTimer();
    }

    public void ForceFinish()
    {
        Time = 0f;  
    }

    public void SubtractTime(float timeAmount)
    {
        Time -= timeAmount;
    }

    public readonly bool IsTimerUp() => Time <= 0f;
    

    public void ResetTimer()
    {
        Time = _getRandomTimeInRange();
    }

    private float _getRandomTimeInRange()
    {
        return ((float)_random.NextDouble() * (_maxDefaultTimerValue - _minDefaultTimerValue)) + _minDefaultTimerValue;
    }
}
