using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Slider))]
public class SliderSetting : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] string key;
    [SerializeField] bool isInt;
    [SerializeField] VoidChannel channel;

    Slider slider;

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isInt)
            PlayerPrefs.SetInt(key, Mathf.FloorToInt(slider.value));
        else
            PlayerPrefs.SetFloat(key, slider.value);

        channel.RaiseEvent();
    }

    void Awake()
    {
        slider = GetComponent<Slider>();
    }
}
