using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SelectedObjectsInRangeSO", menuName = "ScriptableObjects/SelectedObjectsInRangeSO")]
public class SelectedObjectsInRangeSO : ScriptableObject
{
    public event Action<EditableItem> OnSelectSubject;
    public EditableItem SelectedEditingSubject { get; private set; }

    public event Action<GridTile> OnSelectTile;
    public GridTile SelectedGridTile { get; private set; }

    public event Action<Shop> OnSelectShop;
    public Shop SelectedShop { get; private set; }

    public event Action<BaseCounter> OnSelectCounter;
    public BaseCounter SelectedCounter { get; private set; }

    public event Action OnStartEditing;
    public event Action OnEndEditing;

    public bool IsCurrentlyEditing { get; private set; } = false;

    private void OnEnable()
    {
        OnSelectSubject += (EditableItem subject) => SelectedEditingSubject = subject;
        OnSelectTile += (GridTile selectedGridTile) => SelectedGridTile = selectedGridTile;
        OnSelectShop += (Shop selectedShop) => SelectedShop = selectedShop;
        OnSelectCounter += (BaseCounter selectedCounter) => SelectedCounter = selectedCounter;

        OnStartEditing += () => IsCurrentlyEditing = true;
        OnEndEditing += () => IsCurrentlyEditing = false;
    }

    public void TriggerOnStartEditing() => OnStartEditing?.Invoke();
    public void TriggerOnEndEditing() => OnEndEditing?.Invoke();

    public void TriggerSelectEditingSubject(EditableItem subject)
    {
        if (subject != SelectedEditingSubject && !IsCurrentlyEditing) OnSelectSubject?.Invoke(subject);
    }

    public void TriggerSelectGridTile(GridTile tile)
    {
        if (tile != SelectedGridTile && IsCurrentlyEditing) OnSelectTile?.Invoke(tile);
    }

    public void TriggerSelectShop(Shop shop)
    {
        if (shop != SelectedShop) OnSelectShop?.Invoke(shop);
    }

    public void TriggerSelectCounter(BaseCounter counter)
    {
        if (counter != SelectedCounter) OnSelectCounter?.Invoke(counter);
    }
}
