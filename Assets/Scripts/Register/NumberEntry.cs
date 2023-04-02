using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Recounter
{
    public class NumberEntry : MonoBehaviour
    {
        [SerializeField] TMP_Text _field;
        [SerializeField] CanvasGroup _window;
        [SerializeField] Button _keyPrefab;
        [SerializeField] TMP_Text _keyText;
        [SerializeField] Transform _keyParent;
        [SerializeField] Button _acceptButton;
        [SerializeField] float _maxValue = 100000;

        Action<float> _numberEntered;
        Action _canceled;

        float _value;

        void Awake()
        {
            CreateKeypad();
            UpdateDisplay();

            //Close();
        }

        void CreateKeypad()
        {
            int[] order = { 7, 8, 9, 4, 5, 6, 1, 2, 3, 0 };

            foreach (var num in order)
            {
                CreateKey(num.ToString(), () => EnterNumber(num));
            }

            CreateKey("00", DoubleZero);
            CreateKey("C", Clear);

            _keyPrefab.gameObject.SetActive(false);

            void CreateKey(string label, UnityAction click)
            {
                _keyText.text = label;

                Instantiate(_keyPrefab, _keyParent).onClick.AddListener(click);
            }
        }

        void EnterNumber(int input)
        {
            _value *= 10;
            _value += input * 0.01f;

            UpdateDisplay();
        }

        void DoubleZero()
        {
            _value *= 100;
            UpdateDisplay();
        }
        void Clear()
        {
            if (_value == _maxValue)
            {
                _value = 0;
            }
            else
            {
                _value -= _value % 0.1f;
                _value /= 10;
            }

            UpdateDisplay();
        }

        void UpdateDisplay()
        {
            if (_value > _maxValue)
                _value = _maxValue;

            _field.text = _value.ToString("C");
        }

        public void PromptNumber(Action<float> action, Action cancel)
        {
            Open();

            _numberEntered = action;
            _canceled = cancel;
        }

        public void Accept()
        {
            _numberEntered?.Invoke(_value);
            Close();
        }

        public void Cancel()
        {
            _canceled?.Invoke();
            Close();
        }

        void Open()
        {
            _window.alpha = 1;
            _window.interactable = true;
            _window.blocksRaycasts = true;
        }

        void Close()
        {
            _window.alpha = 0;
            _window.interactable = false;
            _window.blocksRaycasts = false;
        }
    }
}
