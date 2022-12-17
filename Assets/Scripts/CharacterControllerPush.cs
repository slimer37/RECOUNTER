using UnityEngine;

public class CharacterControllerPush : MonoBehaviour
{
    [SerializeField] float _pushForce;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!hit.rigidbody) return;

        hit.rigidbody.AddForce(hit.moveDirection * _pushForce);
    }
}
