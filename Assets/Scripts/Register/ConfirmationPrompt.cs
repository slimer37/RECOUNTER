using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter
{
    public class ConfirmationPrompt : MonoBehaviour
    {
        [SerializeField] TMP_Text _titleText;
        [SerializeField] TMP_Text _messageText;
        [SerializeField] Button _yes;
        [SerializeField] TMP_Text _yesText;
        [SerializeField] Button _no;
        [SerializeField] TMP_Text _noText;
        [SerializeField] CanvasGroup _group;

        Action _yesAction;
        Action _noAction;

        void Awake()
        {
            _yes.onClick.AddListener(Yes);
            _no.onClick.AddListener(No);

            Hide();
        }

        void Yes()
        {
            _yesAction?.Invoke();
            Hide();
        }

        void No()
        {
            _noAction?.Invoke();
            Hide();
        }

        public void Prompt(string message, Action yes, Action no = null, string yesLabel = "Yes", string noLabel = "No")
            => Prompt("Confirm", message, yes, no, yesLabel, noLabel);

        public void Prompt(string title, string message, Action yes, Action no = null, string yesLabel = "Yes", string noLabel = "No")
        {
            _titleText.text = title;
            _messageText.text = message;

            _yesAction = yes;
            _yesText.text = yesLabel;

            _noAction = no;
            _noText.text = noLabel;

            Show();
        }

        void Show()
        {
            _group.alpha = 1;
            _group.interactable = true;
            _group.blocksRaycasts = true;
        }

        void Hide()
        {
            _group.alpha = 0;
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }
    }
}
