using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Recounter.Art
{
    public class BrushUI : Brush
    {
        [Header("Settings")]
        [SerializeField] ColorPicker colorPicker;
        [SerializeField] Slider radiusSlider;
        [SerializeField] TextMeshProUGUI radiusDisplay;

        [Header("Shortcuts")]
        [SerializeField] RectTransform canvas;
        [SerializeField] InputAction changeBrushSize;
        [SerializeField] int radiusIncrement;

        protected override void Awake()
        {
            base.Awake();
            UpdateUIState();

            UpdateRadius(radiusSlider.value);
            radiusSlider.onValueChanged.AddListener(UpdateRadius);

            UpdateColor(colorPicker.Color);
            colorPicker.onColorChanged.AddListener(UpdateColor);

            changeBrushSize.performed += ChangeBrushSize;
            changeBrushSize.Enable();
        }

        public override void Activate(Texture texture)
        {
            base.Activate(texture);
            UpdateUIState();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            UpdateUIState();
        }

        void UpdateUIState()
        {
            colorPicker.Interactable = Active;
        }

        void UpdateRadius(float newRadius)
        {
            Radius = newRadius;

            radiusDisplay.text = Radius.ToString();
        }

        void UpdateColor(Color newColor)
        {
            Color = newColor;
        }

        void ChangeBrushSize(InputAction.CallbackContext ctx)
        {
            var scroll = ctx.ReadValue<float>() > 0 ? 1 : -1;
            radiusSlider.value += scroll * radiusIncrement;
        }
    }
}
