using UnityEngine;

[CreateAssetMenu(fileName = "SliceableFoodSO", menuName = "ScriptableObjects/SliceableFoodSO")]
public class SliceableFoodSO : ScriptableObject
{
    public KitchenItem SlicedPrefab;
    public int CuttingSlicesCount;
}
