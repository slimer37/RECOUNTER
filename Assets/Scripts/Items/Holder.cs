using UnityEngine;

namespace Recounter.Items
{
    public class Holder : MonoBehaviour
    {
        [SerializeField] Hand _hand;
        [SerializeField] Hotbar _hotbar;
        [SerializeField] Vector3 _defaultHoldRotation;
        [SerializeField] Vector3 _baseHoldPosition;
        [SerializeField] Transform _body;

        void Awake()
        {
            _hotbar.ItemBecameActive += Hold;
            _hotbar.ItemPutAway += PutAway;
        }

        void Hold(Item item, bool canResetPosition)
        {
            var adjustedHoldRot = item.OverrideHoldRotation ?? Quaternion.Euler(_defaultHoldRotation);
            var adjustedHoldPos = _baseHoldPosition + item.HoldPosShift;

            item.gameObject.SetActive(true);

            _hand.Hold(item, adjustedHoldPos, adjustedHoldRot);

            if (item.ViewmodelPose.IsValid)
            {
                _hand.SetHandViewmodel(item.ViewmodelPose);
            }

            if (!canResetPosition) return;

            item.transform.SetPositionAndRotation(_body.position, _body.rotation);
        }

        void PutAway(Item item, bool wasItemKept)
        {
            if (!item) return;

            if (wasItemKept)
            {
                item.gameObject.SetActive(false);
            }

            _hand.Clear();
        }
    }
}
