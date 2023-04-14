using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter
{
    public class InteractionReticle : MonoBehaviour
    {
        [SerializeField] LayerMask _interactableMask;
        [SerializeField] Transform _cam;
        [SerializeField] TextMeshProUGUI _text;

        [Header("Fade Reticle")]
        [SerializeField] float _detectionRadius;
        [SerializeField] float _fade;
        [SerializeField] CanvasGroup _fadeReticle;

        [Header("Animation")]
        [SerializeField] float _punchAmount;
        [SerializeField] float _punchDuration;

        [Header("Icons")]
        [SerializeField] Image _fillBar;
        [SerializeField] Image _iconImage;
        [SerializeField] InteractableIconSettings _iconSettings;

        Tween _punch;

        float _targetAlpha;

        void Awake()
        {
            _punch = _iconImage.rectTransform.DOPunchScale(Vector3.one * _punchAmount, _punchDuration)
                .Pause().SetAutoKill(false);
        }

        public void Clear()
        {
            _text.text = "";
            _iconImage.sprite = _iconSettings.GetSprite(Interactable.Icon.None);
            _fadeReticle.alpha = 1;
            _fillBar.fillAmount = 0;
        }

        void OnEnable() => Clear();

        void Update() => _fadeReticle.alpha = Mathf.Lerp(_fadeReticle.alpha, _targetAlpha, _fade * Time.deltaTime);

        void FixedUpdate()
        {
            _targetAlpha = 0;

            if (Physics.CheckSphere(_cam.position, _detectionRadius, _interactableMask))
            {
                _targetAlpha = 1;
            }
        }

        public void UpdateUI(Interactable.HudInfo info, bool forcePunch = false)
        {
            var iconSprite = _iconSettings.GetSprite(info.icon);
            var fill = info.fill ?? 0;

            // Punch when icon changes (except if it's the blank pointer).
            if (info.icon != Interactable.Icon.None && (forcePunch || _iconImage.sprite != iconSprite || _text.text != info.text))
                _punch.Restart();

            _text.text = info.text;
            _iconImage.sprite = iconSprite;
            _fillBar.fillAmount = fill;
        }

    }
}
