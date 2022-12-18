using NaughtyAttributes;
using System.Collections;
using UnityEngine;

public class RagdollDeath : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Animator ragdollPrefab;
    [SerializeField, AnimatorParam("animator")] string speedParam;

    Renderer[] renderers;
    int speedId;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        speedId = Animator.StringToHash(speedParam);
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void Die()
    {
        StartCoroutine(Swap());
    }

    IEnumerator Swap()
    {
        var state = animator.GetCurrentAnimatorStateInfo(0);

        var ragdoll = Instantiate(ragdollPrefab, transform.position, transform.rotation);

        ragdoll.CrossFade(state.fullPathHash, 0, 0, state.normalizedTime);
        ragdoll.SetFloat(speedId, animator.GetFloat(speedParam));

        HideRenderers();

        yield return null;
        yield return null;

        ragdoll.enabled = false;

        Destroy(gameObject);
    }

    void HideRenderers()
    {
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }
    }
}
