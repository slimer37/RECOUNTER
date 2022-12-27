using UnityEngine;

public class PowerTest : MonoBehaviour
{
    [SerializeField] Renderer lightbulbRenderer;
    [SerializeField] Material onMaterial;
    [SerializeField] Light lightSource;
    [SerializeField] PowerInlet inlet;

    Material offMaterial;

    void Awake()
    {
        offMaterial = lightbulbRenderer.material;
        inlet.Powered += OnPower;
        inlet.Depowered += OnPower;

        SetLightbulb(false);
    }

    void OnPower()
    {
        SetLightbulb(true);
    }

    void SetLightbulb(bool on)
    {
        lightbulbRenderer.material = on ? onMaterial : offMaterial;
        lightSource.enabled = on;
    }
}
