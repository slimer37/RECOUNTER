using System.Collections;
using UnityEngine;

namespace Recounter.Customers
{
    public abstract class State
    {
        protected Customer Customer { get; }

        public State(Customer customer)
        {
            Customer = customer;
        }

        public virtual IEnumerator Start()
        {
            yield break;
        }
    }

    public class WanderState : State
    {
        public WanderState(Customer customer) : base(customer) { }

        public override IEnumerator Start()
        {
            while (true)
            {
                Customer.Controller.MoveTo(Customer.transform.position + (Vector3)Random.insideUnitCircle * 5);
                yield return new WaitForSeconds(1.0f);
            }
        }
    }
}
