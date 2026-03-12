using JyunrcaeaFramework.Graphics;
using SDL2;

namespace JyunrcaeaFramework.Core;

/// <summary>
/// 창과 관련된 기능을 다룹니다.
/// </summary>
public static class Window {
    internal static SDL.SDL_Rect size = new();
    internal static SDL.SDL_Point position = new();
    internal static SDL.SDL_Point default_size = new();
    /// <summary>
    /// 기본 가로값
    /// </summary>
    public static int DefaultWidth => default_size.x;
    /// <summary>
    /// 기본 세로값
    /// </summary>
    public static int DefaultHeight => default_size.y;
    /// <summary>
    /// 창을 조절해도 적당한 위치에 있을수 있도록 제공하는 적정 사이즈입니다.
    /// </summary>
    public static float AppropriateSize { get; internal set; } = 1;

    /// <summary>
    /// 창의 배경색을 설정합니다.
    /// </summary>
    public static Color BackgroundColor = new(31, 30, 51);

    internal static float wh = 0, hh = 0;
    /// <summary>
    /// 창의 수평 위치
    /// </summary>
    public static int X => position.x;
    /// <summary>
    /// 창의 수직 위치
    /// </summary>
    public static int Y => position.y;

    public static uint UWidth => (uint)size.w;
    public static uint UHeight => (uint)size.h;
    /// <summary>
    /// 창의 너비
    /// </summary>
    public static int Width => size.w;
    /// <summary>
    /// 창의 높이
    /// </summary>
    public static int Height => size.h;

    /// <summary>
    /// 다른 창에 가려져도 이 창에 초첨이 맞춰집니다. 잘 작동하진 않습니다.
    /// 창을 맨 위로 올리고 초첨을 맞출려면 Raise() 를 사용하세요.
    /// </summary>
    /// <returns></returns>
    public static bool InputFocus() => SDL.SDL_SetWindowInputFocus(Framework.window) == 0;

    internal static bool fullscreenoption = false;
    /// <summary>
    /// 전체화면 여부
    /// </summary>
    public static bool Fullscreen
    {
        get => fullscreenoption; set
        {
            SDL.SDL_SetWindowFullscreen(Framework.window, (fullscreenoption = value) ? 4097u : 0u);
            if (!value) { Framework.Function.Resize(); Framework.Function.Resized(); }
        }
    }

    public static string Title
    {
        get => SDL.SDL_GetWindowTitle(Framework.window);
        set => SDL.SDL_SetWindowTitle(Framework.window, value);
    }



    internal static int beforewidth=0, beforeheight=0;

    /// <summary>
    /// 화면을 창으로 가득 채우는 기능입니다. 가짜 전체화면으로 불리기도 합니다. (창 테두리를 없애고 창 크기를 모니터 크기에 맞춥니다.)
    /// (안정적인 기능은 아닙니다.)
    /// </summary>
    public static bool DesktopFullscreen
    {
        set
        {
            if (value)
            {
                Borderless = true;
                //SDL.SDL_SetWindowSize(Framework.window, Display.MonitorWidth, Display.MonitorHeight);
                beforewidth = Window.Width;
                beforeheight = Window.Height;
                Window.Resize(Display.MonitorWidth, Display.MonitorHeight);
                Window.Move();
            } else
            {
                Borderless = false;
                Window.Resize(beforewidth, beforeheight);
                Window.Move();
            }
        }
    }

    internal static bool windowborderless = false;
    /// <summary>
    /// 창의 테두리 제거여부
    /// </summary>
    public static bool Borderless
    {
        get => windowborderless;
        set => SDL.SDL_SetWindowBordered(Framework.window, (windowborderless = value) ? SDL.SDL_bool.SDL_FALSE : SDL.SDL_bool.SDL_TRUE);
    }

    /// <summary>
    /// 창을 맨 앞으로 올리고 사용자가 조작할 대상을 이 창으로 설정합니다.
    /// </summary>
    public static void Raise() => SDL.SDL_RaiseWindow(Framework.window);

    /// <summary>
    /// 최소화 또는 최대화 된 창의 크기와 위치를 원래대로 돌려놓습니다.
    /// </summary>
    public static void Restore() => SDL.SDL_RestoreWindow(Framework.window);

