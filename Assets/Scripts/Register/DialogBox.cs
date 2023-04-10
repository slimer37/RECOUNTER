using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter
{
    public class DialogBox : MonoBehaviour
    {
        [SerializeField] TMP_Text _titleText;
        [SerializeField] TMP_Text _messageText;

        [Header("Yes / No")]
        [SerializeField] GameObject _twoButtonGroup;
        [SerializeField] Button _yes;
        [SerializeField] TMP_Text _yesText;
        [SerializeField] Button _no;
        [SerializeField] TMP_Text _noText;
        [SerializeField] CanvasGroup _group;

        [Header("OK")]
        [SerializeField] GameObject _okButtonGroup;
        [SerializeField] Button _ok;
        [SerializeField] TMP_Text _okText;

        Action _yesAction;
        Action _noAction;

        void Awake()
        {
            _yes.onClick.AddListener(Yes);
            _ok.onClick.AddListener(Yes);
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

        public void PromptOK(string title, string message, Action ok = null, string okLabel = "OK")
        {
            _okText.text = okLabel;

            ConfigureAndShow(false, title, message, ok, null);
        }

        public void PromptYesNo(string title, string message, Action yes, Action no = null, string yesLabel = "Yes", string noLabel = "No")
        {
            _yesText.text = yesLabel;
            _noText.text = noLabel;

            ConfigureAndShow(true, title, message, yes, no);
        }

        void ConfigureAndShow(bool twoButtons, string title, string message, Action yes, Action no)
        {
            _twoButtonGroup.SetActive(twoButtons);
            _okButtonGroup.SetActive(!twoButtons);

            _titleText.text = title;
            _messageText.text = message;
            _yesAction = yes;
            _noAction = no;

            Show();
        }

        void Show()
        {
            _group.alpha = 1;
            _group.interactable = true;
            _group.blocksRaycasts = true;
        }

        public void Hide()
        {
            _group.alpha = 0;
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }
    }
}
