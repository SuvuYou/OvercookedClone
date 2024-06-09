using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public const string MUSIC_VOLUME_PLAYER_PREF = "MusicVolume";
    private const float INITIAL_VOLUME = 0.5f;

    public static VolumeManager Volume = new();
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        if (PlayerPrefs.HasKey(MUSIC_VOLUME_PLAYER_PREF))
        {
            Volume.SetVolume(PlayerPrefs.GetFloat(MUSIC_VOLUME_PLAYER_PREF));
        }
        else
        {
            Volume.SetVolume(INITIAL_VOLUME);
        }

        _updateVolume(Volume.Volume);
    }

    private void Start()
    {
        Volume.OnVolumeChange += _updateVolume; 
    }

    private void OnDestroy()
    {
        Volume.OnVolumeChange -= _updateVolume; 
    }

    private void _updateVolume (float volume)
    {
        _audioSource.volume = volume;
    }
}
