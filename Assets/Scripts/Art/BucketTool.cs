using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Recounter.Art
{
    public class BucketTool : ToolBehaviour
    {
        [SerializeField] ColorPicker _picker;
        [SerializeField] float _tolerance;
        
        struct Node
        {
            public ushort x1, x2, y, dy;

            public Node(int x1, int x2, int y, int dy)
            {
                this.x1 = (ushort)x1;
                this.x2 = (ushort)x2;
                this.y = (ushort)y;
                this.dy = (ushort)dy;
            }
        }
        
        Texture2D _activeTexture2d;
        Texture _activeTexture;
        Color[] _colors;

        bool _active;

        Queue<Node> _nodes;
        
        public override void Draw(float x, float y)
        {
            if (!_active) return;
            
            print($"Bucket fill at {x}, {y}");

            _colors = _activeTexture2d.GetPixels();

            var n = Vector2Int.RoundToInt(new Vector2(x, y));
            
            FloodFill(n, _picker.Color);
            
            _activeTexture2d.SetPixels(_colors);
            _activeTexture2d.Apply();
            
            Graphics.CopyTexture(_activeTexture2d, _activeTexture);
        }

        bool IsInside(int x, int y, Color color)
        {
            if (x < 1 || x > _activeTexture.width || y < 1 || y > _activeTexture.height) return false;
            
            var other = GetNode(x, y);

            if (!Approximately(other.r, color.r)) return false;
            if (!Approximately(other.g, color.g)) return false;
            if (!Approximately(other.b, color.b)) return false;
            if (!Approximately(other.a, color.a)) return false;

            return true;
        }

        bool Approximately(float a, float b) => Mathf.Abs(a - b) < _tolerance;

        Color GetNode(int x, int y) => _colors[(y - 1) * _activeTexture.width + x - 1];

        void SetNode(int x, int y, Color color)
        {
            _colors[(y - 1) * _activeTexture.width + x - 1] = color;
        }

        void FloodFill(Vector2Int source, Color color)
        {
            var x = source.x;
            var y = source.y;
            
            _nodes = new Queue<Node>();
            
            _nodes.Enqueue(new Node(x, x, y, 1));
            _nodes.Enqueue(new Node(x, x, y - 1, -1));

            var matchColor = GetNode(x, y);

            if (IsInside(x, y, color)) return;

            while (_nodes.Count > 0)
            {
                var node = _nodes.Dequeue();

                x = node.x1;

                if (IsInside(x, y, matchColor))
                {
                    while (IsInside(x - 1, y, matchColor))
                    {
                        SetNode(x - 1, y, color);
                        x--;
                    }

                    if (x < node.x1)
                    {
                        _nodes.Enqueue(new Node(x, node.x1 - 1, node.y - node.dy, -node.dy));
                    }
                }

                while (node.x1 <= node.x2)
                {
                    while (IsInside(node.x1, node.y, matchColor))
                    {
                        SetNode(node.x1, node.y, color);
                        node.x1++;
                    }
                    
                    if (node.x1 > x) _nodes.Enqueue(new Node(x, node.x1 - 1, node.y + node.dy, node.dy));
                    
                    if (node.x1 - 1 > node.x2) _nodes.Enqueue(new Node(node.x2 + 1, node.x1 - 1, node.y - node.dy, -node.dy));
                    
                    node.x1++;
                    
                    while (node.x1 < node.x2 && !IsInside(node.x1, node.y, matchColor))
                    {
                        node.x1++;
                    }

                    x = node.x1;
                }
            }
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
    }
}
