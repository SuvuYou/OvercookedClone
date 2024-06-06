using UnityEngine;

[CreateAssetMenu(fileName = "SoundsListSO", menuName = "ScriptableObjects/SoundsListSO")]
public class SoundsListSO : ScriptableObject
{
    public AudioClip[] Chop; 
    public AudioClip[] DeliveryFailed; 
    public AudioClip[] DeliverySuccess; 
    public AudioClip[] ObjectDrop; 
    public AudioClip[] ObjectPickup; 
    public AudioClip[] Sizzle; 
    public AudioClip[] Trash; 
    public AudioClip[] Warning; 
    public AudioClip[] Walking; 
}
