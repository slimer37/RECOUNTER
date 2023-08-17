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
                .Pause();
        }

        IEnumerator Start()
        {
            while (true)
            {
                yield return new WaitForSeconds(_autoSaveIntervalMinutes * 60);
                Save();
            }
        }

        void Save()
        {
            GameManager.StoreData.Save();
            _animation.Restart();
        }
    }
}
