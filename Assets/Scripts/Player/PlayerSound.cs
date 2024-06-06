using System.Collections;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [SerializeField] private PlayerStateSO _playerState;

    private bool _isPlayingSound = false;
    private const float _walkingSoundRestTime = 0.2f;

    private void Update()
    {
        if (_playerState.IsWalking && !_isPlayingSound)
        {
            StartCoroutine(_triggerWalkingSoundCoroutine());
        }
    }

    private IEnumerator _triggerWalkingSoundCoroutine()
    {
        _isPlayingSound = true;
        SoundManager.SoundEvents.TriggerOnWalkingSound(transform.position);

        yield return new WaitForSeconds(_walkingSoundRestTime);

        _isPlayingSound = false;
    }
}
