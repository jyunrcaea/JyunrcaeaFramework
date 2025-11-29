using SDL2;

namespace JyunrcaeaFramework;

/// <summary>
/// 프레임워크를 초기화 할때 쓰일 렌더러 옵션입니다.
/// </summary>
public struct RenderOption
{
    internal SDL.SDL_RendererFlags option = new();
    public bool anti_alising = true;

    public RenderOption(bool sccelerated = true, bool software = true, bool vsync = false, bool anti_aliasing = true)
    {
        if (sccelerated) option |= SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED;
        if (software) option |= SDL.SDL_RendererFlags.SDL_RENDERER_SOFTWARE;
        if (vsync) option |= SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC;
        //option |= SDL.SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE;
        this.anti_alising = anti_aliasing;
    }
}