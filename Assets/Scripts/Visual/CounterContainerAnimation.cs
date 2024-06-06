using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CounterContainerAnimation : MonoBehaviour
{
    private const string OPEN_CLOSE = "OpenClose";

    [SerializeField] private CounterContainer _counter;
    private Animator _animator;
   
    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        _counter.OnContainerOpen += () => _animator.SetTrigger(OPEN_CLOSE);
    }
}
