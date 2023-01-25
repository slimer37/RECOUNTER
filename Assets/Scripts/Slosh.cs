using UnityEngine;

public class Slosh : MonoBehaviour
{
    public int MaterialIndex = 0;
    public float MaxWobble = 0.5f;
    public float WobbleSpeed = 3f;
    public float Recovery = 3f;

    float pulse;

    Material material;
    Vector3 lastPos;
    Vector3 lastRot;

    Vector3 wobbleVector;

    int wobbleXId;
    int wobbleZId;

    void Start()
    {
        material = GetComponent<Renderer>().materials[MaterialIndex];
        pulse = 2 * Mathf.PI * WobbleSpeed;

        wobbleXId = Shader.PropertyToID("_WobbleX");
        wobbleZId = Shader.PropertyToID("_WobbleZ");
    }

    void Update()
    {
        var currentAngles = transform.eulerAngles;

        var angularVelocity = Vector3.zero;

        angularVelocity.x = Mathf.DeltaAngle(currentAngles.z, lastRot.z);
        angularVelocity.z = Mathf.DeltaAngle(currentAngles.x, lastRot.x);

        var deltaPosition = lastPos - transform.position;

        // add clamped velocity to wobble
        wobbleVector += Vector3.ClampMagnitude(
            MaxWobble * (deltaPosition + Time.deltaTime * angularVelocity),
            MaxWobble);

        wobbleVector = Vector3.Lerp(wobbleVector, Vector3.zero, Time.deltaTime * Recovery);

        var wobbleAmount = wobbleVector * Mathf.Sin(pulse * Time.time);

        material.SetFloat(wobbleXId, Vector3.Dot(wobbleAmount, transform.forward));
        material.SetFloat(wobbleZId, Vector3.Dot(wobbleAmount, transform.right));

        lastPos = transform.position;
        lastRot = transform.rotation.eulerAngles;
    }
}