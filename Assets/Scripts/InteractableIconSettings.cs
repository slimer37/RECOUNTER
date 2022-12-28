using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Settings/Interactable Icon Settings")]
public class InteractableIconSettings : ScriptableObject
{
    [SerializeField] Sprite[] icons;

    public Sprite GetSprite(Interactable.Icon icon) => icons[(int)icon];
}

public static class IconInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        settings = Addressables.LoadAssetAsync<InteractableIconSettings>("InteractableIconSettings").WaitForCompletion();
    }

    static InteractableIconSettings settings;

    public static Sprite ToSprite(this Interactable.Icon icon) => settings.GetSprite(icon);
}
