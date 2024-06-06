using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SelectedCounterSO", menuName = "ScriptableObjects/SelectedCounterSO")]
public class SelectedCounterSO : ScriptableObject
{
    public event Action<BaseCounter> OnSelectCounter;
    public BaseCounter SelectedCounter { get; private set; }

    private void OnEnable()
    {
        OnSelectCounter += _updateSelectedCounter;
    }

    private void OnDisable()
    {
        OnSelectCounter -= _updateSelectedCounter;
    }

    private void _updateSelectedCounter(BaseCounter counter)
    {
        SelectedCounter = counter;
    }

    public void TriggerSelectCounterEvent(BaseCounter counter)
    {
        if (counter != SelectedCounter) OnSelectCounter?.Invoke(counter);
    }
}
