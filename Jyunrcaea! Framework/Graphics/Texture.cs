using SDL2;

namespace JyunrcaeaFramework.Graphics;

/// <summary>
/// ??? ??? ?? ??????.
/// </summary>
public class Texture : IDisposable
{
    internal Texture() { }
    internal Texture(IntPtr pointer)
    {
        this.texture = SDL.SDL_CreateTextureFromSurface(Framework.renderer , pointer);
        if (this.texture == IntPtr.Zero)
        {
            throw new JyunrcaeaFrameworkException("???? ????? ???????.");
        }
        SDL.SDL_QueryTexture(this.texture , out _ , out _ , out this.absolutesrc.x , out this.absolutesrc.y);
        this.needresettexture = true;
        if (!this.FixedRenderRange)
        {
            this.src.w = this.absolutesrc.x;
            this.src.h = this.absolutesrc.y;
        }
        Ready();
    }

    public Texture(string filename)
    {
        if ((this.texture = SDL_image.IMG_LoadTexture(Framework.renderer , filename)) == IntPtr.Zero)
            throw new JyunrcaeaFrameworkException("SDL image Error: " + SDL.SDL_GetError());
        SDL.SDL_QueryTexture(this.texture , out _ , out _ , out this.absolutesrc.x , out this.absolutesrc.y);
        this.needresettexture = true;
        if (!this.FixedRenderRange)
        {
            this.src.w = this.absolutesrc.x;
            this.src.h = this.absolutesrc.y;
        }
        Ready();
    }

    public bool FixedRenderRange = false;

    internal bool needresettexture = false;

    internal IntPtr texture;

    internal SDL.SDL_Rect src = new();

    internal SDL.SDL_Point absolutesrc = new();

    public int Width => absolutesrc.x;

    public int Height => absolutesrc.y;

    /// <summary>
    /// ?? ??? ??? ?? ???? ???? ?? ?????.
    /// RenderRange? null ??? ?? ?? ?? ? ??? false? ???.
    /// </summary>
    public bool AutoRange { get; internal set; } = true;

    internal byte alpha = 255;

    public byte Opacity {
        get {
            return alpha;
        }
        set {
            alpha = value;
            if (this.texture != IntPtr.Zero)
                SDL.SDL_SetTextureAlphaMod(texture , alpha);
        }
    }

    public void SetRenderRange(int x , int y , int width , int height)
    {
        src.x = x;
        src.y = y;
        src.w = width;
        src.h = height;
        needresettexture = true;
    }

    public RectSize? RenderRange {
        get { if (AutoRange) return null; return new(src.x , src.y , src.w , src.h); }
        set {
            needresettexture = true;
            if (value == null)
            {
                AutoRange = true;
                src.x = src.y = 0;
                src.w = absolutesrc.x;
                src.h = absolutesrc.y;
                return;
            }
            AutoRange = false;
            src = value.size;
        }
    }

    internal void Ready()
    {
        if (this.texture == IntPtr.Zero)
            return;
        if (SDL.SDL_SetTextureBlendMode(this.texture , SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND) < 0)
            throw new JyunrcaeaFrameworkException($"???? ??? ?? ??? ???????. SDL Error: {SDL.SDL_GetError()}");
        if (alpha != 255)
            SDL.SDL_SetTextureAlphaMod(texture , alpha);
    }

    internal virtual void Free() {
        SDL.SDL_DestroyTexture(this.texture);
        this.absolutesrc.x = this.absolutesrc.y = 0;
        this.needresettexture = true;
        this.texture = IntPtr.Zero;
    }

    public virtual void Dispose()
    {
        if (this.texture != IntPtr.Zero) Free();
        GC.SuppressFinalize(this);
    }

    ~Texture()
    {
        Dispose();
    }
}
