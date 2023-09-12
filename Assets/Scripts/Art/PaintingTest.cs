using UnityEngine;

namespace Recounter.Art
{
    public class PaintingTest : Interactable
    {
        [SerializeField] Renderer rend;

        Texture currentPainting;

        protected override HudInfo FormHud(Employee e) => new()
        {
            icon = Icon.Hand,
            text = "Start Painting"
        };

        protected override bool CanInteract(Employee e) => !ArtCreator.SessionInProgress;

        protected override void OnInteract(Employee e)
        {
            ArtCreator.BeginSession(currentPainting).Completed += OnCompleteSession;

            InputLayer.Suspend(true, true);
        }

        void OnCompleteSession(Texture result)
        {
            currentPainting = result;
            rend.material.mainTexture = result;

            InputLayer.Suspend(false);
        }
    }
}
