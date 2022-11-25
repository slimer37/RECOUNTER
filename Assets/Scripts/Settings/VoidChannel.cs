using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Channels/VoidChannel")]
public class VoidChannel : ScriptableObject
{
    public UnityAction OnEventRaised;

    public void RaiseEvent() => OnEventRaised?.Invoke();
}