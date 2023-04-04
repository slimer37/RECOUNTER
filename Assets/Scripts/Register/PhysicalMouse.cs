using System;
using UnityEngine;

namespace Recounter
{
    public class PhysicalMouse : MonoBehaviour
    {
        [SerializeField] Transform _mouse;
        [SerializeField] float _mouseMovementScale;
        [SerializeField] float _mouseRotationScale;
        [SerializeField] ViewmodelPose _handPose;

        Vector3 _mouseBasePosition;
        float _mouseBaseRotation;

        Hand _hand;

        void Awake()
        {
            _mouseBasePosition = _mouse.position;
            _mouseBaseRotation = _mouse.eulerAngles.y;
        }

        public void StartUsing(Hand hand)
        {
            if (hand.IsFull)
                throw new ArgumentException("Cannot use mouse if hand is full.");

            hand.SetHandViewmodel(_handPose);

            _hand = hand;
        }

        public void Move(Vector2 cursorPosition)
        {
            var mouseMove = -cursorPosition * _mouseMovementScale;
            var pos = _mouseBasePosition + transform.right * mouseMove.x + transform.forward * mouseMove.y;
            var rot = _mouseBaseRotation + -mouseMove.x * _mouseRotationScale;

            _mouse.SetPositionAndRotation(pos, Quaternion.Euler(Vector3.up * rot));
        }

        public void StopUsing()
        {
            if (!_hand)
                throw new InvalidOperationException("Mouse not in use.");

            _hand.ResetHandViewmodel();
            _hand = null;
        }
    }
}
