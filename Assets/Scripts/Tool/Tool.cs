using UnityEngine;
using UnityEngine.InputSystem;

namespace Recounter
{
    public abstract class Tool : MonoBehaviour
    {
        protected InputAction UseAction { get; private set; }

        protected virtual void Awake()
        {
            UseAction = InputLayer.Interaction.Interact;
        }

        public bool IsEquipped { get; private set; }

        bool _justEquipped;

        public Employee Owner { get; private set; }

        public void Equip(Employee e)
        {
            _justEquipped = true;

            Owner = e;

            OnEquip();
        }

        public void Unequip()
        {
            IsEquipped = false;

            OnUnequip();
        }

        protected virtual void Update()
        {
            // Delay one frame before running EquippedUpdate.

            if (_justEquipped)
            {
                _justEquipped = false;
                IsEquipped = true;
                return;
            }

            if (!IsEquipped) return;

            EquippedUpdate();
        }

        protected virtual void OnEquip() { }
        protected virtual void OnUnequip() { }

        /// <summary>
        /// Runs every frame after this tool has been equipped.
        /// </summary>
        protected virtual void EquippedUpdate() { }
    }

    public abstract class Tool<T> : Tool, IHoverHandler<T> where T : class
    {
        [SerializeField] LayerMask _toolableMask;
        [SerializeField] LayerMask _allMask;
        [SerializeField] float _range;

        static Camera s_camera;

        HoverRaycaster<T> _hoverRaycaster;

        protected T CurrentHover => _hoverRaycaster.HoverTarget;

        void Start()
        {
            if (s_camera == null)
                s_camera = Camera.main;

            _hoverRaycaster = new(s_camera, _range, _allMask, _toolableMask, GetComponentType.InParent, this);
        }

        protected override void EquippedUpdate()
        {
            _hoverRaycaster.Raycast();
        }

        public virtual void HoverEnter(T obj) { }

        public void HoverStay(T obj)
        {
            if (UseAction.WasPressedThisFrame())
            {
                UseOn(obj);
            }
        }

        public virtual void OnRaycastHit(RaycastHit hit) { }

        public virtual void HoverExit(T obj) { }

        protected abstract void UseOn(T obj);
    }
}
