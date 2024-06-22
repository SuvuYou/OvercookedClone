
using System;
using UnityEngine;
using UnityEngine.UI;

public class SingleColorPickerUI : MonoBehaviour
{
    public event Action<Color> OnColorSelect;
    public Color ColorRef { get; private set; }

    [SerializeField] private Button _pickerActionButton;
    [SerializeField] private GameObject _selectedIndicator;

    public void Init(Color color)
    {
        ResetIndicator();
        ColorRef = color;
        _pickerActionButton.image.color = color;
        _pickerActionButton.onClick.AddListener(() => OnColorSelect?.Invoke(color));
    }

    public void IndicateAsSelected()
    {
        _selectedIndicator.SetActive(true);
    }

    public void ResetIndicator()
    {
        if (_selectedIndicator == null)
        {
            return;
        }
        
        _selectedIndicator.SetActive(false);
    }
}
