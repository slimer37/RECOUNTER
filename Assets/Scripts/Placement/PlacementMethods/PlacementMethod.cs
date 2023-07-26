using Recounter.Items;
using UnityEngine;

namespace Recounter
{
    public enum PlacementCursor { None, Placement, Rotation }

    public abstract class PlacementMethod : MonoBehaviour
    {
        public Vector3 PlacePosition { get; protected set; }
        public Vector3 PlaceEulerAngles { get; protected set; }
        public PlacementCursor Cursor { get; protected set; } = PlacementCursor.Placement;

        public Quaternion PlaceRotation => Quaternion.Euler(PlaceEulerAngles);

        protected Placer Placer { get; private set; }
        protected Transform Body { get; private set; }
        protected Transform Camera { get; private set; }

        protected Placeable ActiveItem => Placer.Active;

        protected virtual bool BlockedPlacement => true;

        public virtual bool ShouldForceGhost() => true;

        public virtual bool Accepts(Placeable item) => true;

        public void SetUp(Placer placer, Transform body, Transform camera)
        {
            Placer = placer;
            Body = body;
            Camera = camera;
        }

        public abstract void CalculateInitialPosition();

        public virtual bool IsPositionValid() => !ActiveItem.WouldIntersectAt(PlacePosition, PlaceRotation);

        protected abstract void Move(bool modifier, Vector2 mouseDelta, float rawScroll);

        public void HandlePlacement(bool modifier, Vector2 mouseDelta, float rawScroll)
        {
            var previousPos = PlacePosition;
            var previousRot = PlaceEulerAngles;

            Move(modifier, mouseDelta, rawScroll);

            if (!BlockedPlacement) return;

            if (!IsPositionValid())
            {
                PlacePosition = previousPos;
                PlaceEulerAngles = previousRot;
            }
        }

        public virtual bool AttemptRelease() => IsPositionValid();
    }
}
