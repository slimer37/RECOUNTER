using FMODUnity;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] Canvas[] additionalMenus;

    Controls controls;

    public static event Action<bool> Paused;

    bool cursorVisible;
    CursorLockMode cursorLockState;

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

        RuntimeManager.PauseAllEvents(pause);

        IsPaused = canvas.enabled = pause;

        if (!pause)
            HideAdditionalMenus();

        Time.timeScale = pause ? 0 : 1;

        if (pause)
            RecordCursor();
        else
            SetCursor();

        Paused?.Invoke(pause);
    }

    void RecordCursor()
    {
        cursorVisible = Cursor.visible;
        cursorLockState = Cursor.lockState;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void SetCursor()
    {
        Cursor.visible = cursorVisible;
        Cursor.lockState = cursorLockState;
    }
}
