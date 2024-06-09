using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ProgressTrackerSO", menuName = "ScriptableObjects/ProgressTrackerSO")]
public class ProgressTrackerSO : ScriptableObject
{
    public float Progress { get; private set; }
    public float MaxProgress { get; private set; }
    public float ProgressNormalized { get; private set; }
    public event Action<float> OnUpdateProgress;

    public void TriggerProgressUpdate(float newProgress)
    {
        Progress = newProgress;

        if (MaxProgress == 0)
        {
            ProgressNormalized = 0; 
        }
        else
        {
            ProgressNormalized = Progress / MaxProgress; 
        }

        OnUpdateProgress?.Invoke(ProgressNormalized);
    }

    public void SetMaxProgress (float maxProgress) => MaxProgress = maxProgress;
}
