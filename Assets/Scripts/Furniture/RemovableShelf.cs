using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recounter
{
    public class RemovableShelf : Interactable
    {
        [SerializeField] Vector3 _holdPos;
        [SerializeField] Vector3 _holdRot;

        protected override HudInfo FormHud(Employee e) => new()
        {
            text = "Remove shelf",
            icon = Icon.Pickup
        };

        protected override void OnInteract(Employee e)
        {
            LastInteractor.LeftHand.Hold(this, _holdPos, Quaternion.Euler(_holdRot));
        }

        protected override bool CanInteract(Employee e) =>
            e.RightHand.IsFull && e.RightHand.HeldObject.CompareTag("Shelf Tool");
    }
}
