using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace Recounter
{
    public class BankNote : Interactable
    {
        [field: SerializeField] public float Value { get; private set; }

        public const float Spacing = 0.005f;

        readonly Stack<BankNote> _stackedNotes = new();

        Collider _collider;

        Hand _hand;

        void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        protected override bool CanInteract(Employee e) => !e.LeftHand.IsFull || e.LeftHand.Contains<BankNote>(out _);

        protected override void OnInteract(Employee e)
        {
            _collider.enabled = false;

            if (e.LeftHand.Contains<BankNote>(out var note))
            {
                note.Stack(this);
            }
            else
            {
                _hand = e.LeftHand;
                _hand.Hold(this);
            }
        }

        void Stack(BankNote note)
        {
            _hand.Clear();

            note.transform.parent = transform;

            _hand.Hold(this);

            _stackedNotes.Push(note);

            note.transform.DOLocalMove(Vector3.up * (_stackedNotes.Count * Spacing), 0.1f);
            note.transform.DOLocalRotate(Vector3.up * Random.Range(-5f, 5f), 0.1f);
        }

        public BankNote RetrieveFromStack()
        {
            if (_stackedNotes.Count == 0)
            {
                _hand.Clear();
                _hand = null;
                return this;
            }

            var note = _stackedNotes.Pop();

            _hand.Clear();

            note.transform.parent = null;

            _hand.Hold(this);

            return note;
        }
    }
}
