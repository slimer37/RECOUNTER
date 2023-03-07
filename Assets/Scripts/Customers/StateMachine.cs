using UnityEngine;

namespace Recounter.Customers
{
    public abstract class StateMachine : MonoBehaviour
    {
        protected State State;

        public void SetState(State state)
        {
            State = state;
            StartCoroutine(state.Start());
        }
    }
}
