using UnityEngine;

public class Slosh : MonoBehaviour
{
    public int MaterialIndex = 0;
    public float MaxWobble = 0.15f;
    public float WobbleSpeed = 3f;
    public float Recovery = 5f;

    float pulse;
    float time = 0.5f;

    Material material;
    Vector3 lastPos;
    Vector3 velocity;
    Vector3 lastRot;
    Vector3 angularVelocity;

    Vector3 wobbleAmount;
    Vector3 wobbleAmountToAdd;

    void Start()
    {
        material = GetComponent<Renderer>().materials[MaterialIndex];
    }

    void Update()
    {
        if (float.IsNaN(wobbleAmountToAdd.x))
        {
            wobbleAmountToAdd = Vector3.zero;
        }

        time += Time.deltaTime;

        // decrease wobble over time
        wobbleAmountToAdd = Vector3.Lerp(wobbleAmountToAdd, Vector3.zero, Time.deltaTime * Recovery);

        // make a sine wave of the decreasing wobble
        pulse = 2 * Mathf.PI * WobbleSpeed;
        wobbleAmount = wobbleAmountToAdd * Mathf.Sin(pulse * time);

        // send it to the shader
        material.SetFloat("_WobbleX", Vector3.Dot(wobbleAmount, transform.forward));
        material.SetFloat("_WobbleZ", Vector3.Dot(wobbleAmount, transform.right));

        // velocity
        velocity = (lastPos - transform.position) / Time.deltaTime;

        var currentAngles = transform.eulerAngles;

        for (var i = 0; i < 3; i++)
        {
            angularVelocity[i] = Mathf.DeltaAngle(currentAngles[i], lastRot[i]);
        }

        // add clamped velocity to wobble
        wobbleAmountToAdd += Vector3.ClampMagnitude(
            0.01f * MaxWobble * (velocity + new Vector3(angularVelocity.z, 0, angularVelocity.x)),
            MaxWobble);

        // keep last position
        lastPos = transform.position;
        lastRot = transform.rotation.eulerAngles;
    }
}