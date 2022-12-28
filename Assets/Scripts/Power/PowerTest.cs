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
        inlet.Powered += () => SetLightbulb(true);
        inlet.Depowered += () => SetLightbulb(false);

        SetLightbulb(false);
    }

    void SetLightbulb(bool on)
    {
        lightbulbRenderer.material = on ? onMaterial : offMaterial;
        lightSource.enabled = on;
    }
}