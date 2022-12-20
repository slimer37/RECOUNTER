using UnityEngine;

public class Painting
{
    public Painting(Texture texture)
    {
        _texture = texture;
    }

    public bool IsClear { get; set; } = true;

    Texture _texture;

    public Texture Texture => _texture;

    public static Painting Restore(Painting src, Painting dest)
    {
        Graphics.CopyTexture(src._texture, dest._texture);
        dest.IsClear = src.IsClear;
        return dest;
    }
}