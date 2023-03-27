using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Recounter
{
    public class VirtualButton : MonoBehaviour
    {
        [SerializeField] RectTransform cursor;
        [SerializeField] Image image;
        [SerializeField] Button button;
        [SerializeField] InputAction down;

        RectTransform rect;

        bool isOver;

        void Awake()
        {
            rect = transform as RectTransform;

            down.started += Click;
            down.canceled += Click;

            down.Enable();

            button.onClick.AddListener(() => print("Hello"));
        }

        void Click(InputAction.CallbackContext obj)
        {
            var press = obj.ReadValueAsButton();

            var pointer = new PointerEventData(EventSystem.current);

            if (press)
            {
                if (!isOver) return;
                ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            }
            else
            {
                ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
                if (!isOver) return;
                ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
            }
        }

        void Update()
        {
            var pointer = new PointerEventData(EventSystem.current);

            if (RectTransformUtility.RectangleContainsScreenPoint(rect, cursor.position))
            {
                if (isOver) return;

                isOver = true;
                ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
            }
            else
            {
                if (!isOver) return;

                isOver = false;
                ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerExitHandler);
            }
        }
    }
}
