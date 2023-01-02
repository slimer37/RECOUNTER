using DG.Tweening;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

public class PoweredSwitch : Interactable
{
    [SerializeField] PowerInlet inlet;
    [SerializeField] HudInfo interactHud;

    [Header("Animation")]
    [SerializeField] bool animate;
    [SerializeField] bool punch;
    [SerializeField] Transform tweenTarget;
    [SerializeField] Vector3 scaleAmount;
    [SerializeField] Vector3 moveAmount;
    [SerializeField] float duration;

    [Header("SFX")]
    [SerializeField] EventReference switchSound;

    public UnityEvent<bool> Switched;

    public bool IsActuallyOn { get; private set; }
    public bool IsSwitchedOn { get; private set; }

    bool isAnimating;

    void Awake()
    {
        inlet.StateChanged.AddListener(VerifyPowerState);
    }

    protected override bool CanInteract(Employee e) => !isAnimating;

    protected override HudInfo FormHud(Employee e) => interactHud;

    protected override void OnInteract(Employee e)
    {
        Switch(!IsSwitchedOn);

        Animate();

        if (!switchSound.IsNull)
            RuntimeManager.PlayOneShotAttached(switchSound, gameObject);
    }

    void VerifyPowerState(bool powered)
    {
        if (powered && IsSwitchedOn)
        {
            Set(true);
        }
        else if (!powered && IsSwitchedOn)
        {
            Set(false);
        }
    }

    void Switch(bool on)
    {
        if (IsSwitchedOn == on) return;

        IsSwitchedOn = on;

        if (on && !inlet.IsPluggedIn) return;

        Set(on);
    }

    void Set(bool on)
    {
        IsActuallyOn = on;

        Switched?.Invoke(on);
    }

    void Animate()
    {
        if (!animate) return;

        isAnimating = true;

        var scale = scaleAmount;
        var move = moveAmount;

        if (!punch && !IsSwitchedOn)
        {
            scale *= -1;
            move *= -1;
        }

        if (punch)
        {
            tweenTarget.DOPunchPosition(move, duration).SetRelative();
            tweenTarget.DOPunchScale(scale, duration).SetRelative().OnComplete(() => isAnimating = false);
        }
        else
        {
            tweenTarget.DOLocalMove(move, duration).SetRelative();
            tweenTarget.DOScale(scale, duration).SetRelative().OnComplete(() => isAnimating = false);
        }
    }
}
