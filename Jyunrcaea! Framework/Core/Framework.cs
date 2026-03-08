using System.Reflection;
using SDL2;

namespace JyunrcaeaFramework.Core;

/// <summary>
/// 프레임워크에 대한 명령어가 모여있습니다.
/// 초기화, 시작, 종료 등이 있습니다.
/// </summary>
public static class Framework
{
    /// <summary>
    /// 가능한 CPU 사용량을 줄입니다. 안정적인 초당 프레임을 내는데에 방해가 될수 있습니다.
    /// </summary>
    public static bool SavingPerformance { get; set; } = true;
    /// <summary>
    /// 현재 프레임워크의 버전을 알려줍니다.
    /// </summary>
    public static readonly Version Version = new(0, 7, 1);
    /// <summary>
    /// 프레임워크가 이벤트를 받았을때 실행될 함수들이 들어있습니다.
    /// 'FrameworkFunction'을 상속해 기능을 추가할수 있습니다.
    /// </summary>
    public static FrameworkFunction Function = new();
    /// <summary>
    /// SDL2 Window
    /// </summary>
    internal static IntPtr window = IntPtr.Zero;
    /// <summary>
    /// SDL2 Renderer
    /// </summary>
    internal static IntPtr renderer = IntPtr.Zero;

    /// <summary>
    /// Jyunrcaea! Framework를 사용하기 위해 기본적으로 실행해야되는 초기화 함수입니다.
    /// </summary>
    /// <param name="title">창 제목</param>
    /// <param name="width">창 너비</param>
    /// <param name="height">창 높이</param>
    /// <param name="x">가로 위치 (null 일경우 중앙)</param>
    /// <param name="y">세로 위치 (null 일경우 중앙)</param>
    /// <param name="option">초기 창 생성옵션</param>
    /// <param name="keepRenderingWhenResize">창 조절 중에도 계속 렌더링 하기 (64비트 운영체제 전용)</param>
    /// <param name="renderOption">렌더러 옵션</param>
    /// <exception cref="JyunrcaeaFrameworkException">초기화 실패시</exception>
    public static void Init(string title, uint width, uint height, int? x = null, int? y = null, WindowOption? option = null, RenderOption? renderOption = null, AudioOption audioOption = default, bool keepRenderingWhenResize = true)
    {
        #region 값 검사
        if (audioOption.ch > 8) throw new JyunrcaeaFrameworkException("지원하지 않는 스테레오 ( AudioOption.Channls > 8)");
        #endregion
        #region SDL 라이브러리 초기화
        if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) != 0)
        {
            throw new JyunrcaeaFrameworkException($"SDL2 라이브러리 초기화에 실패하였습니다. SDL Error: {SDL.SDL_GetError()}");
        }
        SDL.SDL_WindowFlags winflg = option is null ? SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL : ((WindowOption)option).option;
        window = SDL.SDL_CreateWindow(title, x ?? SDL.SDL_WINDOWPOS_CENTERED, y ?? SDL.SDL_WINDOWPOS_CENTERED, (int)width, (int)height, winflg);
        if (window == IntPtr.Zero)
        {
            throw new JyunrcaeaFrameworkException($"창 초기화에 실패하였습니다. SDL Error: {SDL.SDL_GetError()}");
        }
        renderer = SDL.SDL_CreateRenderer(window, -1, renderOption is null ? SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED : ((RenderOption)renderOption).option);
        if (renderer == IntPtr.Zero)
        {
            SDL.SDL_DestroyWindow(window);
            throw new JyunrcaeaFrameworkException($"렌더 초기화에 실패하였습니다. SDL Error: {SDL.SDL_GetError()}");
        }
        if (renderOption is null || ((RenderOption)renderOption).anti_alising)
        {
            if (SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "2") == 0)
            {
                SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "1");
            }
        }
        if (SDL.SDL_SetRenderDrawBlendMode(renderer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND) != 0) throw new JyunrcaeaFrameworkException($"렌더러의 블랜더 모드 설정에 실패하였습니다. SDL Error: {SDL.SDL_GetError()}");
        //if (SDL.SDL_SetRenderDrawBlendMode(renderer, SDL.SDL_BlendMode.SDL_BLENDMODE_MOD) != 0) throw new JyunrcaeaFrameworkException("SDL Error: " + SDL.SDL_GetError());
        if (SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_JPG | SDL_image.IMG_InitFlags.IMG_INIT_WEBP | SDL_image.IMG_InitFlags.IMG_INIT_TIF | SDL_image.IMG_InitFlags.IMG_INIT_PNG) == 0)
        {
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_Quit();
            throw new JyunrcaeaFrameworkException($"SDL2 image 라이브러리 초기화에 실패하였습니다. SDL image Error : {SDL_image.IMG_GetError()}");
        }
        if (SDL_ttf.TTF_Init() == -1)
        {
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_Quit();
            SDL_image.IMG_Quit();
            throw new JyunrcaeaFrameworkException($"SDL2 ttf 라이브러리 초기화에 실패하였습니다. SDL ttf Error : {SDL_ttf.TTF_GetError()}");
        }
        if (SDL_mixer.Mix_Init(SDL_mixer.MIX_InitFlags.MIX_INIT_MP3 | SDL_mixer.MIX_InitFlags.MIX_INIT_OGG | SDL_mixer.MIX_InitFlags.MIX_INIT_MID | SDL_mixer.MIX_InitFlags.MIX_INIT_FLAC | SDL_mixer.MIX_InitFlags.MIX_INIT_OPUS | SDL_mixer.MIX_InitFlags.MIX_INIT_MOD) == 0)
        {
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_Quit();
            SDL_image.IMG_Quit();
            throw new JyunrcaeaFrameworkException($"SDL mixer 라이브러리 초기화에 실패하였습니다. SDL mixer Error : {SDL_mixer.Mix_GetError()}");
        }
        bool setting = true;
        while (setting)
        {
            if (SDL_mixer.Mix_OpenAudio(audioOption.hz, SDL_mixer.MIX_DEFAULT_FORMAT, audioOption.ch, audioOption.cs) != 0)
            {
                if (!audioOption.trylow || audioOption.ch == 1)
                {
                    SDL.SDL_DestroyWindow(window);
                    SDL.SDL_DestroyRenderer(renderer);
                    SDL.SDL_Quit();
                    SDL_image.IMG_Quit();
                    SDL_mixer.Mix_Quit();
                    throw new JyunrcaeaFrameworkException($"SDL mixer 오디오를 여는데 실패하였습니다. SDL mixer Error : {SDL_mixer.Mix_GetError()}");
                }
                audioOption.ch = (audioOption.ch == 7) ? 6 : audioOption.ch--;
            }
            else setting = false;
        }
        #endregion
        //만약 32bit 일경우 계속 렌더링 불가능
        if (IntPtr.Size != 4 && (Display.KeepRenderingWhenResize = keepRenderingWhenResize))
        {
            SDL.SDL_SetEventFilter((_, eventPtr) =>
                {
                    var e = (SDL.SDL_Event)System.Runtime.InteropServices.Marshal.PtrToStructure(eventPtr, typeof(SDL.SDL_Event))!;
                    //if (e.type == SDL.SDL_EventType.SDL_KEYDOWN && Input.TextInput.Enable && e.key.keysym.sym == SDL.SDL_Keycode.SDLK_BACKSPACE)
                    if (e.key.repeat != 0) return Input.Text.ti ? 1 : 0;
                    if (e.type != SDL.SDL_EventType.SDL_WINDOWEVENT) return 1;
                    switch (e.window.windowEvent)
                    {
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
        #region 믹서, 윈플래그, 텍스쳐 쉐어링, 창 크기, 디스플레이 모드, 힌트
        SDL_mixer.Mix_HookMusicFinished(Music.Finished);
        if ((winflg & SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP) == SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP) Window.fullscreenoption = true;
        if ((winflg & SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS) == SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS) Window.windowborderless = true;
        Window.size = new() { w = Window.default_size.x = Window.beforewidth = (int)width, h = Window.default_size.y = Window.beforeheight = (int)height };
        Window.wh = width * 0.5f;
        Window.hh = height * 0.5f;
        if (SDL.SDL_GetDisplayMode(0, 0, out Display.dm) < 0)
        {
            throw new JyunrcaeaFrameworkException("디스플레이 정보를 갖고오는데 실패했습니다.\nDisplay 클래스 내에 있는 일부 함수와 전체화면 전환이 작동하지 못합니다.");
        }
        SDL.SDL_SetHint(SDL.SDL_HINT_VIDEO_MINIMIZE_ON_FOCUS_LOSS, "1");
        SDL.SDL_SetHint(SDL.SDL_HINT_IME_SHOW_UI, "0");
        SDL.SDL_SetHint(SDL.SDL_HINT_IME_INTERNAL_EDITING, "1");
        //SDL2-CS
        SDL.SDL_SetHint(SDL.SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");
        SDL.SDL_StopTextInput();
        #endregion
    }
    public static bool Running { get; internal set; } = false;
    internal static System.Diagnostics.Stopwatch frametimer = new();
    /// <summary>
    /// 프레임워크가 지금까지 작동된 시간을 틱(tick)으로 반환합니다.
    /// </summary>
    public static long RunningTimeTick => frametimer.ElapsedTicks;
    /// <summary>
    /// 프레임워크가 지금까지 작동된 시간을 밀리초(ms)로 반환합니다.
    /// 하위 호환성을 위해 Float 반환을 제공합니다.
    /// </summary>
    public static float RunningTimeToFloat => frametimer.ElapsedTicks * 0.0001f;

    public static double RunningTime => frametimer.ElapsedTicks * 0.0001d;

    /// <summary>
    /// 프레임워크가 지금까지 작동된 시간을 초(second)로 반환합니다.
    /// </summary>
    public static double RunningTimeToSecond => frametimer.ElapsedTicks * 0.0000001d;

    /// <summary>
    /// Framework.Stop(); 을 호출할때까지 창을 띄웁니다. (또는 오류가 날때까지...)
    /// </summary>
    /// <param name="showWindow">창을 표시할지에 대한 여부</param>
    /// <param name="callResize">시작할때 Resize 이벤트도 호출할지에 대한 여부</param>
    /// <exception cref="JyunrcaeaFrameworkException">실행중에 호출할경우</exception>
    public static void Run(bool callResize = false, bool showWindow = true)
    {
        if (Running) throw new JyunrcaeaFrameworkException("이 함수는 이미 실행중인 함수입니다. (함수가 종료될때까지 호출할수 없습니다.)");
        Running = true;
        Framework.Function.Start();
        if (callResize) Window.Resize(Window.Width, Window.Height);
        FrameworkFunction.updateTime = 0;
        FrameworkFunction.endtime = Display.framelatelimit;
        frametimer.Start();
        SDL.SDL_SetRenderDrawColor(renderer, Window.BackgroundColor.colorbase.r, Window.BackgroundColor.colorbase.g, Window.BackgroundColor.colorbase.b, Window.BackgroundColor.colorbase.a);
        SDL.SDL_RenderClear(renderer);
        if (showWindow) SDL.SDL_ShowWindow(Framework.window);
        RunningLoop();
        Framework.Function.Stop();
        SDL.SDL_DestroyRenderer(renderer);
        SDL.SDL_DestroyWindow(window);
        SDL_mixer.Mix_Quit();
        SDL_image.IMG_Quit();
        SDL_ttf.TTF_Quit();
        SDL.SDL_Quit();
    }


    static void RunningLoop()
    {
        SDL.SDL_Event e;
        while (Running)
        {
            while (SDL.SDL_PollEvent(out e) == 1) EventProcess(e);
            Framework.Function.Draw();
        }
    }

    [Obsolete("I think this is unstable and causing bottleneck.")]
    static async void AsyncEventProcess(SDL.SDL_Event e)
    {
        await Task.Run(() => EventProcess(e));
    }

    static void EventProcess(SDL.SDL_Event e)
    {
        switch (e.type)
        {
            case SDL.SDL_EventType.SDL_QUIT:
                Framework.Function.WindowQuit();
                break;
            case SDL.SDL_EventType.SDL_WINDOWEVENT:
                switch (e.window.windowEvent)
                {
                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                        Function.Resized();
                        break;
                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                        Framework.Function.WindowQuit();
                        break;
                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SHOWN:
                        break;
                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_HIDDEN:
                        break;
                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
                        Framework.Function.WindowMinimized();
                        break;
                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MAXIMIZED:
                        Framework.Function.WindowMaximized();
                        break;
                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_EXPOSED:

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
                        if (SDL.SDL_GetDisplayMode(e.display.data1, 0, out Display.dm) != 0) throw new JyunrcaeaFrameworkException("창이 이동된 모니터의 정보를 얻는데 실패했습니다.");
                        Framework.Function.DisplayChange();
                        break;
                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MOVED:
                        break;
                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                        if (Display.KeepRenderingWhenResize) break;
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
                if ((Keycode)e.key.keysym.sym == Keycode.BACKSPACE && Input.Text.InputedText.Length > 0)
                {
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
                unsafe
                {
                    Input.Text.InputedText += new string((sbyte*)e.text.text);
                    Function.InputText();
                }
                break;
            case SDL.SDL_EventType.SDL_TEXTEDITING:
                unsafe
                {
                    Console.WriteLine("Edit Text: {0}\nCursor Pos: {1}\nSelected Line {2}", new string((sbyte*)e.edit.text), e.edit.start, e.edit.length);
                }
                break;
        }

    }
    /// <summary>
    /// 프레임워크를 중지합니다.
    /// 즉, 창을 종료합니다.
    /// </summary>
    public static void Stop()
    {
        Running = false;
    }
    /// <summary>
    /// 해당 프레임워크의 어셈블리를 가져옵니다.
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
}
