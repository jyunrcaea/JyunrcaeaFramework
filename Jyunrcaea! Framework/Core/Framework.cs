using System.Reflection;
using JyunrcaeaFramework.Audio;
using JyunrcaeaFramework.Collections;
using JyunrcaeaFramework.Structs;
using SDL2;

namespace JyunrcaeaFramework.Core;

public static class Framework
{
    /// <summary>
    /// 프레임이 예정보다 빠를 때 바쁜 대기를 줄입니다.
    /// CPU 사용량은 낮추지만 약간의 프레임 시간 지터가 발생할 수 있습니다.
    /// </summary>
    public static bool SavingPerformance { get; set; } = true;

    /// <summary>
    /// 프레임워크의 시멘틱 버전입니다.
    /// </summary>
    public static readonly Version Version = new(0, 7, 1);

    /// <summary>
    /// 프레임워크가 라이프사이클 및 입력 이벤트에 사용하는 콜백 구현입니다.
    /// 동작을 사용자 정의하려면 이 인스턴스를 교체하세요.
    /// </summary>
    public static FrameworkFunction Function = new();

    /// <summary>
    /// SDL 윈도우 핸들입니다.
    /// </summary>
    internal static IntPtr window = IntPtr.Zero;

    /// <summary>
    /// SDL 렌더러 핸들입니다.
    /// </summary>
    internal static IntPtr renderer = IntPtr.Zero;

