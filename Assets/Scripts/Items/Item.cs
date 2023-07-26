using Recounter.Thumbnails;
using UnityEngine;

namespace Recounter.Items
{
    public abstract class Item : Interactable
    {
        [field: Header("Item")]
        [field: SerializeField] public Hand.ViewmodelPose ViewmodelPose { get; private set; }

        public Texture2D Thumbnail =>
                _thumbnail ? _thumbnail : _thumbnail = ThumbnailCreator.CreateThumbnail(transform);

        Texture2D _thumbnail;
        Hotbar _containerHotbar;

        public bool IsHeld => _containerHotbar;

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

        public static implicit operator bool(Item item) => item != null;
    }
}
