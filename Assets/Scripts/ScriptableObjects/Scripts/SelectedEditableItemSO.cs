using System;
using UnityEngine;
using UnityEngine.iOS;

[CreateAssetMenu(fileName = "SelectedEditableItemSO", menuName = "ScriptableObjects/SelectedEditableItemSO")]
public class SelectedEditableItemSO : ScriptableObject
{
    public event Action<EditableItem> OnSelectSubject;
    public EditableItem SelectedEditingSubject { get; private set; }

    public event Action<GridTile> OnSelectTile;
    public GridTile SelectedGridTile { get; private set; }

    public event Action OnStartEditing;
    public event Action OnEndEditing;

    private bool _isCurrentlyEditing = false;

    private void OnEnable()
    {
        OnSelectSubject += (EditableItem subject) => SelectedEditingSubject = subject;
        OnSelectTile += (GridTile selectedGridTile) => SelectedGridTile = selectedGridTile;
        OnStartEditing += () => _isCurrentlyEditing = true;
        OnEndEditing += () => _isCurrentlyEditing = false;
    }

    public void TriggerOnStartEditing() => OnStartEditing?.Invoke();
    public void TriggerOnEndEditing() => OnEndEditing?.Invoke();

    public void TriggerSelectEditingSubject(EditableItem subject)
    {
        if (subject != SelectedEditingSubject && !_isCurrentlyEditing) OnSelectSubject?.Invoke(subject);
    }

    public void TriggerSelectGridTile(GridTile tile)
    {
        if (tile != SelectedGridTile && _isCurrentlyEditing) OnSelectTile?.Invoke(tile);
    }
}
