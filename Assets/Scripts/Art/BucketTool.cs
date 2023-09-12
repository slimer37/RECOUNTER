using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Recounter.Art
{
    public class BucketTool : ToolBehaviour
    {
        [SerializeField] ColorPicker _picker;
        
        Texture2D _activeTexture2d;
        Texture _activeTexture;

        bool _active;
        
        public override void Draw(float x, float y)
        {
            if (!_active) return;
            
            print($"Bucket fill at {x}, {y}");

            var n = Vector2Int.RoundToInt(new Vector2(x, y));

            FloodFillArea(_activeTexture2d, n.x, n.y, _picker.Color);
            
            _activeTexture2d.Apply();
            
            Graphics.CopyTexture(_activeTexture2d, _activeTexture);
        }

        public override void Activate(Texture texture)
        {
            _activeTexture = texture;
            _activeTexture2d = new Texture2D(texture.width, texture.height, texture.graphicsFormat, 0, TextureCreationFlags.DontInitializePixels);

            var currentRt = RenderTexture.active;

            var rt = new RenderTexture(texture.width, texture.height, texture.graphicsFormat, GraphicsFormat.None, 0);
            
            Graphics.Blit(texture, rt);

            RenderTexture.active = rt;
            
            _activeTexture2d.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            
            _activeTexture2d.Apply();

            RenderTexture.active = currentRt;
            
            _active = true;
        }

        public override void Deactivate()
        {
            _active = false;
        }
        
        // https://web.archive.org/web/20180122054652/http://wiki.unity3d.com/index.php?title=TextureFloodFill
        // via https://discussions.unity.com/t/flood-fill-algorithm-for-colour-fill-paint-bucket-tool/37555/2
        struct Point
        {
            public short x;
            public short y;
            public Point(short aX, short aY) { x = aX; y = aY; }
            public Point(int aX, int aY) : this((short)aX, (short)aY) { }
        }
 
        static void FloodFillArea(Texture2D aTex, int aX, int aY, Color aFillColor)
        {
            int w = aTex.width;
            int h = aTex.height;
            Color[] colors = aTex.GetPixels();
            Color refCol = colors[aX + aY * w];
            Queue<Point> nodes = new Queue<Point>();
            nodes.Enqueue(new Point(aX, aY));
            while (nodes.Count > 0)
            {
                Point current = nodes.Dequeue();
                for (int i = current.x; i < w; i++)
                {
                    Color C = colors[i + current.y * w];
                    if (C != refCol || C == aFillColor)
                        break;
                    colors[i + current.y * w] = aFillColor;
                    if (current.y + 1 < h)
                    {
                        C = colors[i + current.y * w + w];
                        if (C == refCol && C != aFillColor)
                            nodes.Enqueue(new Point(i, current.y + 1));
                    }
                    if (current.y - 1 >= 0)
                    {
                        C = colors[i + current.y * w - w];
                        if (C == refCol && C != aFillColor)
                            nodes.Enqueue(new Point(i, current.y - 1));
                    }
                }
                for (int i = current.x - 1; i >= 0; i--)
                {
                    Color C = colors[i + current.y * w];
                    if (C != refCol || C == aFillColor)
                        break;
                    colors[i + current.y * w] = aFillColor;
                    if (current.y + 1 < h)
                    {
                        C = colors[i + current.y * w + w];
                        if (C == refCol && C != aFillColor)
                            nodes.Enqueue(new Point(i, current.y + 1));
                    }
                    if (current.y - 1 >= 0)
                    {
                        C = colors[i + current.y * w - w];
                        if (C == refCol && C != aFillColor)
                            nodes.Enqueue(new Point(i, current.y - 1));
                    }
                }
            }
            aTex.SetPixels(colors);
        }
    }
}
