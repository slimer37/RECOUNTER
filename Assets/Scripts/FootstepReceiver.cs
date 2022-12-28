using FMODUnity;
using UnityEngine;

public class FootstepReceiver : MonoBehaviour
{
    [SerializeField] EventReference footstepSfx;

    [Header("Optional")]
    [SerializeField] PlayerController controller;
    [SerializeField] CharacterController characterController;

    public void PlayFootstep()
    {
        if (!characterController || !characterController.isGrounded) return;

        RuntimeManager.PlayOneShot(footstepSfx, transform.position);

        if (!controller) return;

        controller.ImpulseFootstep();
    }
}
