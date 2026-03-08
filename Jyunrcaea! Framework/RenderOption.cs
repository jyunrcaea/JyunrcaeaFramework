using SDL2;

namespace JyunrcaeaFramework;

/// <summary>
/// 프레임워크를 초기화 할때 쓰일 렌더러 옵션입니다.
/// </summary>
public struct RenderOption
{
    internal SDL.SDL_RendererFlags option = new();
    public bool anti_alising = true;

    /// <summary>
    /// 렌더링 옵션을 지정한 구조체를 생성합니다.
    /// </summary>
    /// <remarks>Use this constructor to configure rendering options for the renderer. Enabling hardware
    /// acceleration and VSync may improve performance and visual quality, but may not be supported on all
    /// systems.</remarks>
    /// <param name="accelerated">하드웨어 가속을 사용할지에 대한 여부입니다.</param>
    /// <param name="software">소프트웨어 렌더링을 사용할지에 대한 여부입니다.</param>
    /// <param name="vsync">모니터 주사율에 맞춰 수직 동기화를 사용할지에 대한 여부입니다.</param>
    /// <param name="anti_aliasing">안티 엘리어싱을 사용할지에 대한 여부입니다.</param>
    public RenderOption(bool accelerated = true, bool software = true, bool vsync = false, bool anti_aliasing = true)
    {
        if (accelerated) option |= SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED;
        if (software) option |= SDL.SDL_RendererFlags.SDL_RENDERER_SOFTWARE;
        if (vsync) option |= SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC;
        //option |= SDL.SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE;
        this.anti_alising = anti_aliasing;
    }
}