using UnityEngine;

public class Pause : MonoBehaviour
{
    [SerializeField] CanvasGroup group;

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

        group.alpha = 0;
        group.blocksRaycasts = false;
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

    public void SetPaused(bool pause)
    {
        if (!IsPaused == pause) return;

        IsPaused = pause;

        Time.timeScale = pause ? 0 : 1;

        group.alpha = pause ? 1 : 0;
        group.blocksRaycasts = pause;

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
