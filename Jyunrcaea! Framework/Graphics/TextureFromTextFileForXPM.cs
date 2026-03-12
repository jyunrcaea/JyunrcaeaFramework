using JyunrcaeaFramework.Core;
using SDL2;

namespace JyunrcaeaFramework.Graphics;

/// <summary>
/// Texture loaded from a text file containing XPM data.
/// </summary>
public class TextureFromTextFileForXPM : Texture
{
    /// <summary>
    /// Path of the XPM source file.
    /// </summary>
    public string FilePath = string.Empty;

    /// <summary>
    /// Creates a texture by reading an XPM text file from disk.
    /// </summary>
    public TextureFromTextFileForXPM(string filePath)
    {
        FilePath = filePath;

        string[] data = File.ReadAllLines(filePath);
        IntPtr surface = SDL_image.IMG_ReadXPMFromArray(data);
        if (surface == IntPtr.Zero)
        {
            throw new JyunrcaeaFrameworkException($"Failed to load XPM text file. SDL Error: {SDL.SDL_GetError()}");
        }

        texture = SDL.SDL_CreateTextureFromSurface(Framework.renderer, surface);
        SDL.SDL_FreeSurface(surface);

        if (texture == IntPtr.Zero)
        {
            throw new JyunrcaeaFrameworkException($"Failed to create texture from XPM file. SDL Error: {SDL.SDL_GetError()}");
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
