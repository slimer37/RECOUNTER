using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Recounter
{
    public class PlayPrompt : MonoBehaviour
    {
        [SerializeField] Button _playButton;
        [SerializeField] Canvas _promptCanvas;
        [SerializeField] Canvas _newCanvas;
        [SerializeField] Canvas _loadCanvas;
        [SerializeField] Button _newButton;
        [SerializeField] Button _loadButton;

        InputAction _esc;

        void Awake()
        {
            HidePrompt();

            _playButton.onClick.AddListener(ShowPrompt);

            _newButton.onClick.AddListener(ShowNewSaveMenu);
            _loadButton.onClick.AddListener(ShowLoadMenu);

            _esc = new Controls().Menu.Exit;
            _esc.performed += _ => Exit();
            _esc.Enable();
        }

        void OnDestroy()
        {
            _esc.Dispose();
        }

        void ShowPrompt() => _promptCanvas.enabled = true;

        void Exit()
        {
            if (_newCanvas.enabled || _loadCanvas.enabled) return;

            HidePrompt();
        }

        void HidePrompt() => _promptCanvas.enabled = false;

        void ShowNewSaveMenu() => _newCanvas.enabled = true;

        void ShowLoadMenu() => _loadCanvas.enabled = true;
    }
}
