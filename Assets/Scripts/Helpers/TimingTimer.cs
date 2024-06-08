public struct TimingTimer
{
    public float Timer { get; private set; }
    public float DefaultTimerValue;

    public TimingTimer (float defaultTimerValue){
        Timer = defaultTimerValue;
        DefaultTimerValue = defaultTimerValue;
    }

    public void SubtractTime(float timeAmount)
    {
        Timer -= timeAmount;
    }

    public void ResetTimer()
    {
        Timer = DefaultTimerValue;
    }
}
