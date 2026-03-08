using SDL2;

namespace JyunrcaeaFramework.Graphics;

public class TextureFromTextFileForXPM : Texture
{
    public TextureFromTextFileForXPM(string FilePath)
    {
        this.FilePath = FilePath;
        string[] data = File.ReadAllLines(FilePath);
        IntPtr surface = SDL_image.IMG_ReadXPMFromArray(data);
        if (surface == IntPtr.Zero)
            throw new JyunrcaeaFrameworkException($"불러올수 없는 XPM 문자열 SDL Error: {SDL.SDL_GetError()}");
        this.texture = SDL.SDL_CreateTextureFromSurface(Framework.renderer , surface);
        SDL.SDL_FreeSurface(surface);
        if (this.texture == IntPtr.Zero)
            throw new JyunrcaeaFrameworkException($"텍스쳐로 변환 실패함 SDL Error: {SDL.SDL_GetError()}");
        Ready();
    }

    public string FilePath = string.Empty;

    public override void Dispose()
    {
        SDL.SDL_DestroyTexture(this.texture);
    }
}
