using UnityEngine;

namespace Recounter
{
    public class RemovedShelfTool : Tool<AdjustableShelfBase>
    {
        [SerializeField] RemovableShelf _shelf;

        Vector3 _position;

        public override void OnRaycastHit(RaycastHit hit)
        {
            // For some reason IsEquipped is false yet this still fires the frame after it shouldn't.
            // Why???
            if (!IsEquipped) return;

            if (hit.normal != CurrentHover.transform.forward || !CurrentHover.CheckAlignment(hit.point, out var position))
            {
                Finish();
                return;
            }

            Begin();

            _position = position;

            _shelf.transform.position = position + hit.normal * 0.2f;
            _shelf.transform.rotation = Quaternion.LookRotation(CurrentHover.transform.forward, Vector3.up);
        }

        protected override void OnEquip() =>
            Owner.LeftHand.SetCarryStates(Hand.CarryStates.InWorld | Hand.CarryStates.FreePositionAndRotation);

        protected override void OnUnequip() => Owner.LeftHand.Clear();

        public override void HoverExit(AdjustableShelfBase obj) => Finish();

        protected override void UseOn(AdjustableShelfBase obj)
        {
            if (_shelf.Intersects()) return;

            _shelf.transform.position = _position;
            _shelf.transform.SetParent(CurrentHover.transform);
            _shelf.AttachToShelf();
        }

        void Begin()
        {
            Owner.LeftHand.SetCarryStates(Hand.CarryStates.InWorld | Hand.CarryStates.FreePositionAndRotation);
        }

        void Finish()
        {
            Owner.LeftHand.SetCarryStates(Hand.CarryStates.None);
        }
    }
}
