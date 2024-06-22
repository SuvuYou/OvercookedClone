using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorPickersSO", menuName = "ScriptableObjects/ColorPickersSO")]
public class ColorPickersSO : ScriptableObject
{
    [SerializeField] private List<Color> _colors;
    [SerializeField] private SingleColorPickerUI _colorPickerPrefab;

    public List<Color> Colors { get; private set; }
    public List<SingleColorPickerUI> ColorPickersUI { get; private set; }
    
    public void InitPickers(Transform location)
    {
        Colors = _colors;

        foreach(Color color in _colors)
        {
            SingleColorPickerUI picker = Instantiate(_colorPickerPrefab, location);
            picker.Init(color);
            ColorPickersUI.Add(picker);
        }
    }

    public void ResetAllColorPickers()
    {
        foreach(SingleColorPickerUI picker in ColorPickersUI)
        {
            picker.ResetIndicator();
        }
    }

    public void SelectPickerByColor(Color color)
    {
        ResetAllColorPickers();

        foreach(SingleColorPickerUI picker in ColorPickersUI)
        {
            if (color == picker.ColorRef)
            {
                picker.IndicateAsSelected();
            }
        }
    }
}
