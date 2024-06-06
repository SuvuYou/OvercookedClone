using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{
    [SerializeField] StoveCounter _stove;
    [SerializeField] private AudioSource _audioSource;
    private bool _isSoundBeingPlayedCached;

    private void Start()
    {
        _stove.OnStoveOn += () => _turnStoveSizzleOn();
        _stove.OnStoveOff += () => _turnStoveSizzleOff();
        GameManager.Instance.OnPause += _updateSoundOnPause;
    }

    private void OnDestroy()
    {
        _stove.OnStoveOn -= () => _turnStoveSizzleOn();
        _stove.OnStoveOff -= () => _turnStoveSizzleOff();
        GameManager.Instance.OnPause -= _updateSoundOnPause;
    }

    private void _updateSoundOnPause(bool isPaused)
    {
        if (isPaused) _turnStoveSizzleOff(shouldUpdateCache: false);
        else if (_isSoundBeingPlayedCached) _turnStoveSizzleOn();
    }

    private void _turnStoveSizzleOn(bool shouldUpdateCache = true)
    {
        if (shouldUpdateCache) _isSoundBeingPlayedCached = false;
        _audioSource.volume = SoundManager.Volume.Volume;
        _audioSource.Play();
    }

    private void _turnStoveSizzleOff(bool shouldUpdateCache = true)
    {
        if (shouldUpdateCache) _isSoundBeingPlayedCached = false;
        _audioSource.Stop();
    }
}
