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
        RuntimeManager.PlayOneShot(footstepSfx, transform.position);

        if (!controller || !characterController.isGrounded) return;

        controller.ImpulseFootstep();
    }
}
