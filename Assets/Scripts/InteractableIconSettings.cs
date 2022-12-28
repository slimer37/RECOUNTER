using UnityEngine;

[CreateAssetMenu(menuName = "Settings/Interactable Icon Settings")]
public class InteractableIconSettings : ScriptableObject
{
    [SerializeField] Sprite[] icons;

    public Sprite GetSprite(Interactable.Icon icon) => icons[(int)icon];
}
