using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public const string SFX_SOUND_VOLUME_PLAYER_PREF = "SFXVolume";
    private const float INITIAL_VOLUME = 0.5f;

    public static VolumeManager Volume = new();
    public static SoundEventsSO SoundEvents; 
    [SerializeField] private SoundEventsSO _soundEvents;
    [SerializeField] private SoundsListSO _soundsList;
    
    void Awake ()
    {
        SoundEvents = _soundEvents;

        if (PlayerPrefs.HasKey(SFX_SOUND_VOLUME_PLAYER_PREF))
        {
            Volume.SetVolume(PlayerPrefs.GetFloat(SFX_SOUND_VOLUME_PLAYER_PREF));
        }
        else
        {
            Volume.SetVolume(INITIAL_VOLUME);
        }
    }

    void Start()
    {
        SoundEvents.OnCutSound += (Vector3 pos) => _playSoundAtPoint(_soundsList.Chop, pos);
        SoundEvents.OnDeliverFailedSound += (Vector3 pos) => _playSoundAtPoint(_soundsList.DeliveryFailed, pos);
        SoundEvents.OnDeliverSuccessSound += (Vector3 pos) => _playSoundAtPoint(_soundsList.DeliverySuccess, pos);
        SoundEvents.OnObjectDropSound += (Vector3 pos) => _playSoundAtPoint(_soundsList.ObjectDrop, pos);
        SoundEvents.OnObjectPickupSound += (Vector3 pos) => _playSoundAtPoint(_soundsList.ObjectPickup, pos);
        SoundEvents.OnTrashSound += (Vector3 pos) => _playSoundAtPoint(_soundsList.Trash, pos);
        SoundEvents.OnWalkingSound += (Vector3 pos) => _playSoundAtPoint(_soundsList.Walking, pos);
        SoundEvents.OnWarningSound += (Vector3 pos) => _playSoundAtPoint(_soundsList.Warning, pos);
    }

    private void _playSoundAtPoint(AudioClip[] clips, Vector3 position, float volumeMultiplier = 1f)
    {
        AudioSource.PlayClipAtPoint(clips[Random.Range(0, clips.Length - 1)], position, Volume.Volume * volumeMultiplier);
    }
}
