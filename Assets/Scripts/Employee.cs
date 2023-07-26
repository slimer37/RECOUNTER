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

    [Header("HUD")]
    [SerializeField] Canvas _hudCanvas;
    [SerializeField] CanvasGroup _hudCanvasGroup;
    [SerializeField] float _hudFadeTime;
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
    
    public void ShowMessage(string message, float duration = 3, float fadeTime = 0.5f)
    {
        _message.text = message;
        _message.alpha = 1;

        _message.DOKill();
        _message.DOFade(0, fadeTime).SetDelay(duration);
    }

    public void ClearMessage()
    {
        _message.DOKill();
        _message.alpha = 0;
    }
}
