using System.Collections;
using UnityEngine;

namespace Recounter.Service
{
    public class CashPayment : PaymentMethod
    {
        [SerializeField] CashDrawer _drawer;

        public override string Label => "Cash";

        protected override void OnInitiate() => StartCoroutine(OpenDrawer());

        IEnumerator OpenDrawer()
        {
            if (!_drawer.IsOpen) _drawer.Open();

            yield return new WaitUntil(() => !_drawer.IsOpen);

            Finish();
        }
    }
}
