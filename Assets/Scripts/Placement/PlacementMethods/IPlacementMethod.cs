using UnityEngine;

namespace Recounter
{
    public enum PlacementCursor { None, Placement, Rotation }

    public interface IPlacementMethod
    {
        public void Initialize(Placer placer, Transform body, Transform camera);

        public void GetInitialPositionAndRotation(out Vector3 position, out Vector3 eulerAngles);

        public bool IsItemPositionValid(Vector3 position, Quaternion rotation);

        public void HandlePlacement(
            ref Vector3 placePosition, ref Vector3 placeRotation, bool modifier, Vector2 mouseDelta, float rawScroll, out PlacementCursor cursor);
    }
}
