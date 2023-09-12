using UnityEngine;

namespace Recounter.Art
{
    public abstract class ToolBehaviour : MonoBehaviour, ITool
    {
        public abstract void Draw(float x, float y);

        public abstract void Activate(Texture texture);

        public abstract void Deactivate();
    }
}
