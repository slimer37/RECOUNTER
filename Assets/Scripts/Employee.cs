using DG.Tweening;
using UnityEngine;
using Recounter;

public class Employee : MonoBehaviour
{
    [field: SerializeField] public Hotbar ItemHotbar { get; private set; }
    [field: SerializeField] public PlayerController Controller { get; private set; }
    [field: SerializeField] public PlayerInteraction Interaction { get; private set; }
    [field: SerializeField] public Hand LeftHand { get; private set; }
    [field: SerializeField] public Hand RightHand { get; private set; }
    [field: SerializeField] public Placer Placer { get; private set; }

    [Header("HUD")]
    [SerializeField] Canvas _hudCanvas;
    [SerializeField] CanvasGroup _hudCanvasGroup;
    [SerializeField] float _hudFadeTime;

    public bool HandsAreFree => !(LeftHand.IsFull || RightHand.IsFull);

    public void ShowHud(bool enabled = true)
    {
        _hudCanvas.enabled = true;

        _hudCanvasGroup.DOKill();

        var tween = _hudCanvasGroup.DOFade(enabled ? 1 : 0, _hudFadeTime);

        if (enabled) return;

        tween.OnComplete(() => _hudCanvas.enabled = false);
    }
}
