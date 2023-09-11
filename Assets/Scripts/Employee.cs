using DG.Tweening;
using Recounter.Items;
using TMPro;
using UnityEngine;

public class Employee : MonoBehaviour
{
    [field: SerializeField] public Hotbar ItemHotbar { get; private set; }
    [field: SerializeField] public PlayerController Controller { get; private set; }
    [field: SerializeField] public PlayerInteraction Interaction { get; private set; }
    [field: SerializeField] public Hand LeftHand { get; private set; }
    [field: SerializeField] public Hand RightHand { get; private set; }

    [field: SerializeField] public Camera Camera { get; private set; }

    [Header("HUD")]
    [SerializeField] Canvas _hudCanvas;
    [SerializeField] CanvasGroup _hudCanvasGroup;
    [SerializeField] float _hudFadeTime;
    [SerializeField] float _messageFadeTime;
    [SerializeField] TextMeshProUGUI _message;

    void Awake()
    {
        _message.alpha = 0;
    }

    public bool HandsAreFree => !(LeftHand.IsFull || RightHand.IsFull);

    public void ShowHud(bool enabled = true)
    {
        _hudCanvas.enabled = true;

        _hudCanvasGroup.DOKill();

        var tween = _hudCanvasGroup.DOFade(enabled ? 1 : 0, _hudFadeTime);

        if (enabled) return;

        tween.OnComplete(() => _hudCanvas.enabled = false);
    }
    
    /// <summary>
    /// Shows a message to this player.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="duration">How long the message stays.</param>
    public void ShowMessage(string message, float duration = 3)
    {
        SetMessage(message);
        TweenFadeOut().SetDelay(duration);
    }

    public void ShowPermanentMessage(string message) => SetMessage(message);

    void SetMessage(string message)
    {
        _message.text = message;
        _message.alpha = 1;
    }

    /// <summary>
    /// Immediately hides the shown message.
    /// </summary>
    public void HideMessage() => TweenFadeOut();

    Tween TweenFadeOut()
    {
        _message.DOKill();
        return _message.DOFade(0, _messageFadeTime);
    }

    public void ClearMessage()
    {
        _message.DOKill();
        _message.alpha = 0;
    }
}
