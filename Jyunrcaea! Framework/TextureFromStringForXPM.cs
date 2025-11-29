using SDL2;

namespace JyunrcaeaFramework;

public class TextureFromStringForXPM : Texture
{
    public TextureFromStringForXPM(string[] xpmdata)
    {
        this.StringForXPM = xpmdata;
        IntPtr surface = SDL_image.IMG_ReadXPMFromArray(StringForXPM);
        if (surface == IntPtr.Zero)
            throw new JyunrcaeaFrameworkException($"불러올수 없는 XPM 문자열 SDL Error: {SDL.SDL_GetError()}");
        this.texture = SDL.SDL_CreateTextureFromSurface(Framework.renderer , surface);
        SDL.SDL_FreeSurface(surface);
        if (this.texture == IntPtr.Zero)
            throw new JyunrcaeaFrameworkException($"텍스쳐로 변환 실패함 SDL Error: {SDL.SDL_GetError()}");
        this.needresettexture = true;
        SDL.SDL_QueryTexture(this.texture , out _ , out _ , out this.absolutesrc.x , out this.absolutesrc.y);
        if (!this.FixedRenderRange)
        {
            this.src.w = this.absolutesrc.x;
            this.src.h = this.absolutesrc.y;
        }
        Ready();
    }

    public string[] StringForXPM = null!;

    public override void Dispose()
    {
        SDL.SDL_DestroyTexture(this.texture);
        this.texture = IntPtr.Zero;
    }
}