using UnityEngine;

namespace Recounter.Thumbnails
{
    [CreateAssetMenu(menuName = "Settings/Thumbnail Creator Settings")]
    public class ThumbnailCreatorSettings : ScriptableObject
    {
        [field: SerializeField] public Vector3 ViewDirection { get; private set; } = -Vector3.one;
        [field: SerializeField, Range(-0.25f, 0.25f)] public float Padding { get; private set; }
        [field: SerializeField, Min(1)] public Vector2Int Resolution { get; private set; } = Vector2Int.one * 64;
        [field: SerializeField] public bool Orthographic { get; private set; }
        [field: SerializeField] public bool ShouldCloneModel { get; private set; }
        [field: SerializeField] public Color BackgroundColor { get; private set; }
        [field: SerializeField, Range(1, 2)] public float RenderSupersampling { get; private set; } = 1;

        [field: Header("Texture Settings")]
        [field: SerializeField] public FilterMode FilterMode { get; private set; }
    }
}
