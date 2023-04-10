using DG.Tweening;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

namespace Recounter
{
    public class CurrencySlot : Interactable
    {
        [SerializeField] float _moveTime;
        [SerializeField] Vector3 _rotation;
        [SerializeField] float _holdThreshold = 0.15f;

        [Header("SFX")]
        [SerializeField] EventReference _handleSfx;

        [Header("Randomness")]
        [SerializeField] Vector3 _randomizeRotation;
        [SerializeField] Vector3 _randomizePosition;

        [Header("Lever Angle Calculation")]
        [SerializeField] Transform _lever;
        [SerializeField] float _leverLength = 0.203f;
        [SerializeField] float _leverRestOffset;
        [SerializeField] float _leverAngleOffset;
        [SerializeField] Vector3 _leverAxis;
        [SerializeField] float _leverOpenAngle;

        [Header("Lever Return Animation")]
        [SerializeField] float _leverReturnTime;
        [SerializeField] Ease _leverReturnEase;

        readonly Stack<BankNote> _notes = new();

        float _holdTime;

        bool _holding;

        float _leverClosedAngle;

        protected override HudInfo FormHud(Employee e) => new()
        {
            icon = Icon.Extract,
            text = "Tap to take\nHold to insert"
        };

        float GetClosedAngle()
        {
            var baseHeight = transform.position.y + _notes.Count * BankNote.Spacing;
            var h = _lever.position.y - baseHeight + _leverRestOffset;
            return Mathf.Acos(h / _leverLength) * Mathf.Rad2Deg + _leverAngleOffset;
        }

        void Awake()
        {
            foreach (var note in GetComponentsInChildren<BankNote>())
            {
                StoreNote(note, true);
            }

            _lever.localEulerAngles = _leverAxis * GetClosedAngle();
        }

        protected override void OnInteract(Employee e)
        {
            _lever.DOKill();

            _holdTime = 0;
            _holding = true;

            _leverClosedAngle = GetClosedAngle();
        }

        void StoreNote(BankNote note, bool direct = false)
        {
            _notes.Push(note);

            var noteTransform = note.transform;

            noteTransform.parent = transform;

            var localPos = Vector3.up * (BankNote.Spacing * _notes.Count) + Randomize(_randomizePosition);
            var localRot = _rotation + Randomize(_randomizeRotation);

            if (direct)
            {
                noteTransform.localPosition = localPos;
                noteTransform.localEulerAngles = _rotation;
                return;
            }

            noteTransform.DOLocalMove(localPos, _moveTime);
            noteTransform.DOLocalRotate(localRot, _moveTime);

            PlaySfx();
        }

        void GrabNote(Employee e)
        {
            var note = _notes.Pop();

            note.transform.DOKill();
            note.Interact(e);

            PlaySfx();
        }

        void PlaySfx() => RuntimeManager.PlayOneShotAttached(_handleSfx, gameObject);

        void Update()
        {
            if (_holding)
            {
                _holdTime += Time.deltaTime;
                _lever.localEulerAngles = _leverAxis * Mathf.LerpAngle(_leverClosedAngle, _leverOpenAngle, _holdTime / _holdThreshold);

                if (_holdTime > _holdThreshold)
                {
                    AttemptToStoreNote();
                    _holding = false;
                }
            }
        }

        void AttemptToStoreNote()
        {
            if (Interactor.LeftHand.Contains<BankNote>(out var note))
            {
                StoreNote(note.RetrieveFromStack());
                _leverClosedAngle = GetClosedAngle();
            }
        }

        protected override void OnEndInteraction()
        {
            _lever.DOLocalRotate(_leverAxis * _leverClosedAngle, _leverReturnTime)
                .SetEase(_leverReturnEase);

            if (!_holding) return;
            
            if (_notes.Count > 0)
            {
                GrabNote(Interactor);
            }

            _holding = false;
        }

        static Vector3 Randomize(Vector3 vector)
        {
            for (int i = 0; i < 3; i++)
            {
                vector[i] = Random.Range(-vector[i], vector[i]);
            }

            return vector;
        }
    }
}
