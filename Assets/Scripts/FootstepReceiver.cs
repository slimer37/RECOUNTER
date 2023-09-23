using FMODUnity;
using Recounter;
using UnityEngine;

public class FootstepReceiver : MonoBehaviour
{
    [SerializeField] EventReference footstepSfx;

    public void PlayFootstep()
    {
        RuntimeManager.PlayOneShot(footstepSfx, transform.position);
    }
}
