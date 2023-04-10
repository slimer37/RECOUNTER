using System;
using UnityEngine;

namespace Recounter.Service
{
    public abstract class PaymentMethod : MonoBehaviour
    {
        public abstract string Label { get; }

        Action _finish;

        bool _inProgress;

        public void Initiate(Action finish)
        {
            if (_inProgress) throw new InvalidOperationException("Cannot start new payment; Payment is still in progress.");

            _inProgress = true;
            _finish = finish;
            OnInitiate();
        }

        protected abstract void OnInitiate();

        protected void Finish()
        {
            if (!_inProgress) throw new InvalidOperationException("Cannot finish; There is no payment in progress.");

            _finish?.Invoke();
            _inProgress = false;
        }
    }
}
