using UnityEngine;
using UnityEngine.UI;

namespace Recounter.UI
{
    public class PlayPrompt : MonoBehaviour
    {
        [SerializeField] Button _playButton;
        [SerializeField] Canvas _promptCanvas;
        [SerializeField] Canvas _newCanvas;
        [SerializeField] Canvas _loadCanvas;
        [SerializeField] Button _newButton;
        [SerializeField] Button _loadButton;

        void Awake()
        {
            HidePrompt();

            _playButton.onClick.AddListener(ShowPrompt);

            _newButton.onClick.AddListener(ShowNewSaveMenu);
            _loadButton.onClick.AddListener(ShowLoadMenu);
        }

        void ShowPrompt() => _promptCanvas.enabled = true;

        void HidePrompt() => _promptCanvas.enabled = false;

        void ShowNewSaveMenu() => _newCanvas.enabled = true;

        void ShowLoadMenu() => _loadCanvas.enabled = true;
    }
}
