using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CustomerAnimator : MonoBehaviour
{
    private const string IS_WALKING = "IsWalking";

    [SerializeField] private Customer _customer;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        _animator.SetBool(IS_WALKING, _customer.IsWalking());
    }
}
