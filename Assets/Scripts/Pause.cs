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

    bool pausedByDefocus;

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
        SetPaused(pause, true);
    }

    void HideAdditionalMenus()
    {
        foreach (var menu in additionalMenus)
            menu.enabled = false;
    }

    public void SetPaused(bool pause) => SetPaused(pause, false);

    public void SetPaused(bool pause, bool applicationPause)
    {
        var validPause = IsPaused != pause;

        if (validPause && applicationPause)
        {
            validPause &= pause || pausedByDefocus;

            pausedByDefocus = pause;
        }

        if (validPause)
        {
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

        // Always call including when pause state does not change;
        // Counteracts auto-unpause when game is re-focused
        RuntimeManager.PauseAllEvents(IsPaused);
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
