using UnityEngine;

public class ColorSelectorPanelUI : MonoBehaviour
{
    [SerializeField] private ColorPickersSO _colorPickers;

    private void Awake()
    {
        _colorPickers.InitPickers(transform);
    }
}
