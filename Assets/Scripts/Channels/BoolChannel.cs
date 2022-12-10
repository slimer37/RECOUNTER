using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Channels/BoolChannel")]
public class BoolChannel : ScriptableObject
{
    public UnityAction<bool> OnEventRaised;

    public void RaiseEvent(bool value) => OnEventRaised?.Invoke(value);
}