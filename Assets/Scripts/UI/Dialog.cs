using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter.UI
{
    public readonly struct DialogButton
    {
        public readonly string label;
        public readonly Color color;

        public DialogButton(string label, Color color)
        {
            this.label = label;
            this.color = color;
        }
    }

    public class Dialog : MonoBehaviour
    {
        [SerializeField] bool _markAsMain;

        [Header("Window")]
        [SerializeField] TMP_Text _title;
        [SerializeField] TMP_Text _message;
        [SerializeField] CanvasGroup _canvasGroup;
        [SerializeField] float _fadeDuration;

        [Header("Buttons")]
        [SerializeField] Button _template;
        [SerializeField] Transform _buttonGroup;

        [Header("Defaults")]
        [SerializeField] Color _okColor;
        [SerializeField] Color _yesColor;
        [SerializeField] Color _noColor;

        Action<int> _response;

        List<GameObject> _buttonInstances = new();

        public bool IsOpen { get; private set; }

        public static Dialog Main
        {
            get
            {
                if (!s_main)
                {
                    throw new NullReferenceException("No main Dialog exists." +
                        "Either none have been marked as main or it was destroyed.");
                }

                return s_main;
            }
        }

        static Dialog s_main;

        void Awake()
        {
            if (!_markAsMain) return;

            if (s_main) throw new Exception("More than one main Dialog exists.");

            s_main = this;

            Fade(false, true);
        }

        void Fade(bool fadeIn, bool instant = false)
        {
            IsOpen = fadeIn;

            _canvasGroup.blocksRaycasts = _canvasGroup.interactable = fadeIn;

            if (instant)
                _canvasGroup.alpha = fadeIn ? 1 : 0;
            else
                _canvasGroup.DOFade(fadeIn ? 1 : 0, _fadeDuration);
        }

        public void Form(Action<int> response, string title, string message, params DialogButton[] buttons)
        {
            if (buttons is null or { Length: 0 })
                throw new ArgumentNullException(nameof(buttons), "No buttons provided for dialog.");

            Fade(true);

            _title.text = title;
            _message.text = message;

            _response = response;

            foreach (var buttonInstance in _buttonInstances)
            {
                Destroy(buttonInstance);
            }

            _buttonInstances.Clear();

            _template.gameObject.SetActive(true);

            for (var i = 0; i < buttons.Length; i++)
            {
                DialogButton button = buttons[i];
                var buttonObject = Instantiate(_template, _buttonGroup);

                buttonObject.GetComponent<Image>().color = button.color;
                buttonObject.GetComponentInChildren<TMP_Text>().text = button.label;

                var index = i;
                buttonObject.onClick.AddListener(() => Respond(index));

                _buttonInstances.Add(buttonObject.gameObject);
            }

            _template.gameObject.SetActive(false);
        }

        void Respond(int index)
        {
            Fade(false);

            _response?.Invoke(index);

            _response = null;
        }

        public void Info(
            Action response,
            string title,
            string message,
            string label = "OK") =>
            Form(
                _ => response?.Invoke(),
                title,
                message,
                new DialogButton(label, Main._okColor));

        public void Confirm(
            Action response,
            string title,
            string message,
            string confirmLabel = "Yes",
            string cancelLabel = "No") =>
            YesNo(
                b =>
                {
                    if (b) response?.Invoke();
                },
                title, message, confirmLabel, cancelLabel);

        public void YesNo(
            Action<bool> response,
            string title,
            string message,
            string yesLabel = "Yes",
            string noLabel = "No") =>
            Form(
                i => response?.Invoke(i == 0),
                title,
                message,
                new DialogButton(yesLabel, Main._yesColor),
                new DialogButton(noLabel, Main._noColor));
    }
}
