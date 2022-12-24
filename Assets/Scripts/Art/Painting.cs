using UnityEngine;

public class Painting
{
    public Painting(Texture texture, Color backgroundColor)
    {
        BackgroundColor = backgroundColor;
        _texture = texture;
    }

    public bool IsClear { get; set; } = true;

    public readonly Color BackgroundColor;
    Texture _texture;

    public Texture Texture => _texture;

    public static Painting Restore(Painting src, Painting dest)
    {
        Graphics.CopyTexture(src._texture, dest._texture);
        dest.IsClear = src.IsClear;
        return dest;
    }
}