using UnityEngine;

namespace Recounter
{
    public enum PlacementCursor { None, Placement, Rotation }

    public abstract class PlacementMethod : MonoBehaviour
    {
        protected Placer Placer { get; private set; }
        protected Transform Body { get; private set; }
        protected Transform Camera { get; private set; }

        protected Item ActiveItem => Placer.Active;

        public virtual bool ShouldForceGhost() => true;

        public virtual bool Accepts(Item item) => true;

        public void SetUp(Placer placer, Transform body, Transform camera)
        {
            Placer = placer;
            Body = body;
            Camera = camera;
        }

        public abstract void GetInitialPositionAndRotation(out Vector3 position, out Vector3 eulerAngles);

        public virtual bool IsItemPositionValid(Item item, Vector3 position, Quaternion rotation) =>
            !item.WouldIntersectAt(position, rotation);

        public abstract void HandlePlacement(ref Vector3 placePosition, ref Vector3 placeRotation, bool modifier,
            Vector2 mouseDelta, float rawScroll, out PlacementCursor cursor);

        public virtual bool AttemptRelease(Item item, Vector3 position, Quaternion rotation) =>
            IsItemPositionValid(item, position, rotation);
    }
}
