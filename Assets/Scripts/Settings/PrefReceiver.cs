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
    [SerializeField, ShowIf("type", PrefType.String)] UnityEvent<string> StringChange;
    [SerializeField, ShowIf("type", PrefType.Int)] UnityEvent<int> IntChange;
    [SerializeField, ShowIf("type", PrefType.Float)] UnityEvent<float> FloatChange;

    void OnEnable() => PrefManager.SetCallbacks(this);
    void OnDisable() => PrefManager.RemoveCallbacks(this);

    public void OnStringPrefChanged(string key, string value)
    {
        if (this.key != key) return;
        StringChange?.Invoke(value);
    }

    public void OnIntPrefChanged(string key, int value)
    {
        if (this.key != key) return;
        IntChange?.Invoke(value);
    }

    public void OnFloatPrefChanged(string key, float value)
    {
        if (this.key != key) return;
        FloatChange?.Invoke(value);
    }
}
