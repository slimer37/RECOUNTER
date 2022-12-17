using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CustomerController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField, AnimatorParam("animator")] string speedParam;

    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        animator.SetFloat(speedParam, agent.velocity.magnitude);
    }

    public void MoveTo(Vector3 position)
    {
        agent.SetDestination(position);
    }
}
