using UnityEngine;

public class Employee : MonoBehaviour
{
    [field: SerializeField] public Hotbar ItemHotbar { get; private set; }
    [field: SerializeField] public PlayerController Controller { get; private set; }
    [field: SerializeField] public PlayerInteraction Interaction { get; private set; }
    [field: SerializeField] public Hand LeftHand { get; private set; }
    [field: SerializeField] public Hand RightHand { get; private set; }
}
