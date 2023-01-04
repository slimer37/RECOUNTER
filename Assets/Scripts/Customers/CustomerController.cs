using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CustomerController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField, AnimatorParam("animator")] string xSpeedParam;
    [SerializeField, AnimatorParam("animator")] string ySpeedParam;

    NavMeshAgent agent;

    int xSpeedId;
    int ySpeedId;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        xSpeedId = Animator.StringToHash(xSpeedParam);
        ySpeedId = Animator.StringToHash(ySpeedParam);
    }

    void Update()
    {
        var localVelocity = transform.InverseTransformDirection(agent.velocity);
        animator.SetFloat(xSpeedId, localVelocity.x);
        animator.SetFloat(ySpeedId, localVelocity.z);
    }

    public void MoveTo(Vector3 position)
    {
        agent.SetDestination(position);
    }
}
