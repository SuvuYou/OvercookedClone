using UnityEngine;

[CreateAssetMenu(fileName = "FryableFoodSO", menuName = "ScriptableObjects/FryableFoodSO")]
public class FryableFoodSO : ScriptableObject
{
    public KitchenItem FriedPrefab;
    public StoveCounter.FryingState State;
    public float FryingTimer;
}
