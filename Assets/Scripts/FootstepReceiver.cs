using FMODUnity;
using UnityEngine;

public class FootstepReceiver : MonoBehaviour
{
    [SerializeField] EventReference footstepSfx;

    [Header("Optional")]
    [SerializeField] PlayerController controller;

    public void PlayFootstep()
    {
        if (controller && !controller.ImpulseFootstep()) return;

        RuntimeManager.PlayOneShot(footstepSfx, transform.position);
    }
}
