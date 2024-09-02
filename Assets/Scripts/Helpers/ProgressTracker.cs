using System;

public class ProgressTracker
{
    public event Action<float> OnUpdateProgress;

    public float Progress { get; private set; }
    public float MaxProgress { get; private set; }
    // from 0 to 1
    public float ProgressNormalized { get; private set; }

    public ProgressTracker(float maxProgress = 0)
    {
        MaxProgress = maxProgress;
        Progress = 0;
    }

    public void SetMaxProgress (float maxProgress) => MaxProgress = maxProgress;
    
    public void TriggerProgressUpdate(float newProgress)
    {
        Progress = newProgress;

        ProgressNormalized = _normalizeProgress(currentProgress: newProgress);
        ProgressNormalized = _applyBoundaries(subjectNumber: ProgressNormalized);

        OnUpdateProgress?.Invoke(ProgressNormalized);
    }

    private float _normalizeProgress(float currentProgress)
    {
        if (MaxProgress == 0) return 0;
        else return currentProgress / MaxProgress;
    }

    private float _applyBoundaries(float subjectNumber, float lowerBoundary = 0, float upperBoundary = 1)
    {
        if (subjectNumber < lowerBoundary) subjectNumber = lowerBoundary;
        if (subjectNumber > upperBoundary) subjectNumber = upperBoundary;

        return subjectNumber;
    }
}
