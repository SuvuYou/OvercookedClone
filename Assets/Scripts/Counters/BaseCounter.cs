using UnityEngine;

public class BaseCounter : KitchenItemParent
{
    [SerializeField] private SelectedObjectsInRangeSO _selectedObjects;
    [SerializeField] private GameObject _selectedVisualIndicator;

    private void Start()
    {
        _selectedObjects.OnSelectCounter += _checkIsCounterSelected;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    
        _selectedObjects.OnSelectCounter -= _checkIsCounterSelected;
    }

    public virtual void Interact(KitchenItemParent player) { }

    public virtual void InteractAlternative(KitchenItemParent player) { }

    private void _checkIsCounterSelected(BaseCounter newSelectedCounter)
    {
        _selectedVisualIndicator.SetActive(newSelectedCounter == this);
    }
}
