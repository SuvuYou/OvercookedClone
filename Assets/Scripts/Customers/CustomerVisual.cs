using UnityEngine;

public class CustomerVisual : MonoBehaviour
{
    [SerializeField] MeshRenderer _headMesh;
    [SerializeField] MeshRenderer _bodyMesh;
    [SerializeField] private Customer _customer;
    [SerializeField] private Material _material;
    private Material _customMaterial;

    [SerializeField] private Color DEFAULT_COLOR;
    [SerializeField] private Color WAITING_FOR_ORDER_COLOR;
    [SerializeField] private Color EATING_COLOR;
    [SerializeField] private Color WAITING_TO_LEAVE_COLOR;

    private const float TIME_FOR_TRANSITION = 10f;
    private const string EMISSION_COLOR_STRING = "_EmissionColor";

    private bool _isTransitioning = false;
    private TimingTimer _transitionTimer = new(defaultTimerValue: TIME_FOR_TRANSITION);

    private Color _currentColor;
    private Color _targetColor;

    private void Awake()
    {
        _customMaterial = new(_material);
        _headMesh.material = _customMaterial;
        _bodyMesh.material = _customMaterial;
    }

    private void Start()
    {
        _setColor(DEFAULT_COLOR);

        _customer.OnSitDown += () => _startTransition(targetColor: WAITING_FOR_ORDER_COLOR);
        _customer.OnRecieveOrder += () => _startTransition(targetColor: EATING_COLOR);
        _customer.OnFinishEating += () => _startTransition(targetColor: WAITING_TO_LEAVE_COLOR);
        _customer.OnCustomerLeaving += () => _startTransition(targetColor: DEFAULT_COLOR);
    }

    private void Update()
    {
        if (_isTransitioning)
        {
            _transitionTimer.SubtractTime(Time.deltaTime);

            float currentPercentage = (TIME_FOR_TRANSITION - _transitionTimer.Time) * (1 / TIME_FOR_TRANSITION);
            _setColor(Color.Lerp(_currentColor, _targetColor, currentPercentage));

            if (_transitionTimer.IsTimerUp()) 
            {
                _transitionTimer.ResetTimer();
                _isTransitioning = false;
            }
        }
    }

    private void _startTransition(Color targetColor)
    {
        _isTransitioning = true;
        _currentColor = _getColor();
        _targetColor = targetColor;
    }

    private void _setColor(Color newColor) => _customMaterial.SetColor(EMISSION_COLOR_STRING, newColor);
    private Color _getColor() => _customMaterial.GetColor(EMISSION_COLOR_STRING);
}
