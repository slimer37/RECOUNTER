using Recounter;
using FMODUnity;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] Canvas[] additionalMenus;

    public static event Action<bool> Paused;

    bool cursorVisible;
    CursorLockMode cursorLockState;

    bool pausedByDefocus;

    public static bool IsPaused { get; private set; }

    static Pause instance;

    void Awake()
    {
        instance = this;

        IsPaused = false;

        canvas.enabled = false;
    }

    void OnExit(InputAction.CallbackContext obj)
    {
        SetPaused(!IsPaused);
    }

    public static void SetEnabled(bool enable) => instance.enabled = enable;

    void OnEnable()
    {
        InputLayer.Menu.Exit.performed += OnExit;
    }

    void OnDisable()
    {
        InputLayer.Menu.Exit.performed -= OnExit;
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
