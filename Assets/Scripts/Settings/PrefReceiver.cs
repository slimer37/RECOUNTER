using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class PrefReceiver : MonoBehaviour, IPrefListener
{
    enum PrefType
    {
        String,
        Int,
        Float
    }

    [SerializeField] PrefType type;
    [SerializeField] string key;

    [SerializeField, ShowIf("type", PrefType.String)] string defaultString;
    [SerializeField, ShowIf("type", PrefType.String)] UnityEvent<string> StringChange;

    [SerializeField, ShowIf("type", PrefType.Int)] int defaultInt;
    [SerializeField, ShowIf("type", PrefType.Int)] UnityEvent<int> IntChange;

    [SerializeField, ShowIf("type", PrefType.Float)] float defaultFloat;
    [SerializeField, ShowIf("type", PrefType.Float)] UnityEvent<float> FloatChange;

    void OnEnable() => PrefManager.SetCallbacks(this);
    void OnDisable() => PrefManager.RemoveCallbacks(this);

    void Awake()
    {
        switch (type)
        {
            case PrefType.String:
                StringChange.Invoke(PlayerPrefs.GetString(key, defaultString));
                break;
            case PrefType.Int:
                IntChange.Invoke(PlayerPrefs.GetInt(key, defaultInt));
                break;
            case PrefType.Float:
                FloatChange.Invoke(PlayerPrefs.GetFloat(key, defaultFloat));
                break;
        }
    }

    public void OnStringPrefChanged(string key, string value)
    {
        if (type != PrefType.String || this.key != key) return;
        StringChange?.Invoke(value);
    }

    public void OnIntPrefChanged(string key, int value)
    {
        if (type != PrefType.Int || this.key != key) return;
        IntChange?.Invoke(value);
    }

    public void OnFloatPrefChanged(string key, float value)
    {
        if (type != PrefType.Float || this.key != key) return;
        FloatChange?.Invoke(value);
    }
}
