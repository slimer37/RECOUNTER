using UnityEngine;

namespace Recounter.Customers
{
    public class Customer : StateMachine
    {
        [field: SerializeField] public CustomerController Controller { get; private set; }

        void Start()
        {
            SetState(new WanderState(this));
        }
    }
}
