using SDL2;

namespace JyunrcaeaFramework.Structs;

/// <summary>
/// 창을 생성할때 쓰일 창 옵션입니다.
/// </summary>
public struct WindowOption
{
    internal SDL.SDL_WindowFlags option;

    public WindowOption(bool resize = true, bool borderless = false, bool fullscreen = false, bool hide = true)
    {
        option = SDL.SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI;
        if (resize) option |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
        if (borderless) option |= SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
        if (fullscreen) option |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
        // 보더리스 지원 포기
        //if (fullscreen_desktop) option |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
        if (hide) option |= SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN;
    }
}
