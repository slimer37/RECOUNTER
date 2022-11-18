using UnityEngine;

public class Slosh : MonoBehaviour
{
    public float MaxWobble = 6f;
    public float WobbleSpeed = 1f;
    public float Recovery = 1f;

    float pulse;
    float time = 0.5f;

    Renderer rend;
    Vector3 lastPos;
    Vector3 velocity;
    Vector3 lastRot;
    Vector3 angularVelocity;

    Vector3 wobbleAmount;
    Vector3 wobbleAmountToAdd;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    private void Update()
    {
        time += Time.deltaTime;

        // decrease wobble over time
        wobbleAmountToAdd = Vector3.Lerp(wobbleAmountToAdd, Vector3.zero, Time.deltaTime * Recovery);

        // make a sine wave of the decreasing wobble
        pulse = 2 * Mathf.PI * WobbleSpeed;
        wobbleAmount = wobbleAmountToAdd * Mathf.Sin(pulse * time);

        // send it to the shader
        rend.material.SetFloat("_WobbleX", Vector3.Dot(wobbleAmount, transform.forward));
        rend.material.SetFloat("_WobbleZ", Vector3.Dot(wobbleAmount, transform.right));

        // velocity
        velocity = (lastPos - transform.position) / Time.deltaTime;
        angularVelocity = transform.rotation.eulerAngles - lastRot;

        // add clamped velocity to wobble
        wobbleAmountToAdd += Vector3.ClampMagnitude(
            0.01f * MaxWobble * (velocity + new Vector3(angularVelocity.z, 0, angularVelocity.x)),
            MaxWobble);

        // keep last position
        lastPos = transform.position;
        lastRot = transform.rotation.eulerAngles;
    }
}