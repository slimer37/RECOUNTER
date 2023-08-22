using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter
{
    public class AutoSave : MonoBehaviour
    {
        [SerializeField] float _autoSaveIntervalMinutes;

        [Header("Icon Animation")]
        [SerializeField] Image _autoSaveIcon;
        [SerializeField] float _fadeDuration;
        [SerializeField] int _loops;

        Tween _animation;

        void Awake()
        {
            _animation = _autoSaveIcon
                .DOFade(0, _fadeDuration)
                .From()
                .SetLoops(_loops * 2, LoopType.Yoyo)
                .SetEase(Ease.OutSine)
                .SetAutoKill(false)
                .Pause()
                .SetUpdate(true);

            GameManager.StoreData.Saved += OnSaved;
        }

        void OnDestroy()
        {
            GameManager.StoreData.Saved -= OnSaved;
        }

        void OnSaved()
        {
            _animation.Restart();

            StopAllCoroutines();
            StartCoroutine(SaveAfterInterval());
        }

        void Start()
        {
            StartCoroutine(SaveAfterInterval());
        }

        IEnumerator SaveAfterInterval()
        {
            yield return new WaitForSeconds(_autoSaveIntervalMinutes * 60);
            Save();
        }

        void Save()
        {
            GameManager.StoreData.Save();

            Debug.Log("Auto saved.", this);
        }
    }
}
