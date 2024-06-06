using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerStateSO _playerState;

    private float _playerHeight = 2f;
    private float _playerRadius = 0.7f;
    private float _movementSpeed = 10f;
    private float _rotationSpeed = 17f;

    private float _movementInputThreshold = 0.15f;

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Active)
        {
            return;
        }

        Vector3 movementDirection = PlayerInput.Instance.GetMovementDirectionVector();
        _playerState.SetMovementState(movementDirection);
        
        if (_playerState.IsWalking) 
        {
            _moveByDirection(movementDirection);
        }
        
        if (_playerState.CurrentFacingDirection.magnitude > 0) _rotateByDirection(_playerState.CurrentFacingDirection);
    }

    private void _moveByDirection(Vector3 movementDirection)
    {
        float distance = _movementSpeed * Time.deltaTime;
        Vector3 xVec = new (_registerMovementWithinThreshold(movementDirection.x), 0f, 0f);
        Vector3 zVec = new (0f, 0f, _registerMovementWithinThreshold(movementDirection.z));

        bool canWalkOnXAxis = !Physics.CapsuleCast(transform.position, transform.position + (Vector3.up * _playerHeight), _playerRadius, xVec, distance);
        bool canWalkOnZAxis = !Physics.CapsuleCast(transform.position, transform.position + (Vector3.up * _playerHeight), _playerRadius, zVec, distance);

        Vector3 allowedMovementDirection = Vector3.zero;

        if (canWalkOnXAxis)
        {
            allowedMovementDirection += xVec;
        }

        if (canWalkOnZAxis)
        {
            allowedMovementDirection += zVec;
        }

        transform.Translate(allowedMovementDirection.normalized * (_movementSpeed * Time.deltaTime), Space.World);
    }

    private float _registerMovementWithinThreshold(float input)
    {
        if (input < _movementInputThreshold && input > -_movementInputThreshold)
        {
            return 0f;
        }

        return input;
    }

    private void _rotateByDirection(Vector3 movementDirection)
    {
        transform.forward = Vector3.Slerp(transform.forward, movementDirection, Time.deltaTime * _rotationSpeed);
    }
}
