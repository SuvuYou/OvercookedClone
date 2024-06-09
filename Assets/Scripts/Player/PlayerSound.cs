using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerSound : NetworkBehaviour
{
    [SerializeField] private PlayerStateSO _playerState;

    private bool _isPlayingSound = false;
    private const float _walkingSoundRestTime = 0.2f;

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

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
