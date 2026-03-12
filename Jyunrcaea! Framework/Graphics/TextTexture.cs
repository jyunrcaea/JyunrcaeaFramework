using JyunrcaeaFramework.Core;
using SDL2;

namespace JyunrcaeaFramework.Graphics;

/// <summary>
/// Texture generated from UTF-8 text using SDL_ttf.
/// </summary>
public class TextTexture : Texture
{
    /// <summary>
    /// Current text content used for rendering.
    /// </summary>
    public string Text;

    /// <summary>
    /// Font resource used by this text texture.
    /// </summary>
    public Font Font;

    public int Size { get; internal set; }
    public Color Color;
    public Color? BackgroundColor;
    public bool Blended = true;
    public uint WarpLength = 0;

    /// <summary>
    /// Returns whether the underlying font handle is still valid.
    /// </summary>
    public bool ResourceReady => Font.pointer != IntPtr.Zero;

    /// <summary>
    /// True when this instance is responsible for disposing <see cref="Font"/>.
    /// </summary>
    public bool OwnsFont { get; }

    /// <summary>
    /// Creates a text texture from font, color, and text options.
    /// </summary>
    public TextTexture(Font font, int size, string text, Color color, Color? backgroundColor = null, bool blended = true, bool ownsFont = false)
    {
        Font = font;
        Size = size;
        Color = color;
        BackgroundColor = backgroundColor;
        Text = text;
        Blended = blended;
        OwnsFont = ownsFont;
        Rendering();
    }

    /// <summary>
    /// Recreates the SDL texture from the current text options.
    /// </summary>
    public void ReRender()
    {
        if (texture != IntPtr.Zero)
        {
            Free();
        }

        Rendering();
    }

    /// <summary>
    /// Changes font size without automatically re-rendering text content.
    /// </summary>
    public void Resize(int size)
    {
        if (Size == size)
        {
            return;
        }

        if (SDL_ttf.TTF_SetFontSize(Font.pointer, Size = size) != 0)
        {
            throw new JyunrcaeaFrameworkException($"SDL_ttf Error: {SDL_ttf.TTF_GetError()}");
        }
    }

    IntPtr buffer;

    void Rendering()
    {
        string renderText = string.IsNullOrEmpty(Text) ? " " : Text;

        if (BackgroundColor is null)
        {
            buffer = Blended
                ? SDL_ttf.TTF_RenderUTF8_Blended_Wrapped(Font.pointer, renderText, Color.colorbase, WarpLength)
                : SDL_ttf.TTF_RenderUTF8_Solid_Wrapped(Font.pointer, renderText, Color.colorbase, WarpLength);
        }
        else
        {
            buffer = SDL_ttf.TTF_RenderUTF8_Shaded_Wrapped(Font.pointer, renderText, Color.colorbase, BackgroundColor.colorbase, WarpLength);
        }

        if (buffer == IntPtr.Zero)
        {
            throw new JyunrcaeaFrameworkException($"SDL_ttf text rendering failed. SDL_ttf Error: {SDL_ttf.TTF_GetError()}");
        }

        var surface = SDL.PtrToStructure<SDL.SDL_Surface>(buffer);
        absolutesrc.x = surface.w;
        absolutesrc.y = surface.h;

        texture = SDL.SDL_CreateTextureFromSurface(Framework.renderer, buffer);
        SDL.SDL_FreeSurface(buffer);

        if (texture == IntPtr.Zero)
        {
            throw new JyunrcaeaFrameworkException($"Failed to create text texture. SDL Error: {SDL.SDL_GetError()}");
        }

        needresettexture = true;
        if (!FixedRenderRange)
        {
            src.w = absolutesrc.x;
            src.h = absolutesrc.y;
        }

        Ready();
    }

    /// <summary>
    /// Destroys texture and optionally disposes the font owned by this instance.
    /// </summary>
    public override void Dispose()
    {
        base.Dispose();

        if (OwnsFont)
        {
            Font.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    ~TextTexture()
    {
        Dispose();
    }
}