    /// <summary>
    /// 프레임워크를 초기화하고 지정된 구성 옵션으로 메인 애플리케이션 윈도우를 생성합니다.
    /// </summary>
    /// <remarks>이 메서드는 윈도우, 렌더링 또는 오디오 기능에 의존하는 모든 프레임워크 기능을 사용하기 전에 반드시 호출되어야 합니다.
    /// 초기화에 실패하면 성공적으로 다시 초기화될 때까지 프레임워크를 사용할 수 없습니다. 스레드 안전성은 보장되지 않으므로 메인 스레드에서 이 메서드를 호출하세요.</remarks>
    /// <param name="title">윈도우 제목 표시줄에 표시할 윈도우 제목입니다.</param>
    /// <param name="width">윈도우의 너비(픽셀 단위)입니다. 0보다 커야 합니다.</param>
    /// <param name="height">윈도우의 높이(픽셀 단위)입니다. 0보다 커야 합니다.</param>
    /// <param name="x">화면상 윈도우의 수평 위치(픽셀 단위)입니다. null인 경우 윈도우는 수평으로 중앙 정렬됩니다.</param>
    /// <param name="y">화면상 윈도우의 수직 위치(픽셀 단위)입니다. null인 경우 윈도우는 수직으로 중앙 정렬됩니다.</param>
    /// <param name="option">윈도우 구성 옵션입니다. null인 경우 기본 옵션을 사용합니다.</param>
    /// <param name="renderOption">렌더러 구성 옵션입니다. null인 경우 기본 옵션을 사용합니다.</param>
    /// <param name="audioOption">오디오 구성 옵션입니다. 채널 수는 8을 초과할 수 없습니다.</param>
    /// <param name="keepRenderingWhenResize">윈도우를 리사이징하는 동안 렌더링을 계속할지 여부를 나타냅니다. <see langword="true"/>로 설정하면
    /// 리사이징 중에 지속적인 렌더링을 활성화합니다.</param>
    /// <exception cref="JyunrcaeaFrameworkException">지원하지 않는 오디오 채널 수, SDL2 또는 관련 라이브러리 오류, 윈도우 또는 렌더러 생성 실패로 인해
    /// 초기화에 실패하면 발생합니다.</exception>
    public static void Init(string title, uint width, uint height, int? x = null, int? y = null, WindowOption? option = null, RenderOption? renderOption = null, AudioOption audioOption = default, bool keepRenderingWhenResize = true)
    {
        if (audioOption.ch > 8) {
            throw new JyunrcaeaFrameworkException("Unsupported audio channel count. (AudioOption.Channls > 8)");
        }

        if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) != 0) {
            throw new JyunrcaeaFrameworkException($"Failed to initialize SDL2. SDL Error: {SDL.SDL_GetError()}");
        }

        SDL.SDL_WindowFlags winflg = option is null
            ? SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL
            : option.Value.option;

        window = SDL.SDL_CreateWindow(title, x ?? SDL.SDL_WINDOWPOS_CENTERED, y ?? SDL.SDL_WINDOWPOS_CENTERED, (int)width, (int)height, winflg);
        if (window == IntPtr.Zero) {
            ShutdownSdl();
            throw new JyunrcaeaFrameworkException($"Failed to create window. SDL Error: {SDL.SDL_GetError()}");
        }

        renderer = SDL.SDL_CreateRenderer(
            window,
            -1,
            renderOption is null ? SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED : renderOption.Value.option
        );

        if (renderer == IntPtr.Zero) {
            ShutdownSdl();
            throw new JyunrcaeaFrameworkException($"Failed to create renderer. SDL Error: {SDL.SDL_GetError()}");
        }

        if (renderOption is null || renderOption.Value.anti_alising) {
            if (SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "2") == 0) {
                SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "1");
            }
        }

        if (SDL.SDL_SetRenderDrawBlendMode(renderer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND) != 0) {
            ShutdownSdl();
            throw new JyunrcaeaFrameworkException($"Failed to set renderer blend mode. SDL Error: {SDL.SDL_GetError()}");
        }

        if (SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_JPG | SDL_image.IMG_InitFlags.IMG_INIT_WEBP | SDL_image.IMG_InitFlags.IMG_INIT_TIF | SDL_image.IMG_InitFlags.IMG_INIT_PNG) == 0) {
            ShutdownSdl();
            throw new JyunrcaeaFrameworkException($"Failed to initialize SDL2_image. SDL image Error: {SDL_image.IMG_GetError()}");
        }

        if (SDL_ttf.TTF_Init() == -1) {
            ShutdownSdl();
            throw new JyunrcaeaFrameworkException($"Failed to initialize SDL2_ttf. SDL ttf Error: {SDL_ttf.TTF_GetError()}");
        }

        if (SDL_mixer.Mix_Init(SDL_mixer.MIX_InitFlags.MIX_INIT_MP3 | SDL_mixer.MIX_InitFlags.MIX_INIT_OGG | SDL_mixer.MIX_InitFlags.MIX_INIT_MID | SDL_mixer.MIX_InitFlags.MIX_INIT_FLAC | SDL_mixer.MIX_InitFlags.MIX_INIT_OPUS | SDL_mixer.MIX_InitFlags.MIX_INIT_MOD) == 0) {
            ShutdownSdl();
            throw new JyunrcaeaFrameworkException($"Failed to initialize SDL2_mixer. SDL mixer Error: {SDL_mixer.Mix_GetError()}");
        }

        bool setting = true;
        while (setting) {
            if (SDL_mixer.Mix_OpenAudio(audioOption.hz, SDL_mixer.MIX_DEFAULT_FORMAT, audioOption.ch, audioOption.cs) != 0) {
                if (!audioOption.trylow || audioOption.ch == 1) {
                    ShutdownSdl();
                    throw new JyunrcaeaFrameworkException($"Failed to open audio device. SDL mixer Error: {SDL_mixer.Mix_GetError()}");
                }

                audioOption.ch = (audioOption.ch == 7) ? 6 : audioOption.ch - 1;
            }
            else {
                setting = false;
            }
        }

        if (IntPtr.Size != 4 && (Display.KeepRenderingWhenResize = keepRenderingWhenResize)) {
            SDL.SDL_SetEventFilter((_, eventPtr) => {
                var e = (SDL.SDL_Event)System.Runtime.InteropServices.Marshal.PtrToStructure(eventPtr, typeof(SDL.SDL_Event))!;

                if ((e.type == SDL.SDL_EventType.SDL_KEYDOWN || e.type == SDL.SDL_EventType.SDL_KEYUP) && e.key.repeat != 0) {
                    return Input.Text.ti ? 1 : 0;
                }

                if (e.type != SDL.SDL_EventType.SDL_WINDOWEVENT) {
                    return 1;
                }

                switch (e.window.windowEvent) {
                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                    Framework.Function.WindowQuit();
                    return 0;

                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                    Window.size.w = e.window.data1;
                    Window.size.h = e.window.data2;
                    Window.wh = Window.size.w * 0.5f;
                    Window.hh = Window.size.h * 0.5f;
                    Framework.Function.Resize();
                    Framework.Function.Draw();
                    return 0;

                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                    return 1;

                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MOVED:
                    Window.position.x = e.window.data1;
                    Window.position.y = e.window.data2;
                    Framework.Function.WindowMove();
                    Framework.Function.Draw();
                    return 1;

                    default:
                    return 1;
                }
            }, IntPtr.Zero);
        }

        SDL_mixer.Mix_HookMusicFinished(Music.Finished);

        if ((winflg & SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP) == SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP) {
            Window.fullscreenoption = true;
        }

        if ((winflg & SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS) == SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS) {
            Window.windowborderless = true;
        }

        Window.size = new() { w = Window.default_size.x = Window.beforewidth = (int)width, h = Window.default_size.y = Window.beforeheight = (int)height };
        Window.wh = width * 0.5f;
        Window.hh = height * 0.5f;

        if (SDL.SDL_GetDisplayMode(0, 0, out Display.dm) < 0) {
            ShutdownSdl();
            throw new JyunrcaeaFrameworkException("Failed to read display mode information.");
        }

        SDL.SDL_SetHint(SDL.SDL_HINT_VIDEO_MINIMIZE_ON_FOCUS_LOSS, "1");
        SDL.SDL_SetHint(SDL.SDL_HINT_IME_SHOW_UI, "0");
        SDL.SDL_SetHint(SDL.SDL_HINT_IME_INTERNAL_EDITING, "1");
        SDL.SDL_SetHint(SDL.SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");
        SDL.SDL_StopTextInput();
    }

    /// <summary>
    /// 메인 루프가 현재 활성 상태인지 나타냅니다.
    /// </summary>
    public static bool Running { get; internal set; } = false;

    internal static System.Diagnostics.Stopwatch frametimer = new();
    static readonly double tickToMilliseconds = 1000d / System.Diagnostics.Stopwatch.Frequency;
    static readonly double tickToSeconds = 1d / System.Diagnostics.Stopwatch.Frequency;

    /// <summary>
    /// 경과된 프레임워크 런타임(스톱워치 틱 단위)입니다.
    /// </summary>
    public static long RunningTimeTick => frametimer.ElapsedTicks;

    /// <summary>
    /// 경과된 프레임워크 런타임(밀리초 단위, <see cref="float"/>)입니다.
    /// </summary>
    public static float RunningTimeToFloat => (float)(frametimer.ElapsedTicks * tickToMilliseconds);

    /// <summary>
    /// 경과된 프레임워크 런타임(밀리초 단위)입니다.
    /// </summary>
    public static double RunningTime => frametimer.ElapsedTicks * tickToMilliseconds;

    /// <summary>
    /// 경과된 프레임워크 런타임(초 단위)입니다.
    /// </summary>
    public static double RunningTimeToSecond => frametimer.ElapsedTicks * tickToSeconds;

    /// <summary>
    /// 메인 루프를 시작하고 <see cref="Stop"/>을 호출할 때까지 실행 상태를 유지합니다.
    /// </summary>
    /// <param name="callResize">true일 때, 루프 시작 전에 한 번의 리사이즈 이벤트를 발생시킵니다.</param>
    /// <param name="showWindow">true일 때, 루프 진입 전에 SDL 윈도우를 표시합니다.</param>
    public static void Run(bool callResize = false, bool showWindow = true)
    {
        if (Running) {
            throw new JyunrcaeaFrameworkException("Run() cannot be called while the framework is already running.");
        }

        Running = true;
        bool started = false;

        try {
            Framework.Function.Start();
            started = true;

            if (callResize) {
                Window.Resize(Window.Width, Window.Height);
            }

            FrameworkFunction.updateTime = 0;
            FrameworkFunction.endtime = Display.framelatelimit;
            frametimer.Restart();

            _ = SDL.SDL_SetRenderDrawColor(renderer, Window.BackgroundColor.colorbase.r, Window.BackgroundColor.colorbase.g, Window.BackgroundColor.colorbase.b, Window.BackgroundColor.colorbase.a);
            _ = SDL.SDL_RenderClear(renderer);

            if (showWindow) {
                SDL.SDL_ShowWindow(Framework.window);
            }

            RunningLoop();
        }
        finally {
            Running = false;
            try {
                if (started) {
                    Framework.Function.Stop();
                }
            }
            finally {
                ShutdownSdl();
            }
        }
    }

    static void RunningLoop()
    {
        SDL.SDL_Event e;
        while (Running) {
            while (SDL.SDL_PollEvent(out e) == 1) {
                EventProcess(e);
            }

            Framework.Function.Draw();
        }
    }

    static void EventProcess(SDL.SDL_Event e)
    {
        switch (e.type) {
            case SDL.SDL_EventType.SDL_QUIT:
            Framework.Function.WindowQuit();
            break;

            case SDL.SDL_EventType.SDL_WINDOWEVENT:
            switch (e.window.windowEvent) {
                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                Function.Resized();
                break;

                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                Framework.Function.WindowQuit();
                break;

                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
                Framework.Function.WindowMinimized();
                break;

                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MAXIMIZED:
                Framework.Function.WindowMaximized();
                break;

                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESTORED:
                Framework.Function.WindowRestore();
                break;

                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
                Framework.Function.KeyFocusOut();
                break;

                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
                Framework.Function.KeyFocusIn();
                break;

                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_LEAVE:
                Framework.Function.MouseFocusOut();
                break;

                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_ENTER:
                Framework.Function.MouseFocusIn();
                break;

                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_DISPLAY_CHANGED:
                if (SDL.SDL_GetDisplayMode(e.window.data1, 0, out Display.dm) != 0) {
                    throw new JyunrcaeaFrameworkException("Failed to query display information after monitor change.");
                }

                Framework.Function.DisplayChange();
                break;

                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                if (Display.KeepRenderingWhenResize) {
                    break;
                }

                Window.size.w = e.window.data1;
                Window.size.h = e.window.data2;
                Window.wh = Window.size.w * 0.5f;
                Window.hh = Window.size.h * 0.5f;
                Framework.Function.Resize();
                break;
            }
            break;

            case SDL.SDL_EventType.SDL_DROPFILE:
            Framework.Function.DropFile(SDL.UTF8_ToManaged(e.drop.file, true));
            break;

            case SDL.SDL_EventType.SDL_KEYDOWN:
            Framework.Function.KeyDown((Keycode)e.key.keysym.sym);
            if ((Keycode)e.key.keysym.sym == Keycode.BACKSPACE && Input.Text.InputedText.Length > 0) {
                Input.Text.InputedText = Input.Text.InputedText.Remove(Input.Text.InputedText.Length - 1);
                Function.InputText();
            }
            break;

            case SDL.SDL_EventType.SDL_MOUSEMOTION:
            Framework.Function.MouseMove();
            break;

            case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
            Framework.Function.MouseKeyDown((MouseKey)e.button.button);
            break;

            case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
            Framework.Function.MouseKeyUp((MouseKey)e.button.button);
            break;

            case SDL.SDL_EventType.SDL_KEYUP:
            Framework.Function.KeyUp((Keycode)e.key.keysym.sym);
            break;

            case SDL.SDL_EventType.SDL_TEXTINPUT:
            unsafe {
                Input.Text.InputedText += new string((sbyte*)e.text.text);
                Function.InputText();
            }
            break;
        }
    }

    /// <summary>
    /// 메인 루프의 종료를 요청합니다.
    /// </summary>
    public static void Stop()
    {
        Running = false;
    }

    /// <summary>
    /// 실행 중인 프레임워크 어셈블리를 가져옵니다.
    /// </summary>
    public static Assembly GetAssembly { get; } = Assembly.GetExecutingAssembly();

    internal static Stack<Size2D> DrawPosStack = new();
    internal static SDL.SDL_Rect DrawPos = new() { x = 0, y = 0 };
    internal static SDL.SDL_Rect RenderRange = new();

    internal static void Rendering(Group group)
    {
        group.Render(renderer);
    }

    internal static void Positioning(Group group)
    {
        group.UpdatePosition(DrawPos.x, DrawPos.y);
    }

    static void ShutdownSdl()
    {
        if (renderer != IntPtr.Zero) {
            SDL.SDL_DestroyRenderer(renderer);
            renderer = IntPtr.Zero;
        }

        if (window != IntPtr.Zero) {
            SDL.SDL_DestroyWindow(window);
            window = IntPtr.Zero;
        }

        SDL_mixer.Mix_CloseAudio();
        SDL_mixer.Mix_Quit();
        SDL_ttf.TTF_Quit();
        SDL_image.IMG_Quit();
        Input.Mouse.DisposeCachedCursors();
        SDL.SDL_Quit();
    }
}


