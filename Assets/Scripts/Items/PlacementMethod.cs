using UnityEngine;

namespace Recounter
{
    public enum PlacementCursor { None, Placement, Rotation }

    public abstract class PlacementMethod : MonoBehaviour
    {
        protected Placer Placer { get; private set; }
        protected Transform Body { get; private set; }
        protected Camera Camera { get; private set; }

        public abstract Vector3 GetInitialPlacementPosition();

        public abstract bool IsItemPositionValid(Vector3 position, Quaternion rotation);

        public virtual void Initialize(Placer placer, Transform body, Camera camera)
        {
            Placer = placer;
            Body = body;
            Camera = camera;
        }

        public abstract void HandlePlacement(
            ref Vector3 placePosition, ref Vector3 placeRotation, bool modifier, Vector2 mouseDelta, float rawScroll, out PlacementCursor cursor);
    }
}
