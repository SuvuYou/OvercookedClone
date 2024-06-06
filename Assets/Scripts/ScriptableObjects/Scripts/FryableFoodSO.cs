using UnityEngine;

[CreateAssetMenu(fileName = "FryableFoodSO", menuName = "ScriptableObjects/FryableFoodSO")]
public class FryableFoodSO : ScriptableObject
{
    public KitchenItem FriedPrefab;
    public float FryingTimer;
}
