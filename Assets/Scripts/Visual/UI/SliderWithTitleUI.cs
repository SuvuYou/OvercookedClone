using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderWithTitleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _valueText;
    [SerializeField] private Slider _slider;

    public event Action<float> OnValueChange;

    private void Start()
    {
        _slider.onValueChanged.AddListener(_onChangeValue);
    }

    private void OnDestroy()
    {
        _slider.onValueChanged.RemoveAllListeners();
    }

    private void _onChangeValue(float value)
    {
        _valueText.text = (value * 10).ToString(format: "0.0");

        OnValueChange?.Invoke(value);
    }

    public void SyncValue(float value)
    {
        _valueText.text = (value * 10).ToString(format: "0.0");
        _slider.value = value;
    } 

    public void SelectSlider()
    {
        _slider.Select();
    }
}
