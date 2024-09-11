
using System;

public struct TimingTimer
{
    public float Time { get; private set; }
    public float MaxTime { get; private set; }
    public bool IsActive { get; private set; }
    private float _minDefaultTimerValue;
    private float _maxDefaultTimerValue;
    private Random _random;

    public TimingTimer (float defaultTimerValue = 0f){
        _minDefaultTimerValue = defaultTimerValue;
        _maxDefaultTimerValue = defaultTimerValue;
        _random = new Random();
        
        Time = 0f;
        MaxTime = 0f;
        IsActive = true;

        ResetTimer();
    }

    public TimingTimer (float minDefaultTimerValue, float maxDefaultTimerValue){
        IsActive = true;
        _minDefaultTimerValue = minDefaultTimerValue;
        _maxDefaultTimerValue = maxDefaultTimerValue;
        _random = new Random();

        Time = 0f;
        MaxTime = 0f;
        IsActive = true;

        ResetTimer();
    }

    public void SetDefaultTimerTime (float defaultTimerValue){
        _minDefaultTimerValue = defaultTimerValue;
        _maxDefaultTimerValue = defaultTimerValue;
        _random = new Random();

        Time = 0f;
        MaxTime = 0f;

        ResetTimer();
    }

    public void SetDefaultTimerTime (float minDefaultTimerValue, float maxDefaultTimerValue){
        _minDefaultTimerValue = minDefaultTimerValue;
        _maxDefaultTimerValue = maxDefaultTimerValue;
        _random = new Random();

        Time = 0f;
        MaxTime = 0f;

        ResetTimer();
    }

    public void Activate () => IsActive = true;

    public void Deactivate () => IsActive = false;

    public readonly bool IsTimerUp() => Time <= 0f;

    public void ForceFinish() => Time = 0f;  
    
    public void SubtractTime(float timeAmount) => Time -= timeAmount;
    
    public void ResetTimer() 
    {
        Time = _getRandomTimeInRange();
        MaxTime = Time;
    }

    private float _getRandomTimeInRange()
    {
        return ((float)_random.NextDouble() * (_maxDefaultTimerValue - _minDefaultTimerValue)) + _minDefaultTimerValue;
    }
}
