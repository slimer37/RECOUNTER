using UnityEngine;

public class Pause : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] Canvas[] additionalMenus;

    Controls controls;

    bool cursorVisible;
    CursorLockMode cursorLockMode;

    public static bool IsPaused { get; private set; }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        controls = new Controls();
        controls.Menu.Exit.performed += _ => SetPaused(!IsPaused);
        controls.Enable();

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

        if (pause)
        {
            cursorVisible = Cursor.visible;
            cursorLockMode = Cursor.lockState;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = cursorVisible;
            Cursor.lockState = cursorLockMode;
        }
    }
}
