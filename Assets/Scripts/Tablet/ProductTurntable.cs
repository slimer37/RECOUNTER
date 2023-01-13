using NaughtyAttributes;
using Recounter.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Recounter.Tablet
{
    public class ProductTurntable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField, Layer] int _layer;

        [Header("Image")]
        [SerializeField] Vector2Int _resolution;
        [SerializeField] RawImage _image;
        [SerializeField] float _padding;

        [Header("Setup")]
        [SerializeField, Required] Camera _camera;
        [SerializeField, Required] Transform _rotator;

        [Header("Rotation Control")]
        [SerializeField] float _startRotation;
        [SerializeField] float _sensitivity = 1;
        [SerializeField] float _idleSpeed = 1;
        [SerializeField] float _speedUpTime = 1;
        [SerializeField] float _idleTime = 1;

        bool _freeze;
        bool _idle;

        float _timeIdling;
        float _t;

        Transform _subject;

        public void Display(Product product)
        {
            _subject = Instantiate(product.Prefab, _rotator).transform;

            _subject.gameObject.SetHierarchyLayersWithoutRestore(_layer);

            _subject.localRotation = Quaternion.identity;

            Rigidbody rb;
            if (rb = _subject.GetComponentInChildren<Rigidbody>())
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            var rend = _subject.GetComponentInChildren<Renderer>();

            _subject.transform.localPosition = -rend.localBounds.center;

            _rotator.rotation = Quaternion.Euler(Vector3.up * _startRotation);

            RuntimePreviewGenerator.CalculateCameraPosition(_camera, rend.bounds, _padding);

            _camera.enabled = true;
        }

        void Awake()
        {
            var renderTexture = new RenderTexture(_resolution.x, _resolution.y, 16);

            _image.texture = renderTexture;

            _camera.targetTexture = renderTexture;

            _camera.enabled = false;
        }

        void OnEnable() => _idle = true;

        void OnDisable()
        {
            if (!_subject) return;

            Destroy(_subject.gameObject);
            _subject = null;

            _camera.enabled = false;
        }

        void Update()
        {
            if (_freeze || !_subject) return;

            if (!_idle)
            {
                _timeIdling += Time.deltaTime;

                if (_timeIdling > _idleTime)
                {
                    _idle = true;
                    _t = 0;
                }

                return;
            }

            _rotator.Rotate(-Mathf.Lerp(0, _idleSpeed, _t) * Time.deltaTime * Vector3.up);
            _t += Time.deltaTime / _speedUpTime;
        }

        public void OnBeginDrag(PointerEventData eventData) => _freeze = true;

        public void OnDrag(PointerEventData eventData)
        {
            _rotator.Rotate(-eventData.delta.x * _sensitivity * Time.deltaTime * Vector3.up);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _freeze = false;
            _idle = false;
            _timeIdling = 0;
        }
    }
}
