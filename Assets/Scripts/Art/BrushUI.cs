using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BrushUI : Brush
{
    [Header("Settings")]
    [SerializeField] ColorPicker colorPicker;
    [SerializeField] Slider radiusSlider;
    [SerializeField] TextMeshProUGUI radiusDisplay;

    [Header("Preview")]
    [SerializeField] Image preview;
    [SerializeField] RectTransform previewTransform;
    [SerializeField] Outline outline;

    [Header("Shortcuts")]
    [SerializeField] RectTransform canvas;
    [SerializeField] InputAction changeBrushSize;
    [SerializeField] int radiusIncrement;

    public event Action<Color> ColorChanged;
    public event Action<float> RadiusChanged;

    protected override void Awake()
    {
        base.Awake();

        UpdateRadius(radiusSlider.value);
        radiusSlider.onValueChanged.AddListener(UpdateRadius);

        UpdateColor(colorPicker.Color);
        colorPicker.onColorChanged.AddListener(UpdateColor);

        changeBrushSize.performed += ChangeBrushSize;
        changeBrushSize.Enable();
    }

    void UpdateRadius(float newRadius)
    {
        Radius = newRadius;

        radiusDisplay.text = Radius.ToString();

        previewTransform.sizeDelta = Radius * 2 * Vector2.one;

        RadiusChanged?.Invoke(newRadius);
    }

    void UpdateColor(Color newColor)
    {
        Color = newColor;

        // Invert outline value based on current color
        preview.color = Color;
        Color.RGBToHSV(Color, out _, out _, out float v);
        outline.effectColor = Color.HSVToRGB(0, 0, 1 - v);

        ColorChanged?.Invoke(newColor);
    }

    void ChangeBrushSize(InputAction.CallbackContext ctx)
    {
        var scroll = ctx.ReadValue<float>() > 0 ? 1 : -1;
        radiusSlider.value += scroll * radiusIncrement;
    }

    void Update()
    {
        previewTransform.position = Mouse.current.position.ReadValue();
    }
}
