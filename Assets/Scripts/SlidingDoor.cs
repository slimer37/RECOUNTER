using DG.Tweening;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    [SerializeField] float _duration;
    [SerializeField] Transform _door;
    [SerializeField] Vector3 _moveAmount;
    [SerializeField] Ease ease;

    Tween _openTween;

    bool _open;

    void Awake()
    {
        _openTween = _door.DOLocalMove(_moveAmount, _duration)
            .SetRelative(true)
            .SetAutoKill(false)
            .SetEase(ease)
            .Pause();
    }

    void OnDestroy()
    {
        _openTween.Kill();
    }

    void OnTriggerStay(Collider other)
    {
        _open = true;
    }

    void FixedUpdate()
    {
        if (_open)
        {
            _openTween.PlayForward();
        }
        else
        {
            _openTween.PlayBackwards();
        }

        _open = false;
    }
}
