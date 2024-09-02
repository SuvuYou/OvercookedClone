using UnityEngine;

public class EditableSubjectVisual : MonoBehaviour
{
    [SerializeField] private SelectedObjectsInRangeSO _selectedObjects;

    private const float TIME_FOR_TRANSITION = 0.25f;

    private bool _isTransitioning = false;
    private TimingTimer _transitionTimer = new(defaultTimerValue: TIME_FOR_TRANSITION);

    private const float DEFAULT_SCALE_MAGNITUTE = 1;
    private const float TARGET_SCALE_MAGNITUTE = 0.4f;

    private float _currentScaleMagnitute;
    private float _targetScaleMagnitute;

    private const float DEFAULT_POSITION_Y = 0f;
    private const float TARGET_POSITION_Y = 1.6f;

    private float _currentPositionY;
    private float _targetPositionY;

    private void Start()
    {
        _selectedObjects.OnSelectShop += _setTransformTarget;
    }

    private void OnDestroy()
    {
        _selectedObjects.OnSelectShop -= _setTransformTarget;
    }

    private void Update()
    {
        if (_isTransitioning)
        {
            _transitionTimer.SubtractTime(Time.deltaTime);

            float currentPercentage = (TIME_FOR_TRANSITION - _transitionTimer.Time) * (1 / TIME_FOR_TRANSITION);
            transform.localScale = Vector3.one * Mathf.Lerp(_currentScaleMagnitute, _targetScaleMagnitute, currentPercentage);
            transform.position = new (transform.position.x, Mathf.Lerp(_currentPositionY, _targetPositionY, currentPercentage), transform.position.z);

            if (_transitionTimer.IsTimerUp()) 
            {
                _transitionTimer.ResetTimer();
                _isTransitioning = false;
            }
        }
    }

    private void _setTransformTarget(Shop newShop)
    {
        if (_selectedObjects.SelectedEditingSubject == null || !_selectedObjects.IsCurrentlyEditing) return;
        if (_selectedObjects.SelectedEditingSubject.gameObject != this.gameObject) return;

        _isTransitioning = true;
        _transitionTimer.ResetTimer();

        if (newShop == null)
        {
            _currentPositionY = transform.position.y;
            _currentScaleMagnitute = transform.localScale.magnitude;

            _targetPositionY = DEFAULT_POSITION_Y;
            _targetScaleMagnitute = DEFAULT_SCALE_MAGNITUTE;

            return;
        }

        _currentPositionY = transform.position.y;
        _currentScaleMagnitute = transform.localScale.x;

        _targetPositionY = TARGET_POSITION_Y;
        _targetScaleMagnitute = TARGET_SCALE_MAGNITUTE;
    }
}
