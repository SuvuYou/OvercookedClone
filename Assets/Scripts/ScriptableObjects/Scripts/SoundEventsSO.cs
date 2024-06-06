using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundEventsSO", menuName = "ScriptableObjects/SoundEventsSO")]
public class SoundEventsSO : ScriptableObject
{
    public event Action<Vector3> OnCutSound;
    public event Action<Vector3> OnDeliverFailedSound;
    public event Action<Vector3> OnDeliverSuccessSound;
    public event Action<Vector3> OnObjectDropSound;
    public event Action<Vector3> OnObjectPickupSound;    
    public event Action<Vector3> OnTrashSound;
    public event Action<Vector3> OnWalkingSound;
    public event Action<Vector3> OnWarningSound;

    public void TriggerOnCutSound (Vector3 soundSourcePosition) => OnCutSound?.Invoke(soundSourcePosition);
    public void TriggerOnDeliverFailedSound (Vector3 soundSourcePosition) => OnDeliverFailedSound?.Invoke(soundSourcePosition);
    public void TriggerOnDeliverSuccessSound (Vector3 soundSourcePosition) => OnDeliverSuccessSound?.Invoke(soundSourcePosition);
    public void TriggerOnObjectDropSound (Vector3 soundSourcePosition) => OnObjectDropSound?.Invoke(soundSourcePosition);
    public void TriggerOnObjectPickupSound (Vector3 soundSourcePosition) => OnObjectPickupSound?.Invoke(soundSourcePosition);
    public void TriggerOnTrashSound (Vector3 soundSourcePosition) => OnTrashSound?.Invoke(soundSourcePosition);
    public void TriggerOnWalkingSound (Vector3 soundSourcePosition) => OnWalkingSound?.Invoke(soundSourcePosition);
    public void TriggerOnWarningSound (Vector3 soundSourcePosition) => OnWarningSound?.Invoke(soundSourcePosition);
}
