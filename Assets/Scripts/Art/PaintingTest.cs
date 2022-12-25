using UnityEngine;

public class PaintingTest : Interactable
{
    [SerializeField] Renderer rend;

    PlayerController controller;

    public override HudInfo GetHudInfo(Employee e)
    {
        return new()
        {
            icon = Icon.Hand,
            text = "Start Painting"
        };
    }

    protected override void OnInteract(Employee e)
    {
        ArtCreator.BeginSession().Completed += OnCompleteSession;

        controller = e.Controller;
        controller.Suspend(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnCompleteSession(Texture result)
    {
        rend.material.mainTexture = result;

        controller.Suspend(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
