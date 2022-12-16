using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] Canvas[] additionalMenus;

    Controls controls;

    public static event Action<bool> Paused;

    public static bool IsPaused { get; private set; }

    void Awake()
    {
        IsPaused = false;

        controls = new Controls();
        controls.Menu.Exit.performed += _ => SetPaused(!IsPaused);
        controls.Enable();

#if UNITY_EDITOR
        controls.Menu.Exit.ApplyBindingOverride("<Keyboard>/backquote", path: "<Keyboard>/escape");
#endif

        canvas.enabled = false;
    }

    void OnDestroy()
    {
        controls.Dispose();
    }

    void OnApplicationPause(bool pause)
    {
        // Do nothing when application unpauses or the game is already paused
        if (!pause || IsPaused) return;

        SetPaused(true);
    }

    void HideAdditionalMenus()
    {
        foreach (var menu in additionalMenus)
            menu.enabled = false;
    }

    public void SetPaused(bool pause)
    {
        if (IsPaused == pause) return;

        IsPaused = canvas.enabled = pause;

        if (!pause)
            HideAdditionalMenus();

        Time.timeScale = pause ? 0 : 1;

        Paused?.Invoke(pause);
    }
}
