using FMODUnity;
using UnityEngine;

public class FootstepReceiver : MonoBehaviour
{
    [SerializeField] EventReference footstepSfx = new() { Path = "event:/Footstep" };

    [Header("Optional")]
    [SerializeField] PlayerController controller;

    public void PlayFootstep()
    {
        RuntimeManager.PlayOneShot(footstepSfx, transform.position);

        if (!controller) return;

        controller.ImpulseFootstep();
    }
}