    /// <summary>
    /// 창의 투명도를 설정합니다. 0.0f이 완전히 투명이며 1.0f이 완전 불투명입니다.
    /// 투명도를 지원하지 않는 운영체제에서는 get은 1.0f을 반환합니다.
    /// </summary>
    public static float Opacity
    {
        get { SDL.SDL_GetWindowOpacity(Framework.window, out var op); return op; }
        set => SDL.SDL_SetWindowOpacity(Framework.window, value);
    }
    /// <summary>
    /// 창 표시여부
    /// </summary>
    public static bool Show { set { if (value) SDL.SDL_ShowWindow(Framework.window); else SDL.SDL_HideWindow(Framework.window); } }
    /// <summary>
    /// 창의 아이콘을 설정합니다.
    /// </summary>
    /// <param name="filename">파일명</param>
    /// <exception cref="JyunrcaeaFrameworkException">파일을 불러올수 없을때</exception>
    public static void Icon(string filename)
    {
        IntPtr surface = SDL_image.IMG_Load(filename);
        if (surface == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"파일을 불러올수 없습니다. (SDL image Error: {SDL_image.IMG_GetError()})");
        SDL.SDL_SetWindowIcon(Framework.window, surface);
        SDL.SDL_FreeSurface(surface);
    }
    /// <summary>
    /// 창의 크기를 조절합니다.
    /// </summary>
    /// <param name="width">너비</param>
    /// <param name="height">높이</param>
    public static void Resize(int width, int height)
    {
        SDL.SDL_SetWindowSize(Framework.window, width, height);
        Window.size.w = width;
        Window.size.h = height;
        SDL.SDL_Event e = new()
        {
            type = SDL.SDL_EventType.SDL_WINDOWEVENT,
            window = new()
            {
                type = SDL.SDL_EventType.SDL_WINDOWEVENT,
                windowEvent = SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED,
                data1 = width,
                data2 = height,
                windowID = SDL.SDL_GetWindowID(Framework.window),
                timestamp = SDL.SDL_GetTicks()
            }
        };
        SDL.SDL_PushEvent(ref e);
        e.window.windowEvent = SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED;
        SDL.SDL_PushEvent(ref e);
    }

    /// <summary>
    /// 창의 위치를 이동합니다.
    /// </summary>
    /// <param name="x">수평 위치</param>
    /// <param name="y">수직 위치</param>
    public static void Move(int? x = null, int? y = null)
    {
        SDL.SDL_SetWindowPosition(Framework.window, x ?? SDL.SDL_WINDOWPOS_CENTERED, y ?? SDL.SDL_WINDOWPOS_CENTERED);
        SDL.SDL_GetWindowPosition(Framework.window, out Window.position.x, out Window.position.y);
        Framework.Function.WindowMove();
    }
    /// <summary>
    /// 최대 창 크기를 지정합니다.
    /// </summary>
    /// <param name="Width">너비</param>
    /// <param name="Height">높이</param>
    public static void SetMaximizeSize(int Width,int Height)
    {
        SDL.SDL_SetWindowMaximumSize(Framework.window, Width, Height);
    }
    /// <summary>
    /// 최소 창 크기를 지정합니다.
    /// </summary>
    /// <param name="Width">너비</param>
    /// <param name="Height">높이</param>
    public static void SetMinimizeSize(int Width,int Height)
    {
        SDL.SDL_SetWindowMinimumSize(Framework.window, Width, Height);
    }
    /// <summary>
    /// 지정한 최대 창 크기를 얻습니다.
    /// </summary>
    /// <param name="Width">너비</param>
    /// <param name="Height">높이</param>
    public static void GetMaximizeSize(out int Width,out int Height) => SDL.SDL_GetWindowMaximumSize(Framework.window,out Width,out Height);
    /// <summary>
    /// 지정한 최소 창 크기를 얻습니다.
    /// </summary>
    /// <param name="Width">너비</param>
    /// <param name="Height">높이</param>
    public static void GetMinimizeSize(out int Width,out int Height) => SDL.SDL_GetWindowMinimumSize(Framework.window,out Width,out Height);
    /// <summary>
    /// 창을 최대화 합니다.
    /// </summary>
    public static void Maximize() => SDL.SDL_MaximizeWindow(Framework.window);
    /// <summary>
    /// 창을 최소화 합니다.
    /// </summary>
    public static void Minimize() => SDL.SDL_MinimizeWindow(Framework.window);
    /// <summary>
    /// 창을 항상 맨위에 올릴지에 대한 여부를 지정합니다.
    /// </summary>
    public static bool AlwaysOnTop
    {
        set => SDL.SDL_SetWindowAlwaysOnTop(Framework.window, value ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE);
    }

    /// <summary>
    /// SDL2에서 사용할수 있는 창의 ID 값을 얻습니다. 
    /// 정식 버전에서 삭제될 기능입니다.
    /// </summary>
    public static uint ID => SDL.SDL_GetWindowID(Framework.window);

    /// <summary>
    /// 창을 닫을때 프레임워크를 중지할지에 대한 여부입니다.
    /// </summary>
    public static bool FrameworkStopWhenClose = true;

    /// <summary>
    /// Alt + F4 키로 창을 종료하는걸 막을지에 대한 여부입니다.
    /// </summary>
    public static bool BlockAltF4
    {
        set
        {
            SDL.SDL_SetHint(SDL.SDL_HINT_WINDOWS_NO_CLOSE_ON_ALT_F4, value ? "1" : "0");
        }
        get => SDL.SDL_GetHint(SDL.SDL_HINT_WINDOWS_NO_CLOSE_ON_ALT_F4) == "1";
    }

    public static bool Resizable
    {
        set {
            SDL.SDL_SetWindowResizable(Framework.window,
                value ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE
            );
        }
    }
}
