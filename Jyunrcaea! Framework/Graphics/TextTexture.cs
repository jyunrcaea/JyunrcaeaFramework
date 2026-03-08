using SDL2;

namespace JyunrcaeaFramework.Graphics;

public class TextTexture : Texture
{
    public string Text;
    public Font Font;
    public int Size { get; internal set; }
    public Color Color;
    public Color? BackgroundColor;
    public bool Blended = true;
    public uint WarpLength = 0;
    public bool ResourceReady => Font.pointer != IntPtr.Zero;

    public TextTexture(Font font , int size , string text , Color color , Color? backgroundColor = null , bool blended = true)
    {
        this.Font = font;
        this.Size = size;
        this.Color = color;
        this.BackgroundColor = backgroundColor;
        this.Text = text;
        this.Blended = blended;
        Rendering();
        Ready();
    }

    public void ReRender()
    {
        if (this.texture != IntPtr.Zero)
            Dispose();
        Rendering();
    }

    public void Resize(int size)
    {
        if (this.Size == size)
            return;
        if (SDL_ttf.TTF_SetFontSize(Font.pointer , this.Size = size) != 0)
            throw new JyunrcaeaFrameworkException($"?? ??? ????? ???????. (SDL Error: {SDL.SDL_GetError()})");
    }

    SDL.SDL_Surface surface;
    IntPtr buffer;

    internal void Rendering()
    {
        if (this.texture != IntPtr.Zero)
        {
            Dispose();
        }
        if (this.Text == "")
        {
            this.Text = " ";
        }
        if (BackgroundColor is null)
        {
            if (Blended)
            {
                buffer = SDL_ttf.TTF_RenderUTF8_Blended_Wrapped(Font.pointer , Text , Color.colorbase , this.WarpLength);
            }
            else
            {
                buffer = SDL_ttf.TTF_RenderUTF8_Solid_Wrapped(Font.pointer , Text , Color.colorbase , this.WarpLength);
            }
        }
        else
        {
            buffer = SDL_ttf.TTF_RenderUTF8_Shaded_Wrapped(Font.pointer , Text , Color.colorbase , BackgroundColor.colorbase , this.WarpLength);
        }
        if (buffer == IntPtr.Zero)
        {
            throw new JyunrcaeaFrameworkException($"???? ??? ??? ???????. (SDL Error: {SDL.SDL_GetError()})");
        }
        surface = SDL.PtrToStructure<SDL.SDL_Surface>(buffer);
        this.absolutesrc.x = surface.w;
        this.absolutesrc.y = surface.h;
        this.texture = SDL.SDL_CreateTextureFromSurface(Framework.renderer , buffer);
        SDL.SDL_FreeSurface(buffer);
        if (this.texture == IntPtr.Zero)
            throw new JyunrcaeaFrameworkException($"??? ? ???? ???? ????? ???????. {SDL.SDL_GetError()}");
        this.needresettexture = true;
        if (!this.FixedRenderRange)
        {
            this.src.w = this.absolutesrc.x;
            this.src.h = this.absolutesrc.y;
        }
    }

    internal override void Free() {
        base.Free();
    }

    ~TextTexture()
    {
        Dispose();
    }
}
