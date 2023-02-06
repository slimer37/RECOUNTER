using UnityEngine;

public class Slosh : MonoBehaviour
{
    [SerializeField] int materialIndex = 0;
    [SerializeField] float maxWobble = 0.5f;
    [SerializeField] float wobbleSpeed = 3f;
    [SerializeField] float recovery = 3f;

    float pulse;

    Material material;
    Vector3 lastPos;
    Vector3 lastRot;

    Vector3 wobbleVector;

    int wobbleXId;
    int wobbleZId;

    Renderer liquidRenderer;

    void Start()
    {
        liquidRenderer = GetComponent<Renderer>();

        material = liquidRenderer.materials[materialIndex];

        pulse = 2 * Mathf.PI * wobbleSpeed;

        wobbleXId = Shader.PropertyToID("_WobbleX");
        wobbleZId = Shader.PropertyToID("_WobbleZ");

        RecordTransform();
    }

    void Update()
    {
        var currentAngles = transform.eulerAngles;

        var angularVelocity = Vector3.zero;

        angularVelocity.x = Mathf.DeltaAngle(currentAngles.z, lastRot.z);
        angularVelocity.z = Mathf.DeltaAngle(currentAngles.x, lastRot.x);

        var deltaPosition = lastPos - transform.position;

        // Add clamped velocity to wobble
        wobbleVector += Vector3.ClampMagnitude(
            maxWobble * (deltaPosition + Time.deltaTime * angularVelocity),
            maxWobble);

        wobbleVector = Vector3.Lerp(wobbleVector, Vector3.zero, Time.deltaTime * recovery);

        if (liquidRenderer.isVisible)
        {
            var wobbleAmount = wobbleVector * Mathf.Sin(pulse * Time.time);

            material.SetFloat(wobbleXId, Vector3.Dot(wobbleAmount, transform.forward));
            material.SetFloat(wobbleZId, Vector3.Dot(wobbleAmount, transform.right));
        }

        RecordTransform();
    }

    void RecordTransform()
    {
        lastPos = transform.position;
        lastRot = transform.rotation.eulerAngles;
    }
}