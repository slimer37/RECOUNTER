using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using UnityEngine;

public class SimpleDoor : Interactable
{
    [SerializeField] Transform door;
    [SerializeField] Vector3 localOpenRotation;
    [SerializeField] Ease openEase;
    [SerializeField] Ease closeEase;
    [SerializeField] float tweenSpeed;
    [SerializeField] string label;

    [Header("SFX")]
    [SerializeField] EventReference creakSfx;
    [SerializeField] float completeSwingTime;
    [SerializeField] EventReference completeSwingSfx;

    EventInstance creakSfxInstance;

    bool isOpen;

    Vector3 localClosedRotation;

    Tween tween;

    protected override bool CanInteract(Employee e)
    {
        return !tween.IsActive();
    }

    public override HudInfo GetHudInfo(Employee e)
    {
        return CanInteract(e)
            ? new()
            {
                icon = Icon.Hand,
                text = $"Open {label}",
            }
            : BlankHud;
    }

    void Awake()
    {
        localClosedRotation = door.localEulerAngles;

        if (completeSwingSfx.IsNull) return;

        creakSfxInstance = RuntimeManager.CreateInstance(creakSfx);
        RuntimeManager.AttachInstanceToGameObject(creakSfxInstance, door);
    }

    protected override void OnInteract(Employee e)
    {
        tween?.Kill();

        isOpen = !isOpen;
        tween = door.DOLocalRotate(isOpen ? localOpenRotation : localClosedRotation, tweenSpeed)
            .SetSpeedBased()
            .SetEase(isOpen ? openEase : closeEase);

        StopAllCoroutines();
        StartCoroutine(PlayCompleteSwingSfx());

        if (!completeSwingSfx.IsNull)
        {
            creakSfxInstance.start();
        }
    }

    IEnumerator PlayCompleteSwingSfx()
    {
        if (completeSwingSfx.IsNull) yield break;

        yield return new WaitForSeconds(completeSwingTime);

        RuntimeManager.PlayOneShotAttached(completeSwingSfx, door.gameObject);
    }
}
