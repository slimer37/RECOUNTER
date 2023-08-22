using Slimer37.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter.UI
{
    public class NewSaveMenu : MonoBehaviour
    {
        [SerializeField] TMP_InputField _nameField;
        [SerializeField] Button _createButton;
        [SerializeField] TMP_Text _createButtonText;

        [Header("Warning")]
        [SerializeField] TMP_Text _validNameWarning;
        [SerializeField] GameObject _warningPanel;
        [SerializeField] string _warningFormat;

        void Awake()
        {
            _nameField.onValueChanged.AddListener(CheckName);

            _createButton.onClick.AddListener(CreateSave);

            _warningPanel.SetActive(false);
        }

        void CreateSave()
        {
            var name = _nameField.text;
            var data = SaveData.CreateWithFile(name);
            GameManager.StartGame(data);
        }

        void CheckName(string name)
        {
            if (GameSerializer.ValidateFileName(name, out var validFileName))
            {
                _warningPanel.SetActive(false);
            }
            else
            {
                _warningPanel.SetActive(true);
                _validNameWarning.text = string.Format(_warningFormat, validFileName);
            }

            var exists = GameSerializer.AlreadyExists(validFileName);

            _createButtonText.text = exists ? "File already exists" : "Create";

            _createButton.interactable = !exists;
        }
    }
}
