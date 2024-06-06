using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStateSO", menuName = "ScriptableObjects/PlayerStateSO")]
public class PlayerStateSO : ScriptableObject
{
    public bool IsWalking {get; private set;}
    public Vector3 CurrentFacingDirection {get; private set;}

    public void SetMovementState (Vector3 movementDirection)
    {
        IsWalking = movementDirection.magnitude > 0;

        if (movementDirection.magnitude > 0)
        {
            CurrentFacingDirection = movementDirection;
        }
        
    }
}
