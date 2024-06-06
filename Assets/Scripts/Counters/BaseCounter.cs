using UnityEngine;

public class BaseCounter : KitchenItemParent
{
    [SerializeField] private SelectedCounterSO _selectedCounter;
    [SerializeField] private GameObject _selectedVisualIndicator;

    private void Start()
    {
        _selectedCounter.OnSelectCounter += _checkIsCounterSelected;
    }

    private void OnDestroy()
    {
        _selectedCounter.OnSelectCounter -= _checkIsCounterSelected;
    }

    public virtual void Interact(KitchenItemParent player) { }

    public virtual void InteractAlternative(KitchenItemParent player) { }

    private void _checkIsCounterSelected(BaseCounter newSelectedCounter)
    {
        _selectedVisualIndicator.SetActive(newSelectedCounter == this);
    }
}
