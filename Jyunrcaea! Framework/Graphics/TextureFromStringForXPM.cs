using JyunrcaeaFramework.Core;
using SDL2;

namespace JyunrcaeaFramework.Graphics;

/// <summary>
/// Texture loaded from XPM text data stored in memory.
/// </summary>
public class TextureFromStringForXPM : Texture
{
    /// <summary>
    /// Raw XPM lines used to create this texture.
    /// </summary>
    public string[] StringForXPM = null!;

    /// <summary>
    /// Creates a texture from an in-memory XPM string array.
    /// </summary>
    public TextureFromStringForXPM(string[] xpmdata)
    {
        StringForXPM = xpmdata;

        IntPtr surface = SDL_image.IMG_ReadXPMFromArray(StringForXPM);
        if (surface == IntPtr.Zero)
        {
            throw new JyunrcaeaFrameworkException($"Failed to load XPM string data. SDL Error: {SDL.SDL_GetError()}");
        }

        texture = SDL.SDL_CreateTextureFromSurface(Framework.renderer, surface);
        SDL.SDL_FreeSurface(surface);

        if (texture == IntPtr.Zero)
        {
            throw new JyunrcaeaFrameworkException($"Failed to create texture from XPM. SDL Error: {SDL.SDL_GetError()}");
        }

        needresettexture = true;
        SDL.SDL_QueryTexture(texture, out _, out _, out absolutesrc.x, out absolutesrc.y);
        if (!FixedRenderRange)
        {
            src.w = absolutesrc.x;
            src.h = absolutesrc.y;
        }

        Ready();
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
