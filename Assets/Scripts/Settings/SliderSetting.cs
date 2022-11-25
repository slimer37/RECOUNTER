using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Slider))]
public class SliderSetting : MonoBehaviour
{
    [SerializeField] string key;
    [SerializeField] float defaultValue;
    [SerializeField] bool isInt;

    [Header("Optional")]
    [SerializeField] TMP_InputField inputField;
    [SerializeField] VoidChannel channel;

    Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();

        slider.value = isInt
            ? PlayerPrefs.GetInt(key, (int)defaultValue)
            : PlayerPrefs.GetFloat(key, defaultValue);

        slider.onValueChanged.AddListener(SetSliderValue);

        if (inputField)
        {
            inputField.contentType = isInt
                ? TMP_InputField.ContentType.IntegerNumber
                : TMP_InputField.ContentType.DecimalNumber;

            inputField.onEndEdit.AddListener(SetFieldValue);
            inputField.onValueChanged.AddListener(SetFieldValue);

            inputField.text = slider.value.ToString();
        }
    }

    void SetSliderValue(float v)
    {
        SetValue(v);

        if (inputField)
            inputField.text = v.ToString();
    }

    void SetFieldValue(string s)
    {
        if (!float.TryParse(s, out var v))
        {
            v = 0;
            inputField.text = "0";
        }

        SetValue(v);
        slider.value = v;
    }

    void SetValue(float v)
    {
        if (isInt)
            PlayerPrefs.SetInt(key, (int)v);
        else
            PlayerPrefs.SetFloat(key, v);

        if (channel)
            channel.RaiseEvent();
    }
}
