using NaughtyAttributes;
using Recounter.Thumbnails;
using UnityEngine;

namespace Recounter.Items
{
    public abstract class Item : Interactable
    {
        [field: Header("Item")]
        [field: SerializeField] public Hand.ViewmodelPose ViewmodelPose { get; private set; }

        [Header("Holding Transform")]
        [SerializeField] Vector3 holdPosShift;
        [SerializeField] bool overridesHoldRot;
        [SerializeField, EnableIf(nameof(overridesHoldRot)), AllowNesting] Vector3 holdRot;

        public Vector3 HoldPosShift => holdPosShift;
        public Quaternion? OverrideHoldRotation => overridesHoldRot ? Quaternion.Euler(holdRot) : null;

        public Texture2D Thumbnail =>
                _thumbnail ? _thumbnail : _thumbnail = ThumbnailCreator.CreateThumbnail(transform);

        public bool IsHeld => _containerHotbar;

        Texture2D _thumbnail;

        Hotbar _containerHotbar;

        protected override void OnInteract(Employee e)
        {
            e.ItemHotbar.TryAddItem(this);
        }

        public void PostPickUp(Hotbar hotbar)
        {
            _containerHotbar = hotbar;
            OnPickUp();
        }

        protected virtual void OnPickUp() { }

        public virtual void Release()
        {
            _containerHotbar.RemoveItem(this);
            _containerHotbar = null;
            OnRelease();
        }

        protected virtual void OnRelease() { }
    }
}
