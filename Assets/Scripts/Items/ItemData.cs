using UnityEngine;

[CreateAssetMenu(menuName = "Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] public string Name { get; private set; }
    [SerializeField] public Texture Thumbnail { get; private set; }
}
