using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CuttingCounterAnimator : MonoBehaviour
{
    private const string CUT = "Cut";

    [SerializeField] private CuttingCounter _counter;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _counter.OnCut += _triggerCuttingAnimation;
    }   

    private void OnDestroy()
    {
        _counter.OnCut -= _triggerCuttingAnimation;
    }   

    private void _triggerCuttingAnimation() => _animator.SetTrigger(CUT);
}
