using UnityEngine;

namespace Recounter
{
    public static class InputLayer
    {
        public static Controls.MenuActions Menu;
        public static Controls.PlacementActions Placement;
        public static Controls.MovementActions Movement;
        public static Controls.InteractionActions Interaction;
        public static Controls.TabletActions Tablet;

        static Controls _controls;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            _controls = new Controls();

            Menu = _controls.Menu;
            Placement = _controls.Placement;
            Movement = _controls.Movement;
            Interaction = _controls.Interaction;
            Tablet = _controls.Tablet;

            _controls.Enable();
        }

        public static void SetCursor(bool show)
        {
            Cursor.visible = show;
            Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        }

        public static void SuspendMovement(bool suspend, bool affectCursor = false)
        {
            if (suspend)
            {
                Movement.Disable();
            }
            else
            {
                Movement.Enable();
            }

            // Always hide cursor when re-enabling movement. Optional when disabling.
            if (suspend && !affectCursor) return;

            SetCursor(suspend);
        }
    }
}
