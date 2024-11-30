#define WINDOWS
using SDL2;
using System.Reflection;

namespace JyunrcaeaFramework;

/// <summary>
/// 프레임워크에 대한 명령어가 모여있습니다.
/// 초기화, 시작, 종료 등이 있습니다.
/// </summary>
public static class Framework
{
    /// <summary>
    /// 가능한 CPU 사용량을 줄입니다. 안정적인 초당 프레임을 내는데에 방해가 될수 있습니다.
    /// </summary>
    public static bool SavingPerformance = true;
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
    /// <param name="KeepRenderingWhenResize">창 조절 중에도 계속 렌더링 하기 (64비트 운영체제 전용)</param>
    /// <param name="render_option">렌더러 옵션</param>
    /// <exception cref="JyunrcaeaFrameworkException">초기화 실패시</exception>
    public static void Init(string title, uint width, uint height, int? x = null, int? y = null, WindowOption? option = null, RenderOption? render_option = null, AudioOption audio_option = default,bool KeepRenderingWhenResize = true)
    {
        #region 값 검사
        if (audio_option.ch > 8) throw new JyunrcaeaFrameworkException("지원하지 않는 스테레오 ( AudioOption.Channls > 8)");
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
        renderer = SDL.SDL_CreateRenderer(window, -1, render_option is null ? SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED : ((RenderOption)render_option).option);
        if (renderer == IntPtr.Zero)
        {
            SDL.SDL_DestroyWindow(window);
            throw new JyunrcaeaFrameworkException($"렌더 초기화에 실패하였습니다. SDL Error: {SDL.SDL_GetError()}");
        }
        if (render_option is null || ((RenderOption)render_option).anti_alising)
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
        while (setting) {
            if (SDL_mixer.Mix_OpenAudio(audio_option.hz, SDL_mixer.MIX_DEFAULT_FORMAT, audio_option.ch, audio_option.cs) != 0)
            {
                if (!audio_option.trylow || audio_option.ch == 1)
                {
                    SDL.SDL_DestroyWindow(window);
                    SDL.SDL_DestroyRenderer(renderer);
                    SDL.SDL_Quit();
                    SDL_image.IMG_Quit();
                    SDL_mixer.Mix_Quit();
                    throw new JyunrcaeaFrameworkException($"SDL mixer 오디오를 여는데 실패하였습니다. SDL mixer Error : {SDL_mixer.Mix_GetError()}");
                }
                audio_option.ch = (audio_option.ch == 7) ? 6 : audio_option.ch--;
            }
            else setting = false;
        }
        #endregion
        //만약 32bit 일경우 계속 렌더링 불가능
        if (IntPtr.Size != 4 && (Display.KeepRenderingWhenResize = KeepRenderingWhenResize))
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
                            return 1;
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
    /// <param name="ShowWindow">창을 표시할지에 대한 여부</param>
    /// <param name="CallResize">시작할때 Resize 이벤트도 호출할지에 대한 여부</param>
    /// <exception cref="JyunrcaeaFrameworkException">실행중에 호출할경우</exception>
    public static void Run(bool CallResize = false, bool ShowWindow = true)
    {
        if (Running) throw new JyunrcaeaFrameworkException("이 함수는 이미 실행중인 함수입니다. (함수가 종료될때까지 호출할수 없습니다.)");
        Running = true;
        Framework.Function.Start();
        if (CallResize) Window.Resize(Window.Width,Window.Height);
        FrameworkFunction.updatetime = 0;
        FrameworkFunction.endtime = Display.framelatelimit;
        frametimer.Start();
        SDL.SDL_SetRenderDrawColor(renderer, Window.BackgroundColor.colorbase.r, Window.BackgroundColor.colorbase.g, Window.BackgroundColor.colorbase.b, Window.BackgroundColor.colorbase.a);
        SDL.SDL_RenderClear(renderer);
        if (ShowWindow) SDL.SDL_ShowWindow(Framework.window);
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
                if((Keycode)e.key.keysym.sym == Keycode.BACKSPACE && Input.Text.InputedText.Length > 0)
                {
                    Input.Text.InputedText = Input.Text.InputedText.Remove(Input.Text.InputedText.Length - 1);
                    Function.InputText();
                }
                break;
            case SDL.SDL_EventType.SDL_MOUSEMOTION:
                Framework.Function.MouseMove();
                break;
            case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                Framework.Function.MouseKeyDown((Input.Mouse.Key)e.button.button);
                break;
            case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                Framework.Function.MouseKeyUp((Input.Mouse.Key)e.button.button);
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
        SDL.SDL_Rect? before = null;    
        for (int i = 0; i < group.Objects.Count; i++)
        {
            if (group.Objects[i].Hide) continue;

            if (group.Objects[i] is Group)
            {
                Rendering((Group)group.Objects[i]);
                continue;
            }

            if (group.Objects[i] is Box)
            {
                if (group.Objects[i] is RoundBox)
                {
                    SDL.SDL_Rect r = ((DrawableObject)group.Objects[i]).renderposition;
                    SDL_gfx.roundedBoxRGBA(
                        Framework.renderer,
                        (short)r.x,
                        (short)r.y,
                        (short)(r.x + r.w),
                        (short)(r.y + r.h),
                        ((RoundBox)group.Objects[i]).Radius,
                        ((Box)group.Objects[i]).Color.colorbase.r,
                        ((Box)group.Objects[i]).Color.colorbase.g,
                        ((Box)group.Objects[i]).Color.colorbase.b,
                        ((Box)group.Objects[i]).Color.colorbase.a
                    );
                    continue;
                }
                SDL.SDL_SetRenderDrawColor(Framework.renderer, ((Box)group.Objects[i]).Color.colorbase.r, ((Box)group.Objects[i]).Color.colorbase.g, ((Box)group.Objects[i]).Color.colorbase.b, ((Box)group.Objects[i]).Color.colorbase.a);
                SDL.SDL_RenderFillRect(Framework.renderer, ref ((DrawableObject)group.Objects[i]).renderposition);
                continue;
            }

            if (group.Objects[i] is Image)
            {
                SDL.SDL_RenderCopyEx(Framework.renderer, ((Image)group.Objects[i]).Texture.texture, ref ((Image)group.Objects[i]).Texture.src, ref ((DrawableObject)group.Objects[i]).renderposition, ((Image)group.Objects[i]).Rotation, IntPtr.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
                continue;
            }

            if (group.Objects[i] is Text)
            {
                SDL.SDL_RenderCopyEx(Framework.renderer, ((Text)group.Objects[i]).TFT.texture, ref ((Text)group.Objects[i]).TFT.src, ref ((DrawableObject)group.Objects[i]).renderposition, ((Text)group.Objects[i]).Rotation, IntPtr.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
                continue;
            }

            if (group.Objects[i] is Circle)
            {
                SDL_gfx.filledCircleRGBA(Framework.renderer, (short)((Circle)group.Objects[i]).renderposition.x, (short)((Circle)group.Objects[i]).renderposition.y, ((Circle)group.Objects[i]).Radius, ((Circle)group.Objects[i]).Color.colorbase.r, ((Circle)group.Objects[i]).Color.colorbase.g, ((Circle)group.Objects[i]).Color.colorbase.b, ((Circle)group.Objects[i]).Color.colorbase.a);
                continue;
            }
        }
        if (before is not null)
        {
            RenderRange = (SDL.SDL_Rect)before;
            SDL.SDL_RenderSetViewport(Framework.renderer,ref RenderRange);
        }
    }

    internal static void Positioning(Group group)
    {
        DrawPosStack.Push(new() { Width = DrawPos.x, Height = DrawPos.y });
        int wx = DrawPos.x + group.Rx;
        int hy = DrawPos.y + group.Ry;
        SDL.SDL_Rect? before = null;
        //if (group.RenderRange is not null)
        //{
        //    before = RenderRange;
        //    RenderRange = new()
        //    {
        //        x = wx,
        //        y = hy,
        //        w = group.RenderRange.Width,
        //        h = group.RenderRange.Height
        //    };
        //    wx = 0;
        //    hy = 0;
        //}

        for (int i = 0; i < group.Objects.Count; i++)
        {
            DrawPos.x = wx;
            DrawPos.y = hy;

            if (group.Objects[i] is Group)
            {
                Positioning((Group)group.Objects[i]);
                continue;
            }

            if (group.Objects[i] is DrawableObject)
            {
                DrawableObject zdo = (DrawableObject)group.Objects[i];
                DrawPos.w = zdo.RealWidth;
                DrawPos.h = zdo.RealHeight;
                DrawPos.x += zdo.Rx;
                DrawPos.y += zdo.Ry;
                if (zdo.DrawX != HorizontalPositionType.Right) DrawPos.x -= zdo.DrawX == HorizontalPositionType.Middle ? (int)(DrawPos.w * 0.5f) : DrawPos.w;
                if (zdo.DrawY != VerticalPositionType.Bottom) DrawPos.y -= zdo.DrawY == VerticalPositionType.Middle ? (int)(DrawPos.h * 0.5f) : DrawPos.h;
                zdo.renderposition.w = DrawPos.w;
                zdo.renderposition.h = DrawPos.h;
                zdo.renderposition.x = DrawPos.x;
                zdo.renderposition.y = DrawPos.y;
                continue;
            }

        }

        if (before is not null)
        {
            RenderRange = (SDL.SDL_Rect)before;
        }
        var ret = DrawPosStack.Pop();
        DrawPos.x = ret.Width;
        DrawPos.y = ret.Height;
    }
}

public delegate void NormalEvent();

public delegate void KeyboardEvent(Keycode e);

public delegate void ClickEvent(Input.Mouse.Key e);

public class StaticGroup : Group
{
    /// <summary>
    /// 제한된 범위의 크기를 지정합니다.
    /// </summary>
    public Size2D LimitedRange = new();

    
}

/// <summary>
/// 크기를 구하거나 렌더 방향을 지정할수 있는 동적인 그룹입니다.
/// </summary>
public class DynamicGroup : Group, DetailOfObject.Size
{
    public int DisplayedWidth => contentrange.w;
    public int DisplayedHeight => contentrange.h;

    internal SDL.SDL_Rect contentrange = new();

    public override void Update(float ms)
    {
        base.Update(ms);
        int i = 0;
        while (this.Objects[i] is not DrawableObject || this.Objects[i] is not DynamicGroup)
        {
            // 그릴수 있는 객체가 없는경우
            if (i++ >= this.Objects.Count)
            {
                contentrange.x = -1;
                contentrange.y = -1;
                contentrange.w = 0;
                contentrange.h = 0;
                return;
            }
        }
        int left = ((DrawableObject)this.Objects[i]).renderposition.x, right = left + ((DrawableObject)this.Objects[i]).renderposition.w;
        int top = ((DrawableObject)this.Objects[i]).renderposition.y, bottom = top + ((DrawableObject)this.Objects[i]).renderposition.h;
        int ww, hh;
        for (;i<this.Objects.Count;i++)
        {
            if (this.Objects[i] is DrawableObject)
            {
                // 왼쪽
                ww = ((DrawableObject)this.Objects[i]).renderposition.x;
                if (ww < left) left = ww;
                // 오른쪽
                ww += ((DrawableObject)this.Objects[i]).renderposition.w;
                if (ww > right) right = ww;
                //위
                hh = ((DrawableObject)this.Objects[i]).renderposition.y;
                if (hh < top) top = hh;
                //아레
                hh += ((DrawableObject)this.Objects[i]).renderposition.h;
                if (hh > bottom) bottom = hh;
            }
            if (this.Objects[i] is DynamicGroup)
            {
                // 왼쪽
                ww = ((DynamicGroup)this.Objects[i]).contentrange.x;
                if (ww < left) left = ww;
                // 오른쪽
                ww += ((DynamicGroup)this.Objects[i]).contentrange.w;
                if (ww > right) right = ww;
                //위
                hh = ((DynamicGroup)this.Objects[i]).contentrange.y;
                if (hh < top) top = hh;
                //아레
                hh += ((DynamicGroup)this.Objects[i]).contentrange.h;
                if (hh > bottom) bottom = hh;
            }
        }

        this.contentrange.x = left;
        this.contentrange.y = top;
        this.contentrange.w = right - left;
        this.contentrange.h = bottom - top;
    }


}


/// <summary>
/// 최상위 그룹입니다.
/// </summary>
public class TopGroup : Group
{
    public TopGroup()
    {
        this.Ancestor = this;
        this.Parent = this;
        PushEvent(this);
    }

    internal EventList EventManager = new();

    internal override void PushEvent(BaseObject target)
    {
        EventManager.Add(target);
    }
}

public class EventList
{
    internal Queue<BaseObject> ObjectQueue = new();

    internal List<Events.Resized> Resized = new();
    internal List<Events.Resize> Resize = new();
    internal List<Events.Update> Update = new();
    internal List<Events.KeyDown> KeyDown = new();
    internal List<Events.KeyUp> keyUp = new();
    internal List<Events.MouseMove> mouseMoves = new();
    internal List<Events.MouseKeyDown> mouseKeyDowns = new();
    internal List<Events.MouseKeyUp> mouseKeyUps = new();

    public void Add(BaseObject obj)
    {
        if (obj is not Group)
        {
             Ad(Resize, obj);
             Ad(Update, obj);
        }
        Ad(Resized, obj);
        Ad(KeyDown, obj);
        Ad(keyUp, obj);
        Ad(mouseMoves, obj);
        Ad(mouseKeyDowns, obj);
        if (obj is Events.MouseKeyUp)
            Ad(mouseKeyUps, obj);
    }

    public void Remove(object obj)
    {
        Rd(Resized, obj);
        Rd(Resize, obj);
        Rd(Update, obj);
        Rd(KeyDown, obj);
        Rd(keyUp, obj);
        Rd(mouseMoves, obj);
        Rd(mouseKeyDowns, obj);
        Rd(mouseKeyUps, obj);
    }

    internal void Ad<T>(List<T> li,object obj)
    {
        if (obj is not T) return;
        li.Add((T)obj);
    }

    internal bool Rd<T>(List<T> li,object obj)
    {
        if (obj is not T) return false;
        return li.Remove((T)obj);
    }

    public void Refresh()
    {
        while (ObjectQueue.Count != 0) Add(ObjectQueue.Dequeue());
    }
}

/// <summary>
/// 크기 조정 클래스입니다.
/// </summary>
public class Size2D
{
    public int Width, Height;
    public Size2D(int Width = 0, int Height = 0)
    {
        this.Width = Width;
        this.Height = Height;
    }
}

public class RenderRange2D : Size2D
{
    HorizontalPositionType DrawX;
    VerticalPositionType DrawY;

    public RenderRange2D(int Width=0,int Height=0,HorizontalPositionType DrawX = HorizontalPositionType.Middle,VerticalPositionType DrawY = VerticalPositionType.Middle) : base(Width,Height)
    {
        this.Width = Width;
        this.Height = Height;
        this.DrawX = DrawX;
        this.DrawY = DrawY;
    }
}

public class Text : ExtendDrawableObject, Events.Update, Events.Resize
{
    /// <summary>
    /// Text 오브젝트를 생성합니다. (Zenerety 렌더링 전용)
    /// </summary>
    /// <param name="content">출력할 내용. (null 일경우 빈 텍스트)</param>
    /// <param name="textcolor">글자 색깔. (null 일경우 검은색)</param>
    public Text(string? content = null, int size = 0, Color? textcolor = null, string? fontfilepath = null)
    {
        this.TFT = new TextureFromText(fontfilepath is null ? Font.DefaultPath : fontfilepath, realsize = size, content is null ? string.Empty : content, textcolor is null ? Color.Black : textcolor);
    }

    internal TextureFromText TFT;
    public string Content
    {
        get => TFT.Text;
        set
        {
            TFT.Text = value;
            refresh = true;
        }
    }
    internal int realsize;
    public int FontSize {
        get => realsize;
        set
        {
            realsize = value;
            TFT.Resize(this.RelativeSize ? (int)(Window.AppropriateSize * (float)realsize) : realsize);
            refresh = true;
        }
    }
    public uint WrapWidth
    {
        get => TFT.WarpLength;
        set
        {
            TFT.WarpLength = value;
            refresh = true;
        }
    }
    public Color TextColor
    {
        get => TFT.Color;
        set
        {
            TFT.Color = value;
            refresh = true;
        }
    }

    public Color? Background
    {
        get => TFT.BackgroundColor;
        set
        {
            TFT.BackgroundColor = value;
            refresh = true;
        }
    }

    internal override int RealWidth => (int)((AbsoluteSize is null ? TFT.Width : AbsoluteSize.Width) * Scale.X);
    internal override int RealHeight => (int)((AbsoluteSize is null ? TFT.Height : AbsoluteSize.Height) * Scale.Y);

    public bool Blended { get => TFT.Blended; set => TFT.Blended = value; }
    internal bool refresh = false;

    public override byte Opacity { get => TFT.Opacity; set => TFT.Opacity = value; }

    /// <summary>
    /// 변경 사항이 있을경우 텍스트를 다시 렌더링합니다. (일반적으로 이 코드는 마지막에 호출하는게 좋습니다.)
    /// </summary>
    /// <param name="ms"></param>
    public virtual void Update(float ms)
    {
        if (refresh)
        {
            if (this.FontSize != 0) TFT.ReRender();
            refresh = false;
        }
    }

    public virtual void Resize()
    {
        if (this.RelativeSize) { TFT.Resize((int)(Window.AppropriateSize * (float)realsize)); refresh = true; }
    }
}

/// <summary>
/// 배율 조정 클래스입니다.
/// </summary>
public class Scale2D
{
    public double X, Y;
    public Scale2D(double x,double y)
    {
        X = x;
        Y = y;
    }
    public Scale2D(double xy)
    {
        X = Y = xy;
    }
    public Scale2D()
    {
        X = 1d;
        Y = 1d;
    }
}

/// <summary>
/// 모든 객체의 기본이 되는 핵심 객체 
/// </summary>
public class BaseObject
{
    public bool Hide;

    public int X { get; set; }
    public int Y { get; set; }
    internal virtual int Rx => X + Cx;
    internal virtual int Ry => Y + Cy;

    public Group? Parent { get; internal set; } = null;

    public T TypedParent<T>() where T : Group
    {
        if (this.Parent is null) throw new JyunrcaeaFrameworkException("이 객체가 포함된 그룹이 없습니다.");
        return (T)this.Parent;
    }

    internal int Cx = 0, Cy = 0;

    internal double CxD = 0.5, CyD = 0.5;

    //public bool CxIsChanged = false;
    //public bool CyIsChanged = false;

    public double CenterX
    {
        get => CxD;
        //{
        //    // 이미 값이 변경되었고, 부모가 null이 아닐경우 계산
        //    if (CxIsChanged && Parent is not null)
        //    {
        //        CxIsChanged = false;
        //        Cx = (int)(Parent.RealWidth * CxD);
        //    }
        //    return CxD;
        //}
        set
        {
            CxD = value;
            //if (CxIsChanged && Parent is not null) Cx = (int)(Parent.RealWidth * value);
        }
    }
    
    public double CenterY
    {
        get => CyD;
        set
        {
            CyD = value;
            //if (Parent is not null) Cy = (int)(Parent.RealWidth * value);
        }
    }
    

    internal bool MoveAnimation = false;
}

/// <summary>
/// 그릴수 있는 객체
/// </summary>
public abstract class DrawableObject : BaseObject, DetailOfObject.Size
{
    internal int rw = 0, rh = 0;
    internal int ww = 0, hh = 0;

    /// <summary>
    /// 실제 너비
    /// </summary>
    internal virtual int RealWidth { get; }
    /// <summary>
    /// 실제 높이
    /// </summary>
    internal virtual int RealHeight { get; }

    public int DisplayedWidth => this.RealWidth;
    public int DisplayedHeight => this.RealHeight;

    public abstract byte Opacity { get; set; }

    internal int dxw=0, dyh=0;

    /// <summary>
    /// 렌더링 될 이미지의 절대적 크기
    /// (null 로 설정시 원본 이미지의 크기를 따릅니다.)
    /// </summary>
    public Size2D? AbsoluteSize = null;

    /// <summary>
    /// 이미지의 너비 및 높이의 배율
    /// </summary>
    public Scale2D Scale = new();
    public double ScaleX { get => Scale.X; set => Scale.X = value; }
    public double ScaleY { get => Scale.Y; set => Scale.Y = value; }

    /// <summary>
    /// 창 크기에 맞춰 자동으로 크기 조정을 사용할지에 대한 여부입니다.
    /// </summary>
    public bool RelativeSize = true;

    /// <summary>
    /// 실제 렌더링 범위
    /// </summary>
    internal SDL.SDL_Rect renderposition = new();

    public bool MouseOver => SDL.SDL_PointInRect(ref Input.Mouse.position, ref this.renderposition) == SDL.SDL_bool.SDL_TRUE;
    public bool OverLap(DrawableObject OtherTarget) => SDL.SDL_IntersectRect(ref this.renderposition, ref OtherTarget.renderposition, out _) == SDL.SDL_bool.SDL_TRUE;

    public HorizontalPositionType DrawX = HorizontalPositionType.Middle;
    public VerticalPositionType DrawY = VerticalPositionType.Middle;
}

/// <summary>
/// 그리기도 가능하고 회전, 뒤집기 등이 가능한 확장된 객체
/// </summary>
public abstract class ExtendDrawableObject : DrawableObject
{
    /// <summary>
    /// 회전값
    /// </summary>
    public double Rotation = 0;

    public int AbsoluteWidth => this.RealWidth;
    public int AbsoluteHeight => this.RealHeight;
}

/// <summary>
/// 다른 객체를 묶어주는 그룹 객체입니다.
/// </summary>
public class Group : BaseObject, Events.Update, Events.Resize, Events.Prepare
{
    internal override int Rx => this.X;
    internal override int Ry => this.Y;

    public Group()
    {
        Objects = new(this);
    }

    internal TopGroup? Ancestor = null;

    public bool ResourceReady { get; internal set; } = false;

    /// <summary>
    /// 묶을 객체들
    /// </summary>
    public ObjectList Objects { get; protected set; } = null!;

    internal virtual void ResetPosition(Size2D Position)
    {
        for (int i = 0; i < Objects.Count; i++)
        {
            Objects[i].Cx = (int)(Position.Width * Objects[i].CxD);
            Objects[i].Cy = (int)(Position.Height * Objects[i].CyD);
            if (Objects[i] is Group) ((Group)Objects[i]).ResetPosition(Position);
        }
    }

    internal int RealWidth => base.Parent is null ? Window.Width : base.Parent.RealWidth;

    internal int RealHeight => base.Parent is null ? Window.Height : base.Parent.RealHeight;

    /// <summary>
    /// 오브젝트 준비
    /// </summary>
    public virtual void Prepare()
    {
        for (int i = 0; i < this.Objects.Count; i++)
        {
            this.Ancestor!.PushEvent(this.Objects[i]);

            if (this.Objects[i] is Group)
            {
                ((Group)this.Objects[i]).Objects.Ancestor =((Group)this.Objects[i]).Ancestor = this.Ancestor;
            }
            else if (this.Objects[i] is Text)
            {
                ((Text)Objects[i]).TFT.Ready();
            }

            if (this.Objects[i] is Events.Prepare prepare)
                prepare.Prepare();
        }
        this.ResourceReady = true;
    }

    /// <summary>
    /// 하위 객체들의 리소스를 해제하는 함수입니다. (override 할 경우 base.Release() 가 한번 실행되어야 합니다.)
    /// </summary>
    public virtual void Release()
    {
        this.ResourceReady = false;
        for (int i = 0; i < this.Objects.Count; i++)
        {
            if (this.Objects[i] is Group)
            {
                ((Group)this.Objects[i]).Release();
                continue;
            }

            if (this.Objects[i] is Image)
            {
                ((Image)this.Objects[i]).Texture.Dispose();
            }

            if (this.Objects[i] is Text)
            {
                ((Text)this.Objects[i]).TFT.Dispose();
            }
        }
    }

    public virtual void Update(float ms)
    {
        if(this.Objects.AddQueueCount != 0)
            this.Objects.Update();
        for (int i=0;i<this.Objects.Count;i++)
        {
            if (Objects[i] is Events.Update) ((Events.Update)Objects[i]).Update(ms);
        }
    }

    public virtual void Resize()
    {
        for (int i = 0; i < this.Objects.Count; i++)
        {
            if (Objects[i] is Events.Resize) ((Events.Resize)Objects[i]).Resize();
        }
    }

    internal virtual void PushEvent(BaseObject target)
    {
        this.Ancestor!.PushEvent(target);
        if (target is not Group)
        {
            return;
        }
        Group group = (Group)target;
        for (int i =0;i<group.Objects.Count;i++)
        {
            this.PushEvent(group.Objects[i]);
        }
    }
}

/// <summary>
/// 직사각형을 그리는 객체입니다.
/// </summary>
public class Box : DrawableObject, Animation.Available.Opacity, Animation.Available.Size
{
    public Box(int width = 0,int height = 0,Color? color = null)
    {
        this.Size = new(width, height);
        this.Color = color is null ? Color.White : color;
    }

    /// <summary>
    /// 직사각형의 너비와 높이
    /// </summary>
    public Size2D Size { get; set; }

    /// <summary>
    /// 출력할 색상
    /// </summary>
    public Color Color;

    public override byte Opacity { get => Color.Alpha; set => Color.Alpha = value; }

    internal override int RealWidth =>  (int)(Size.Width * Scale.X * (this.RelativeSize ? Window.AppropriateSize : 1));
    internal override int RealHeight => (int)(Size.Height * Scale.Y * (this.RelativeSize ? Window.AppropriateSize : 1));
}

/// <summary>
/// 이미지를 출력하는 객체입니다.
/// </summary>
public class Image : ExtendDrawableObject, Animation.Available.Opacity
{
    /// <summary>
    /// Image 객체를 생성합니다.
    /// </summary>
    /// <param name="Texture">텍스쳐</param>
    public Image(DrawableTexture? Texture = null)
    {
        if (Texture is null) return;
        this.Texture = Texture;
    }

    public Image(string ImageFilePath)
    {
        this.Texture = new TextureFromFile(ImageFilePath);
    }

    public DrawableTexture Texture = null!;

    public override byte Opacity { get => Texture.Opacity;
        set {
            Texture.Opacity = value;
        }
    }

    internal override int RealWidth => (int)((AbsoluteSize is null ? Texture.Width : AbsoluteSize.Width) * Scale.X * (this.RelativeSize ? Window.AppropriateSize : 1));
    internal override int RealHeight => (int)((AbsoluteSize is null ? Texture.Height : AbsoluteSize.Height) * Scale.Y * (this.RelativeSize ? Window.AppropriateSize : 1));
}

/// <summary>
/// 원을 그리는 객체입니다.
/// </summary>
public class Circle : DrawableObject, Animation.Available.Opacity
{
    public Circle(short radius=0, Color? color = null)
    {
        this.Radius = radius;
        this.Color = color is null ? Color.White : color;
    }

    public short Radius;

    public Color Color;

    public override byte Opacity { get => Color.Alpha; set => Color.Alpha = value; }

    internal override int RealWidth => (int)(Radius * 2 * (RelativeSize ? Window.AppropriateSize : 1));
    internal override int RealHeight => (int)(Radius * 2 * (RelativeSize ? Window.AppropriateSize : 1));
}

/// <summary>
/// 그려지지 않는 가짜 객체입니다.
/// </summary>
public class GhostBox : DrawableObject
{
    public GhostBox(int width=0,int height=0)
    {
        this.Size = new(width, height);
    }
    public Size2D Size;

    public override byte Opacity { get; set; } = 0;

    internal override int RealWidth => (int)(Size.Width * Scale.X * (this.RelativeSize ? Window.AppropriateSize : 1));
    internal override int RealHeight => (int)(Size.Height * Scale.Y * (this.RelativeSize ? Window.AppropriateSize : 1));
}

/// <summary>
/// 모서리가 둥근 직사각형을 그리는 객체입니다.
/// </summary>
public class RoundBox : Box
{
    public RoundBox(int Width = 0,int Height = 0,short Radius = 0,Color? color = null) : base(Width,Height,color)
    {
        this.Radius = Radius;
    }

    /// <summary>
    /// 모서리의 둥글기 정도 (픽셀 기준)
    /// </summary>
    public short Radius;
}

class AddTarget
{
    public BaseObject target;
    public int index;

    public AddTarget(BaseObject obj,int pos = -1)
    {
        this.index = pos;
        this.target = obj;
    }
}

/// <summary>
/// 오브젝트 리스트입니다. 추가 및 제거시 자동으로 리소스를 할당/해제 해줍니다.
/// </summary>
public class ObjectList : List<BaseObject>, IDisposable
{
    internal TopGroup? Ancestor;

    Group Parent;
    public ObjectList(Group group)
    {
        Parent = group;
        Ancestor = group.Ancestor;
    }

    Queue<AddTarget> AddList = new();
    public int AddQueueCount => AddList.Count;

    /// <summary>
    /// 객체 목록에 변경해야될 사항이 있는 경우 목록을 새로 고칩니다.
    /// 만약 즉시 추가해야될 객체가 있는경우 이 함수를 호출하여 즉시 추가하세요.
    /// </summary>
    public void Update()
    {
        if (AddList.Count == 0) return;
        while (AddList.Count != 0)
        {
            var target = AddList.Dequeue();
            if (target.index == -1) base.Add(target.target);
            else base.Insert(target.index, target.target);
            if (target.target is Text)
            {
                Console.WriteLine(((Text)target.target).TFT.ResourceReady);
            }
            //AddProcedure(target);
        }
    }

    void AddProcedure(BaseObject obj)
    {
        if (this.Ancestor is not null) this.Ancestor.EventManager.Add(obj);
        if (obj is Group)
        {
            ((Group)obj).Ancestor = this.Ancestor;
            ((Group)obj).Prepare();
            return;
        }
        if (obj is Image)
        {
            ((Image)obj).Texture.Ready();
            return;
        }
        if (obj is Text)
        {
            if (!((Text)obj).TFT.ResourceReady) ((Text)obj).TFT.Ready();
        }
        //if(Ancestor is not null) Ancestor.EventManager.Add(obj);
    }

    /// <summary>
    /// 객체를 추가합니다.
    /// </summary>
    /// <param name="obj">객체</param>
    public new void Add(BaseObject obj)
    {
        if (obj.Parent is not null) throw new JyunrcaeaFrameworkException("이미 다른 부모 객체에게 상속된 객체입니다.");
        obj.Parent = this.Parent;
        if (this.Parent.ResourceReady)
        {
            AddProcedure(obj);
            this.AddList.Enqueue(new(obj));
        }
        else base.Add(obj);
    }

    public new void AddRange(IEnumerable<BaseObject> objs)
    {
        foreach(var a in objs)
        {
            Add(a);
        }
    }

    public void AddRange(params BaseObject[] objs)
    {
        for (int i =0;i < objs.Length;i++)
        {
            this.Add(objs[i]);
        }
    }

    public void AddRange(int index = 0,params BaseObject[] objs)
    {
        for (int i = 0; i < objs.Length; i++)
        {
            this.Insert(index + i, objs[i]);
        }
    }

    public new void Insert(int index,BaseObject zo)
    {
        if (zo.Parent is not null) throw new JyunrcaeaFrameworkException("이미 다른 부모 객체에게 상속된 객체입니다.");
        zo.Parent = this.Parent;
        if (this.Parent.ResourceReady) {
            AddProcedure(zo);
            this.AddList.Enqueue(new(zo, index));
        }
        else base.Insert(index, zo);
    }


    public bool Switch(BaseObject target,int index = -1)
    {
        if (!base.Remove(target)) return false;

        if (index == -1)
        {
            base.Add(target);
        } else
        {
            base.Insert(index, target);
        }
        return true;
    }

    /// <summary>
    /// 객체를 제거합니다.
    /// </summary>
    /// <param name="obj">객체</param>
    public new bool Remove(BaseObject obj)
    {
        if(base.Remove(obj) is false) return false;
        if (obj is Group)
        {
            ((Group)obj).Release();
            return true;
        }
        if (obj is Image)
        {
            ((Image)obj).Texture.Dispose();
            return true;
        }
        if (obj is Text)
        {
            ((Text)obj).TFT.Dispose();
            return true;
        }
        return true;
    }

    public void Dispose()
    {
        if (Framework.Running)
        foreach(var obj in this)
        {
            if (obj is Group)
            {
                ((Group)obj).Release();
                return;
            }
            if (obj is Image)
            {
                ((Image)obj).Texture.Dispose();
                return;
            }
            if (obj is Text)
            {
                ((Text)obj).TFT.Dispose();
                return;
            }  
        }
    }
}

/// <summary>
/// 모니터의 정보를 얻거나, 또는 장면을 추가/제거 하기위한 클래스입니다.
/// </summary>
public static class Display
{
    static TopGroup _target = new();
    /// <summary>
    /// Zenerety 렌더링 방식에 사용되는 장면 탐색기입니다.
    /// </summary>
    public static TopGroup Target {
        get => _target;
        set {
            if (Framework.Running)
            {
                _target.Release();
                value.Prepare();
            }
            _target = value;
        }
    }

    internal static SDL.SDL_DisplayMode dm;

    static float fps = 60;
    /// <summary>
    /// 모니터의 픽셀 너비를 구합니다.
    /// </summary>
    public static int MonitorWidth => dm.w;
    /// <summary>
    /// 모니터의 픽셀 높이를 구합니다.
    /// </summary>
    public static int MonitorHeight => dm.h;
    /// <summary>
    /// 모니터의 주사율을 구합니다.
    /// </summary>
    public static int MonitorRefreshRate => dm.refresh_rate;

    internal static long framelatelimit = 166666;

    /// <summary>
    /// 초당 프레임을 제한합니다. 0을 할경우 모니터 주사율에 맞춥니다.
    /// 무한 프레임을 하고 싶다면 적당히 큰 수를 넣으면 됩니다.
    /// (주의) 프레임워크를 초기화 한뒤 사용해야합니다.
    /// </summary>
    public static float FrameLateLimit {
        get => fps;
        set {
            if ((fps = value) == 0) {
                if (dm.refresh_rate == 0) throw new JyunrcaeaFrameworkException("알수없는 디스플레이 정보");
                fps = dm.refresh_rate;
            }
            framelatelimit = (long)(1d / fps * 10000000);
        }
    }

    public static bool KeepRenderingWhenResize { get; internal set; }
}

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

/// <summary>
/// 모든 이벤트를 모아둔 인터페이스입니다.
/// </summary>
public interface AllEventInterface :
    Events.Resized,
    Events.WindowMove,
    Events.DropFile,
    Events.Update,
    Events.KeyDown,
    Events.MouseMove,
    Events.WindowQuit,
    Events.MouseKeyDown,
    Events.MouseKeyUp,
    Events.KeyUp,
    Events.WindowMaximized,
    Events.WindowMinimized,
    Events.WindowRestore,
    Events.KeyFocusIn,
    Events.KeyFocusOut,
    Events.MouseFocusIn,
    Events.MouseFocusOut,
    Events.InputText,
    Events.Prepare,
    Events.Release
{

}
/// <summary>
/// 프레임워크가 이벤트를 받았으때 실행될 함수들이 모인 클래스입니다.
/// </summary>
public class FrameworkFunction : ObjectInterface, AllEventInterface
{
    //Start와 비슷
    internal static void Prepare(Group group)
    {
        group.Prepare();
    }
    public virtual void Prepare()
    {
        Prepare(Display.Target);
    }

    //Stop, Free 와 비슷
    internal static void Release(Group group)
    {
        group.Release();
    }
    public virtual void Release()
    {
        Release(Display.Target);
    }

    /// <summary>
    /// 'Framework.Run' 함수를 호출시 실행되는 함수입니다.
    /// </summary>
    public override void Start()
    {
        Display.Target.Prepare();
    }
    /// <summary>
    /// 'Framework.Stop' 함수를 호출시 실행되는 함수입니다.
    /// </summary>
    public override void Stop()
    {
        Display.Target.Release();
    }

    /// <summary>
    /// 창의 크기가 조절될경우 실행되는 함수입니다.
    /// (창 크기 조절이 완전히 끝날때 실행되진 않습니다.)
    /// 창이 처음 생성될때는 실행되지 않습니다.
    /// </summary>
    public override void Resize()
    {
        Framework.Positioning(Display.Target);
        Display.Target.Resize();
        Framework.Positioning(Display.Target);
    }

    internal static long endtime = 0;
    /// <summary>
    /// Rendering
    /// </summary>
    internal override void Draw()
    {
        if (endtime > Framework.frametimer.ElapsedTicks) {
            if (Framework.SavingPerformance && endtime > Framework.frametimer.ElapsedTicks + 2000) SDL.SDL_Delay(1);
            return;
        }

        Update(((updatems = Framework.frametimer.ElapsedTicks) - updatetime) * 0.0001f);

        Framework.RenderRange = Window.size;
        SDL.SDL_RenderSetViewport(Framework.renderer,ref Window.size);
        Framework.Rendering(Display.Target);

        SDL.SDL_RenderPresent(Framework.renderer);

        if (SDL.SDL_RenderSetViewport(Framework.renderer, ref Window.size) != 0) throw new JyunrcaeaFrameworkException($"SDL Error: {SDL.SDL_GetError()}");
        SDL.SDL_SetRenderDrawColor(Framework.renderer, Window.BackgroundColor.Red, Window.BackgroundColor.Green, Window.BackgroundColor.Blue, Window.BackgroundColor.Alpha);
        SDL.SDL_RenderClear(Framework.renderer);
        if (endtime <= Framework.frametimer.ElapsedTicks - Display.framelatelimit)
            endtime = Framework.frametimer.ElapsedTicks + Display.framelatelimit;
        else endtime += Display.framelatelimit;
    }

    internal static long updatetime = 0, updatems = 0;

    public virtual void Update(float ms)
    {
        Display.Target.ResetPosition(new(Window.Width , Window.Height));
        Framework.RenderRange = Window.size;
        Framework.Positioning(Display.Target);
        Display.Target.Update(ms);
        Animation.AnimationQueue.Update();
        Framework.RenderRange = Window.size;
        Framework.Positioning(Display.Target);
        updatetime = updatems;
    }
    /// <summary>
    /// 창 크기 조절이 완전히 끝날때 호출되는 함수입니다.
    /// </summary>
    public virtual void Resized()
    {
        for (int i = 0 ; i < Display.Target.EventManager.Resized.Count ; i++)
        {
            Display.Target.EventManager.Resized[i].Resized();
        }
    }

    public virtual void InputText()
    {
        //Thread t = new(() =>
        //{
        //    SDL.SDL_Delay(Input.Text.WaitTime);
        //    SDL.SDL_GetKeyboardState(out int r);
        //    SDL.SDL_Keycode key = (SDL.SDL_Keycode)r;
        //    if (key.HasFlag(SDL.SDL_Keycode.SDLK_BACKSPACE))
        //    {
        //        while(Input.Text.InputedText.Length > 0)
        //        {
        //            Input.Text
        //        }
        //    }
        //});
    }

    int winrestore, winmax, winmin;

    public virtual void WindowMaximized()
    {
        
    }
    public virtual void WindowMinimized()
    {

    }
    public virtual void WindowRestore()
    {

    }
    /// <summary>
    /// 창 위치가 조정될떄 호출되는 함수입니다.
    /// </summary>
    public virtual void WindowMove()
    {

    }

    static int iwq;
    /// <summary>
    /// 창 나가기 버튼을 클릭했을때 호출되는 함수입니다.
    /// </summary>
    public virtual void WindowQuit()
    {
        if (Window.FrameworkStopWhenClose) Framework.Stop();
    }
    /// <summary>
    /// 파일이 드래그 드롭될때 호출되는 함수입니다.
    /// </summary>
    /// <param name="filename"></param>
    public virtual void DropFile(string filename)
    {

    }

    static int ikd;
    /// <summary>
    /// 키보드의 특정 키가 눌렸을때 실행되는 함수입니다.
    /// </summary>
    /// <param name="e"></param>
    public virtual void KeyDown(Keycode e)
    {
        ikd = Display.Target.EventManager.KeyDown.Count;
        for (int i = 0 ; i < ikd ; i++)
        {
            Display.Target.EventManager.KeyDown[i].KeyDown(e);
        }
    }

    static int imm;
    /// <summary>
    /// 마우스가 움직일때 호출되는 함수입니다.
    /// </summary>
    public virtual void MouseMove()
    {
        SDL.SDL_GetMouseState(out Input.Mouse.position.x, out Input.Mouse.position.y);
        int len = Display.Target.EventManager.mouseMoves.Count;
        for (int i = 0 ; i < len ; i++)
        {
            Display.Target.EventManager.mouseMoves[i].MouseMove();
        }
    }

    static int imd, imu, iku;

    public virtual void MouseKeyDown(Input.Mouse.Key key)
    {
        int len = Display.Target.EventManager.mouseKeyDowns.Count;
        for (int i = 0 ; i < len ; i++)
        {
            Display.Target.EventManager.mouseKeyDowns[i].MouseKeyDown(key);
        }
    }

    public virtual void MouseKeyUp(Input.Mouse.Key key)
    {
        int len = Display.Target.EventManager.mouseKeyUps.Count;
        for (int i = 0 ; i < len ; i++)
        {
            Display.Target.EventManager.mouseKeyUps[i].MouseKeyUp(key);
        }
    }

    public virtual void KeyUp(Keycode key)
    {
        iku = Display.Target.EventManager.keyUp.Count;
        for (int i = 0 ; i < iku ; i++)
        {
            Display.Target.EventManager.keyUp[i].KeyUp(key);
        }
    }

    int kfi, kfo;

    public virtual void KeyFocusIn()
    {

    }

    public virtual void KeyFocusOut()
    {

    }

    int mfi, mfo;

    public virtual void MouseFocusIn()
    {

    }

    public virtual void MouseFocusOut()
    {

    }

    public virtual void DisplayChange()
    {
        if (Window.Fullscreen)
        {
            Window.Resize(Display.MonitorWidth, Display.MonitorHeight);
        }
        if (Display.FrameLateLimit == 0) Display.FrameLateLimit = 0;
    }
}
/// <summary>
/// 창을 생성할때 쓰일 창 옵션입니다.
/// </summary>
public struct WindowOption
{
    internal SDL.SDL_WindowFlags option;

    //public WindowOption() { option = SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN; }

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
/// <summary>
/// 프레임워크를 초기화 할때 쓰일 오디오 옵션입니다.
/// </summary>
public struct AudioOption
{
    internal int ch, cs, hz;
    internal bool trylow;

    public AudioOption(byte Channals = 8, bool TryLowChannals = true, int ChunkSize = 8192, int Hz = 48000)
    {
        trylow = TryLowChannals;
        ch = Channals;
        cs = ChunkSize;
        hz = Hz;
    }
}

public abstract class PlayableSound : IDisposable
{
    internal IntPtr sound;
    public abstract void Dispose();
    ~PlayableSound()
    {
        Dispose();
    }
}

public class Music : PlayableSound
{
    public Music(string FileName)
    {
        this.sound = SDL_mixer.Mix_LoadMUS(FileName);
        if (this.sound == IntPtr.Zero)
            throw new JyunrcaeaFrameworkException($"음악을 불러오는데 실패하였습니다. SDL mixer Error: {SDL_mixer.Mix_GetError()}");
    }

    public override void Dispose()
    {
        SDL_mixer.Mix_FreeMusic(this.sound);
        GC.SuppressFinalize(this);
    }
    /// <summary>
    /// 음악을 재생합니다.
    /// </summary>
    /// <param name="music">음악</param>
    /// <returns>성공시 true</returns>
    public static bool Play(Music music)
    {
        playingmusic = music;
        if (SDL_mixer.Mix_PlayMusic(music.sound, 1) == -1) return false;
        return true;
    }
    public bool Play() => Music.Play(this);

    public static bool Resume()
    {
        if (SDL_mixer.Mix_PlayingMusic() != 0) return false;
        SDL_mixer.Mix_ResumeMusic();
        return true;
    }

    static Music? playingmusic = null;
    public static Music? NowPlaying => playingmusic;

    /// <summary>
    /// 음악 제목을 가져옵니다. (시간이 다소 걸립니다.)
    /// 'PlayReady' 가 켜져있거나, 이 음악이 재생중일 경우 좀 더 빠르게 불러올수 있습니다.
    /// </summary>
    public string Title => SDL_mixer.Mix_GetMusicTitle(this.sound);

    public static void Skip()
    {
        SDL_mixer.Mix_HaltMusic();
    }

    public static bool Paused => SDL_mixer.Mix_PausedMusic() == 1;

    public static void Pause()
    {
        SDL_mixer.Mix_PauseMusic();
    }

    /// <summary>
    /// 원하는 시간대로 이동합니다.
    /// </summary>
    public static double NowTime { get { return NowPlaying == null ? -1 : SDL_mixer.Mix_GetMusicPosition(NowPlaying.sound); }
        set
        {
            if (SDL_mixer.Mix_SetMusicPosition(value) == -1) throw new JyunrcaeaFrameworkException("잘못된 위치");
        }
    }

    internal static void Finished()
    {
        if (SDL_mixer.Mix_PlayingMusic() == 0) return;
        if (NowPlaying != null) NowPlaying.Dispose();
        if (MusicFinished != null) MusicFinished();
    }

    public static FunctionWhenMusicFinished? MusicFinished = null;
}

public delegate Music? FunctionWhenMusicFinished();

/// <summary>
/// 객체의 기본이 되는 객체 인터페이스입니다. (사실 추상 클래스이긴 하지만...)
/// </summary>
public abstract class ObjectInterface
{
    public abstract void Resize();
    public abstract void Stop();
    internal abstract void Draw();
    public abstract void Start();
    public bool Hide = false;

    //public abstract int X { get; set; }
    //public abstract int Y { get; set; }
}

public interface DefaultObjectPositionInterface
{
    public int X { get; set; }
    public int Y { get; set; }
}

/// <summary>
/// 이벤트 인터페이스가 모여있는 곳입니다.
/// (인터페이스 이름과 그 안에 있는 함수와 이름이 같습니다. 편하게 코딩하세요!)
/// </summary>
public class Events
{
    public interface Prepare
    {
        public void Prepare();
    }
    public interface Release
    {
        public void Release();
    }

    /// <summary>
    /// 글자를 입력함.
    /// </summary>
    public interface InputText
    {
        public void InputText();
    }

    /// <summary>
    /// 마우스 포커스를 잃음
    /// </summary>
    public interface MouseFocusOut
    {
        public void MouseFocusOut();
    }

    /// <summary>
    /// 마우스 포커스가 잡힘
    /// </summary>
    public interface MouseFocusIn
    {
        public void MouseFocusIn();
    }

    /// <summary>
    /// 키보드 포커스를 잃음
    /// </summary>
    public interface KeyFocusOut
    {
        public void KeyFocusOut();
    }

    /// <summary>
    /// 키보드 포커스가 잡힘
    /// </summary>
    public interface KeyFocusIn
    {
        public void KeyFocusIn();
    }

    /// <summary>
    /// 창에 파일을 끌어서 놓음
    /// </summary>
    public interface DropFile
    {
        public void DropFile(string filename);
    }

    /// <summary>
    /// 창 크기가 바뀜 (아직 조정중)
    /// </summary>
    public interface Resize
    {
        public void Resize();
    }

    /// <summary>
    /// 창 크기가 조정됨
    /// </summary>
    public interface Resized
    {
        public void Resized();
    }

    /// <summary>
    /// 업데이트 (렌더링과 같은 주기로 반복됨)
    /// </summary>
    public interface Update
    {
        public void Update(float millisecond);
    }

    /// <summary>
    /// 창이 이동됨
    /// </summary>
    public interface WindowMove
    {
        public void WindowMove();
    }

    /// <summary>
    /// 키보드에서 키를 누름
    /// </summary>
    public interface KeyDown
    {
        public void KeyDown(Keycode key);
    }

    /// <summary>
    /// 마우스 포인터가 움직임
    /// </summary>
    public interface MouseMove
    {
        public void MouseMove();
    }

    /// <summary>
    /// 창에서 나가기를 누름
    /// </summary>
    public interface WindowQuit
    {
        public void WindowQuit();
    }

    /// <summary>
    /// 마우스의 버튼이 눌림
    /// </summary>
    public interface MouseKeyDown
    {
        public void MouseKeyDown(Input.Mouse.Key key);
    }

    /// <summary>
    /// 마우스의 버튼이 완화됨
    /// </summary>
    public interface MouseKeyUp
    {
        public void MouseKeyUp(Input.Mouse.Key key);
    }

    /// <summary>
    /// 키보드에서 키가 완화됨
    /// </summary>
    public interface KeyUp
    {
        public void KeyUp(Keycode key);
    }

    /// <summary>
    /// 창이 최대화됨
    /// </summary>
    public interface WindowMaximized
    {
        public void WindowMaximized();
    }

    /// <summary>
    /// 창이 최소화됨
    /// </summary>
    public interface WindowMinimized
    {
        public void WindowMinimized();
    }

    /// <summary>
    /// 창이 복구됨
    /// </summary>
    public interface WindowRestore
    {
        public void WindowRestore();
    }

#if DEBUG
public abstract class ODDInterface
{
    internal abstract void ODD();
}
#endif
}

public delegate double FunctionForAnimation(double x);

public enum AnimationType : byte
{
    /// <summary>
    /// 시간에 비례해 이동합니다. (기본값)
    /// </summary>
    Normal = 0,
    /// <summary>
    /// 중간 지점과 가까워질수록 빨라집니다.
    /// 멀어질수록 느려집니다.
    /// </summary>
    Easing = 1,
    /// <summary>
    /// 처음에 느리게 시작합니다.
    /// 그리고 점점 빨라집니다.
    /// 마지막에 다다를수록 기하급수적으로 빨라집니다.
    /// </summary>
    Ease_In = 2,
    /// <summary>
    /// 처음에 빠르게 시작합니다.
    /// 그리고 점점 느려집니다.
    /// (Ease_In 를 반대로 하는것과 같습니다.)
    /// </summary>
    Ease_Out = 3,
    
    EaseInQuad = 4,
    EaseOutQuad = 5,
    EaseInOutQuad = 6,
}

/// <summary>
/// RGBA 형식으로 색을 표현하는 자료형입니다. 0~255 사이의 값을 담습니다.
/// </summary>
public class Color
{
    public Color(byte red=255,byte green=255,byte blue=255,byte alpha = 255)
    {
        this.colorbase = new()
        {
            r = red,
            g = green,
            b = blue,
            a = alpha
        };
    }

    public byte Red { get => this.colorbase.r; set => this.colorbase.r = value; }

    public byte Green { get => this.colorbase.g; set => this.colorbase.g = value; }

    public byte Blue { get => this.colorbase.b; set => this.colorbase.b = value; }

    public byte Alpha { get => this.colorbase.a; set => this.colorbase.a = value; }

    public Color Copy => new(this.Red, this.Green, this.Blue, this.Alpha);



    internal SDL.SDL_Color colorbase = new();

    //흑백 계열
    public static Color White => new(255, 255, 255);
    public static Color Black => new(0,0,0,255);
    public static Color Gray => new(127, 127, 127);
    public static Color Silver => new(192, 192, 192);
    public static Color DarkGray => new(63, 63, 63);
    //보라 계열
    public static Color Purple = new(128, 0, 128);
    public static Color Violet => new(127, 0, 255);
    public static Color Lilac => new(200, 162, 200);
    public static Color Lavender => new(230,230,255);
    //남보라 계열
    public static Color Periwinkle => new(128, 128, 255);
    public static Color LightPeriwinkle => new(204, 204, 255);
    //남색 계열
    public static Color Indigo => new(75, 0, 130);
    public static Color Navy => new(0, 0, 128);
    //초록 계열
    public static Color Lime => new(0, 255, 0);
    public static Color DarkGreen => new(0, 128, 0);
    //연두 계열
    public static Color YellowGreen => new (154, 205, 50);
    public static Color GreenYellow = new (173, 255, 47);
    public static Color Chartreuse => new(127, 255, 0);
    public static Color GrassGreen => new (117, 166, 74);
    public static Color YellowishGreen => new (160, 176, 54);
    //청록 계열
    //추가 예정
    //노랑 계열
    public static Color Yellow => new(255, 255, 0);
    public static Color Turbo = new(255, 204, 33);
    public static Color MoonYellow => new (240, 196, 32);
    public static Color VividYellow => new (255, 227, 2);
    public static Color GoldenYellow => new (255, 140, 0);
    //주황 계열
    public static Color Orange => new(255, 127, 0);
    public static Color DarkOrange => new (255, 140, 0);
}

/// <summary>
/// 입력과 관련된 대부분의 클래스가 있습니다.
/// </summary>
public static class Input
{
    

    /// <summary>
    /// 마우스와 관련된 클래스입니다.
    /// </summary>
    public static class Mouse
    {
        /// <summary>
        /// 창 포커스를 얻기위해 마우스를 클릭했을때 생긴 입력 이벤트를 차단할지에 대한 여부입니다.
        /// </summary>
        public static bool BlockEventAtToFocus
        {
            get => SDL.SDL_GetHint(SDL.SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH) == "1";

            set => SDL.SDL_SetHint(SDL.SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH, value ? "1":"0");
        }

        internal static SDL.SDL_Point position = new();

        public static int X => position.x;

        public static int Y => position.y;

        static bool cursorhide = false;
        

        /// <summary>
        /// 마우스를 창 밖으로 나오지 못하게 가둘지에 대한 여부입니다.
        /// </summary>
        public static bool Grab
        {
            set
            {
                SDL.SDL_SetWindowMouseGrab(Framework.window, value ? SDL.SDL_bool.SDL_TRUE: SDL.SDL_bool.SDL_FALSE);
            }
            get => SDL.SDL_GetWindowMouseGrab(Framework.window) == SDL.SDL_bool.SDL_TRUE;
        }

        /// <summary>
        /// 커서를 숨길지에 대한 여부입니다.
        /// </summary>
        public static bool HideCursor
        {
            get => cursorhide;
            set
            {
                SDL.SDL_ShowCursor((cursorhide = value) ? 0 : 1);
            }
        }

        /// <summary>
        /// 커서가 숨겨져 있을때 창 테두리를 조작할수 있게 할지에 대한 여부입니다. (창 조절, 이동 등)
        /// </summary>
        public static bool BlockWindowFrame
        {
            set
            {
                SDL.SDL_SetHint(SDL.SDL_HINT_WINDOW_FRAME_USABLE_WHILE_CURSOR_HIDDEN, value ? "0" : "1");
            }
            get => SDL.SDL_GetHint(SDL.SDL_HINT_WINDOW_FRAME_USABLE_WHILE_CURSOR_HIDDEN) == "0";
        }

        /// <summary>
        /// 마우스 버튼 목록
        /// </summary>
        public enum Key : byte
        {
            /// <summary>
            /// 왼쪽
            /// </summary>
            Left = 1,
            /// <summary>
            /// 중간 (마우스 휠)
            /// </summary>
            Middle = 2,
            /// <summary>
            /// 오른쪽
            /// </summary>
            Right = 3
        }

        public static void SetCursor(CursorType t)
        {
            SDL.SDL_SetCursor(SDL.SDL_CreateSystemCursor((SDL.SDL_SystemCursor)t));
        }

        public enum CursorType
        {
            Arrow,    // Arrow
            Ibeam,    // I-beam
            Wait,     // Wait
            Crosshair,    // Crosshair
            WaitArrow,    // Small wait cursor (or Wait if not available)
            SIZENWSE, // Double arrow pointing northwest and southeast
            SIZENESW, // Double arrow pointing northeast and southwest
            HorizonSizing,   // Double arrow pointing west and east
            VericalSizing,   // Double arrow pointing north and south
            Move,  // Four pointed arrow pointing north, south, east, and west
            NO,       // Slashed circle or crossbones
            HAND,     // Hand
            SYSTEM_CURSORS
        }
    }
    /// <summary>
    /// 텍스트 입력과 관련된 클래스입니다.
    /// 한국어 및 여러 문자들을 입력받기 위한 기능이 존재합니다.
    /// (0.8 이후부터 지원될 예정입니다.)
    /// </summary>
    public static class Text
    {
        public static string InputedText = string.Empty;

        public static int CursorPosition = 0;
        public static int SelectionLenght = 0;
        public static string SelectedText = string.Empty;

        public static uint WaitTime = 614;
        public static uint RemoveRepeatTime = 100;
        public static bool Removing { get; internal set; } = false;

        /// <summary>
        /// 텍스트 입력 활성/비활성
        /// </summary>
        internal static bool ti = false;
        [Obsolete("아직 구현되지 않은 기능")]
        public static bool Enable
        {
            get => ti;
            set
            {
                if (ti = value) SDL.SDL_StartTextInput();
                else SDL.SDL_StopTextInput();
            }
        }

        /// <summary>
        /// 텍스트가 입력되는 동안 KeyDown/KeyUp 이벤트를 무시합니다. (아직 구현되지 않음, 다음 업데이트를 위해 미리 생성)
        /// </summary>
        [Obsolete("미구현")]
        public static bool BlockKeyEvent{ get; set; } = false;
    }
}

/// <summary>
/// 키보드 키코드
/// </summary>
public enum Keycode
{
    UNKNOWN = 0,

    RETURN = '\r',
    ESCAPE = 27,
    BACKSPACE = '\b',
    TAB = '\t',
    SPACE = ' ',
    EXCLAIM = '!',
    QUOTEDBL = '"',
    HASH = '#',
    PERCENT = '%',
    DOLLAR = '$',
    AMPERSAND = '&',
    QUOTE = '\'',
    LEFTPAREN = '(',
    RIGHTPAREN = ')',
    ASTERISK = '*',
    PLUS = '+',
    COMMA = ',',
    MINUS = '-',
    PERIOD = '.',
    SLASH = '/',
    _0 = '0',
    _1 = '1',
    _2 = '2',
    _3 = '3',
    _4 = '4',
    _5 = '5',
    _6 = '6',
    _7 = '7',
    _8 = '8',
    _9 = '9',
    COLON = ':',
    SEMICOLON = ';',
    LESS = '<',
    EQUALS = '=',
    GREATER = '>',
    QUESTION = '?',
    AT = '@',
    LEFTBRACKET = '[',
    BACKSLASH = '\\',
    RIGHTBRACKET = ']',
    CARET = '^',
    UNDERSCORE = '_',
    BACKQUOTE = '`',
    A = 'a',
    B = 'b',
    C = 'c',
    D = 'd',
    E = 'e',
    F = 'f',
    G = 'g',
    H = 'h',
    I = 'i',
    J = 'j',
    K = 'k',
    L = 'l',
    M = 'm',
    N = 'n',
    O = 'o',
    P = 'p',
    Q = 'q',
    R = 'r',
    S = 's',
    T = 't',
    U = 'u',
    V = 'v',
    W = 'w',
    X = 'x',
    Y = 'y',
    Z = 'z',

    CAPSLOCK = SDL.SDL_Keycode.SDLK_CAPSLOCK,

    F1 = SDL.SDL_Keycode.SDLK_F1,
    F2 = SDL.SDL_Keycode.SDLK_F2,
    F3 = SDL.SDL_Keycode.SDLK_F3,
    F4 = SDL.SDL_Keycode.SDLK_F4,
    F5 = SDL.SDL_Keycode.SDLK_F5,
    F6 = SDL.SDL_Keycode.SDLK_F6,
    F7 = SDL.SDL_Keycode.SDLK_F7,
    F8 = SDL.SDL_Keycode.SDLK_F8,
    F9 = SDL.SDL_Keycode.SDLK_F9,
    F10 = SDL.SDL_Keycode.SDLK_F10,
    F11 = SDL.SDL_Keycode.SDLK_F11,
    F12 = SDL.SDL_Keycode.SDLK_F12,

    PRINTSCREEN = SDL.SDL_Keycode.SDLK_PRINTSCREEN,
    SCROLLLOCK = SDL.SDL_Keycode.SDLK_SCROLLLOCK,
    PAUSE = SDL.SDL_Keycode.SDLK_PAUSE,
    INSERT = SDL.SDL_Keycode.SDLK_INSERT,
    HOME = SDL.SDL_Keycode.SDLK_HOME,
    PAGEUP = SDL.SDL_Keycode.SDLK_PAGEUP,
    DELETE = 127,
    END = SDL.SDL_Keycode.SDLK_END,
    PAGEDOWN = SDL.SDL_Keycode.SDLK_PAGEDOWN,
    RIGHT = SDL.SDL_Keycode.SDLK_RIGHT,
    LEFT = SDL.SDL_Keycode.SDLK_LEFT,
    DOWN = SDL.SDL_Keycode.SDLK_DOWN,
    UP = SDL.SDL_Keycode.SDLK_UP,

    NUMLOCKCLEAR = SDL.SDL_Keycode.SDLK_NUMLOCKCLEAR,
    // KP_DIVIDE = (int)SDL_Scancode.SDL_SCANCODE_KP_DIVIDE |  SCANCODE_MASK,
    // KP_MULTIPLY = (int)SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY |  SCANCODE_MASK,
    // KP_MINUS = (int)SDL_Scancode.SDL_SCANCODE_KP_MINUS |  SCANCODE_MASK,
    KP_PLUS = SDL.SDL_Keycode.SDLK_KP_PLUS,
    NUM_ENTER = SDL.SDL_Keycode.SDLK_KP_ENTER,
    NUM_1 = SDL.SDL_Keycode.SDLK_KP_1,
    NUM_2 = SDL.SDL_Keycode.SDLK_KP_2,
    NUM_3 = SDL.SDL_Keycode.SDLK_KP_3,
    NUM_4 = SDL.SDL_Keycode.SDLK_KP_4,
    NUM_5 = SDL.SDL_Keycode.SDLK_KP_5,
    NUM_6 = SDL.SDL_Keycode.SDLK_KP_6,
    NUM_7 = SDL.SDL_Keycode.SDLK_KP_7,
    NUM_8 = SDL.SDL_Keycode.SDLK_KP_8,
    NUM_9 = SDL.SDL_Keycode.SDLK_KP_9,
    NUM_0 = SDL.SDL_Keycode.SDLK_KP_0,
    // KP_PERIOD = (int)SDL_Scancode.SDL_SCANCODE_KP_PERIOD |  SCANCODE_MASK,

    // APPLICATION = (int)SDL_Scancode.SDL_SCANCODE_APPLICATION |  SCANCODE_MASK,
    POWER = SDL.SDL_Keycode.SDLK_POWER,
    // KP_EQUALS = (int)SDL_Scancode.SDL_SCANCODE_KP_EQUALS |  SCANCODE_MASK,
    // F13 = (int)SDL_Scancode.SDL_SCANCODE_F13 |  SCANCODE_MASK,
    // F14 = (int)SDL_Scancode.SDL_SCANCODE_F14 |  SCANCODE_MASK,
    // F15 = (int)SDL_Scancode.SDL_SCANCODE_F15 |  SCANCODE_MASK,
    // F16 = (int)SDL_Scancode.SDL_SCANCODE_F16 |  SCANCODE_MASK,
    // F17 = (int)SDL_Scancode.SDL_SCANCODE_F17 |  SCANCODE_MASK,
    // F18 = (int)SDL_Scancode.SDL_SCANCODE_F18 |  SCANCODE_MASK,
    // F19 = (int)SDL_Scancode.SDL_SCANCODE_F19 |  SCANCODE_MASK,
    // F20 = (int)SDL_Scancode.SDL_SCANCODE_F20 |  SCANCODE_MASK,
    // F21 = (int)SDL_Scancode.SDL_SCANCODE_F21 |  SCANCODE_MASK,
    // F22 = (int)SDL_Scancode.SDL_SCANCODE_F22 |  SCANCODE_MASK,
    // F23 = (int)SDL_Scancode.SDL_SCANCODE_F23 |  SCANCODE_MASK,
    // F24 = (int)SDL_Scancode.SDL_SCANCODE_F24 |  SCANCODE_MASK,
    // EXECUTE = (int)SDL_Scancode.SDL_SCANCODE_EXECUTE |  SCANCODE_MASK,
    // HELP = (int)SDL_Scancode.SDL_SCANCODE_HELP |  SCANCODE_MASK,
    // MENU = (int)SDL_Scancode.SDL_SCANCODE_MENU |  SCANCODE_MASK,
    // SELECT = (int)SDL_Scancode.SDL_SCANCODE_SELECT |  SCANCODE_MASK,
    // STOP = (int)SDL_Scancode.SDL_SCANCODE_STOP |  SCANCODE_MASK,
    // AGAIN = (int)SDL_Scancode.SDL_SCANCODE_AGAIN |  SCANCODE_MASK,
    // UNDO = (int)SDL_Scancode.SDL_SCANCODE_UNDO |  SCANCODE_MASK,
    // CUT = (int)SDL_Scancode.SDL_SCANCODE_CUT |  SCANCODE_MASK,
    // COPY = (int)SDL_Scancode.SDL_SCANCODE_COPY |  SCANCODE_MASK,
    // PASTE = (int)SDL_Scancode.SDL_SCANCODE_PASTE |  SCANCODE_MASK,
    // FIND = (int)SDL_Scancode.SDL_SCANCODE_FIND |  SCANCODE_MASK,
    // MUTE = (int)SDL_Scancode.SDL_SCANCODE_MUTE |  SCANCODE_MASK,
    // VOLUMEUP = (int)SDL_Scancode.SDL_SCANCODE_VOLUMEUP |  SCANCODE_MASK,
    // VOLUMEDOWN = (int)SDL_Scancode.SDL_SCANCODE_VOLUMEDOWN |  SCANCODE_MASK,
    // KP_COMMA = (int)SDL_Scancode.SDL_SCANCODE_KP_COMMA |  SCANCODE_MASK,
    // KP_EQUALSAS400 =
    //(int)SDL_Scancode.SDL_SCANCODE_KP_EQUALSAS400 |  SCANCODE_MASK,

    // ALTERASE = (int)SDL_Scancode.SDL_SCANCODE_ALTERASE |  SCANCODE_MASK,
    // SYSREQ = (int)SDL_Scancode.SDL_SCANCODE_SYSREQ |  SCANCODE_MASK,
    // CANCEL = (int)SDL_Scancode.SDL_SCANCODE_CANCEL |  SCANCODE_MASK,
    // CLEAR = (int)SDL_Scancode.SDL_SCANCODE_CLEAR |  SCANCODE_MASK,
    // PRIOR = (int)SDL_Scancode.SDL_SCANCODE_PRIOR |  SCANCODE_MASK,
    // RETURN2 = (int)SDL_Scancode.SDL_SCANCODE_RETURN2 |  SCANCODE_MASK,
    // SEPARATOR = (int)SDL_Scancode.SDL_SCANCODE_SEPARATOR |  SCANCODE_MASK,
    // OUT = (int)SDL_Scancode.SDL_SCANCODE_OUT |  SCANCODE_MASK,
    // OPER = (int)SDL_Scancode.SDL_SCANCODE_OPER |  SCANCODE_MASK,
    // CLEARAGAIN = (int)SDL_Scancode.SDL_SCANCODE_CLEARAGAIN |  SCANCODE_MASK,
    // CRSEL = (int)SDL_Scancode.SDL_SCANCODE_CRSEL |  SCANCODE_MASK,
    // EXSEL = (int)SDL_Scancode.SDL_SCANCODE_EXSEL |  SCANCODE_MASK,

    // KP_00 = (int)SDL_Scancode.SDL_SCANCODE_KP_00 |  SCANCODE_MASK,
    // KP_000 = (int)SDL_Scancode.SDL_SCANCODE_KP_000 |  SCANCODE_MASK,
    // THOUSANDSSEPARATOR =
    //(int)SDL_Scancode.SDL_SCANCODE_THOUSANDSSEPARATOR |  SCANCODE_MASK,
    // DECIMALSEPARATOR =
    //(int)SDL_Scancode.SDL_SCANCODE_DECIMALSEPARATOR |  SCANCODE_MASK,
    // CURRENCYUNIT = (int)SDL_Scancode.SDL_SCANCODE_CURRENCYUNIT |  SCANCODE_MASK,
    // CURRENCYSUBUNIT =
    //(int)SDL_Scancode.SDL_SCANCODE_CURRENCYSUBUNIT |  SCANCODE_MASK,
    // KP_LEFTPAREN = (int)SDL_Scancode.SDL_SCANCODE_KP_LEFTPAREN |  SCANCODE_MASK,
    // KP_RIGHTPAREN = (int)SDL_Scancode.SDL_SCANCODE_KP_RIGHTPAREN |  SCANCODE_MASK,
    // KP_LEFTBRACE = (int)SDL_Scancode.SDL_SCANCODE_KP_LEFTBRACE |  SCANCODE_MASK,
    // KP_RIGHTBRACE = (int)SDL_Scancode.SDL_SCANCODE_KP_RIGHTBRACE |  SCANCODE_MASK,
    // KP_TAB = (int)SDL_Scancode.SDL_SCANCODE_KP_TAB |  SCANCODE_MASK,
    // KP_BACKSPACE = (int)SDL_Scancode.SDL_SCANCODE_KP_BACKSPACE |  SCANCODE_MASK,
    // KP_A = (int)SDL_Scancode.SDL_SCANCODE_KP_A |  SCANCODE_MASK,
    // KP_B = (int)SDL_Scancode.SDL_SCANCODE_KP_B |  SCANCODE_MASK,
    // KP_C = (int)SDL_Scancode.SDL_SCANCODE_KP_C |  SCANCODE_MASK,
    // KP_D = (int)SDL_Scancode.SDL_SCANCODE_KP_D |  SCANCODE_MASK,
    // KP_E = (int)SDL_Scancode.SDL_SCANCODE_KP_E |  SCANCODE_MASK,
    // KP_F = (int)SDL_Scancode.SDL_SCANCODE_KP_F |  SCANCODE_MASK,
    // KP_XOR = (int)SDL_Scancode.SDL_SCANCODE_KP_XOR |  SCANCODE_MASK,
    // KP_POWER = (int)SDL_Scancode.SDL_SCANCODE_KP_POWER |  SCANCODE_MASK,
    // KP_PERCENT = (int)SDL_Scancode.SDL_SCANCODE_KP_PERCENT |  SCANCODE_MASK,
    // KP_LESS = (int)SDL_Scancode.SDL_SCANCODE_KP_LESS |  SCANCODE_MASK,
    // KP_GREATER = (int)SDL_Scancode.SDL_SCANCODE_KP_GREATER |  SCANCODE_MASK,
    // KP_AMPERSAND = (int)SDL_Scancode.SDL_SCANCODE_KP_AMPERSAND |  SCANCODE_MASK,
    // KP_DBLAMPERSAND =
    //(int)SDL_Scancode.SDL_SCANCODE_KP_DBLAMPERSAND |  SCANCODE_MASK,
    // KP_VERTICALBAR =
    //(int)SDL_Scancode.SDL_SCANCODE_KP_VERTICALBAR |  SCANCODE_MASK,
    // KP_DBLVERTICALBAR =
    //(int)SDL_Scancode.SDL_SCANCODE_KP_DBLVERTICALBAR |  SCANCODE_MASK,
    // KP_COLON = (int)SDL_Scancode.SDL_SCANCODE_KP_COLON |  SCANCODE_MASK,
    // KP_HASH = (int)SDL_Scancode.SDL_SCANCODE_KP_HASH |  SCANCODE_MASK,
    // KP_SPACE = (int)SDL_Scancode.SDL_SCANCODE_KP_SPACE |  SCANCODE_MASK,
    // KP_AT = (int)SDL_Scancode.SDL_SCANCODE_KP_AT |  SCANCODE_MASK,
    // KP_EXCLAM = (int)SDL_Scancode.SDL_SCANCODE_KP_EXCLAM |  SCANCODE_MASK,
    // KP_MEMSTORE = (int)SDL_Scancode.SDL_SCANCODE_KP_MEMSTORE |  SCANCODE_MASK,
    // KP_MEMRECALL = (int)SDL_Scancode.SDL_SCANCODE_KP_MEMRECALL |  SCANCODE_MASK,
    // KP_MEMCLEAR = (int)SDL_Scancode.SDL_SCANCODE_KP_MEMCLEAR |  SCANCODE_MASK,
    // KP_MEMADD = (int)SDL_Scancode.SDL_SCANCODE_KP_MEMADD |  SCANCODE_MASK,
    // KP_MEMSUBTRACT =
    //(int)SDL_Scancode.SDL_SCANCODE_KP_MEMSUBTRACT |  SCANCODE_MASK,
    // KP_MEMMULTIPLY =
    //(int)SDL_Scancode.SDL_SCANCODE_KP_MEMMULTIPLY |  SCANCODE_MASK,
    // KP_MEMDIVIDE = (int)SDL_Scancode.SDL_SCANCODE_KP_MEMDIVIDE |  SCANCODE_MASK,
    // KP_PLUSMINUS = (int)SDL_Scancode.SDL_SCANCODE_KP_PLUSMINUS |  SCANCODE_MASK,
    // KP_CLEAR = (int)SDL_Scancode.SDL_SCANCODE_KP_CLEAR |  SCANCODE_MASK,
    // KP_CLEARENTRY = (int)SDL_Scancode.SDL_SCANCODE_KP_CLEARENTRY |  SCANCODE_MASK,
    // KP_BINARY = (int)SDL_Scancode.SDL_SCANCODE_KP_BINARY |  SCANCODE_MASK,
    // KP_OCTAL = (int)SDL_Scancode.SDL_SCANCODE_KP_OCTAL |  SCANCODE_MASK,
    // KP_DECIMAL = (int)SDL_Scancode.SDL_SCANCODE_KP_DECIMAL |  SCANCODE_MASK,
    // KP_HEXADECIMAL =
    //(int)SDL_Scancode.SDL_SCANCODE_KP_HEXADECIMAL |  SCANCODE_MASK,

    LCTRL = SDL.SDL_Keycode.SDLK_LCTRL,
    LSHIFT = SDL.SDL_Keycode.SDLK_LSHIFT,
    LALT = SDL.SDL_Keycode.SDLK_LALT,
    LGUI = SDL.SDL_Keycode.SDLK_LGUI,
    RCTRL = SDL.SDL_Keycode.SDLK_RCTRL,
    RSHIFT = SDL.SDL_Keycode.SDLK_RSHIFT,
    RALT = SDL.SDL_Keycode.SDLK_RALT,
    RGUI = SDL.SDL_Keycode.SDLK_RGUI,

    // MODE = (int)SDL_Scancode.SDL_SCANCODE_MODE |  SCANCODE_MASK,

    // AUDIONEXT = (int)SDL_Scancode.SDL_SCANCODE_AUDIONEXT |  SCANCODE_MASK,
    // AUDIOPREV = (int)SDL_Scancode.SDL_SCANCODE_AUDIOPREV |  SCANCODE_MASK,
    // AUDIOSTOP = (int)SDL_Scancode.SDL_SCANCODE_AUDIOSTOP |  SCANCODE_MASK,
    // AUDIOPLAY = (int)SDL_Scancode.SDL_SCANCODE_AUDIOPLAY |  SCANCODE_MASK,
    // AUDIOMUTE = (int)SDL_Scancode.SDL_SCANCODE_AUDIOMUTE |  SCANCODE_MASK,
    // MEDIASELECT = (int)SDL_Scancode.SDL_SCANCODE_MEDIASELECT |  SCANCODE_MASK,
    // WWW = (int)SDL_Scancode.SDL_SCANCODE_WWW |  SCANCODE_MASK,
    // MAIL = (int)SDL_Scancode.SDL_SCANCODE_MAIL |  SCANCODE_MASK,
    // CALCULATOR = (int)SDL_Scancode.SDL_SCANCODE_CALCULATOR |  SCANCODE_MASK,
    // COMPUTER = (int)SDL_Scancode.SDL_SCANCODE_COMPUTER |  SCANCODE_MASK,
    // AC_SEARCH = (int)SDL_Scancode.SDL_SCANCODE_AC_SEARCH |  SCANCODE_MASK,
    // AC_HOME = (int)SDL_Scancode.SDL_SCANCODE_AC_HOME |  SCANCODE_MASK,
    // AC_BACK = (int)SDL_Scancode.SDL_SCANCODE_AC_BACK |  SCANCODE_MASK,
    // AC_FORWARD = (int)SDL_Scancode.SDL_SCANCODE_AC_FORWARD |  SCANCODE_MASK,
    // AC_STOP = (int)SDL_Scancode.SDL_SCANCODE_AC_STOP |  SCANCODE_MASK,
    // AC_REFRESH = (int)SDL_Scancode.SDL_SCANCODE_AC_REFRESH |  SCANCODE_MASK,
    // AC_BOOKMARKS = (int)SDL_Scancode.SDL_SCANCODE_AC_BOOKMARKS |  SCANCODE_MASK,

    BrightnessDown = SDL.SDL_Keycode.SDLK_BRIGHTNESSDOWN,
    BrightnessUp = SDL.SDL_Keycode.SDLK_BRIGHTNESSUP,
    // DISPLAYSWITCH = (int)SDL_Scancode.SDL_SCANCODE_DISPLAYSWITCH |  SCANCODE_MASK,
    // KBDILLUMTOGGLE =
    //(int)SDL_Scancode.SDL_SCANCODE_KBDILLUMTOGGLE |  SCANCODE_MASK,
    // KBDILLUMDOWN = (int)SDL_Scancode.SDL_SCANCODE_KBDILLUMDOWN |  SCANCODE_MASK,
    // KBDILLUMUP = (int)SDL_Scancode.SDL_SCANCODE_KBDILLUMUP |  SCANCODE_MASK,
    // EJECT = (int)SDL_Scancode.SDL_SCANCODE_EJECT |  SCANCODE_MASK,
    // SLEEP = (int)SDL_Scancode.SDL_SCANCODE_SLEEP |  SCANCODE_MASK,
    // APP1 = (int)SDL_Scancode.SDL_SCANCODE_APP1 |  SCANCODE_MASK,
    // APP2 = (int)SDL_Scancode.SDL_SCANCODE_APP2 |  SCANCODE_MASK,

    // AUDIOREWIND = (int)SDL_Scancode.SDL_SCANCODE_AUDIOREWIND |  SCANCODE_MASK,
    // AUDIOFASTFORWARD = (int)SDL_Scancode.SDL_SCANCODE_AUDIOFASTFORWARD |  SCANCODE_MASK
}

/// <summary>
/// 수학 공식을 까먹은 당신을 위해... 편리한 기능을 제공하는 함수들이 모여있습니다.
/// </summary>
public static class Convenience
{
    /// <summary>
    /// 두 객체가 서로 겹치는 부분이 있는지 (닿았는지) 판단합니다. (직사각형 기준)
    /// </summary>
    /// <param name="sp1">첫번째 객체</param>
    /// <param name="sp2">두번째 객체</param>
    /// <returns>겹친 부분이 있을경우 True 를 반환합니다.</returns>
    public static bool Overlap(DrawableObject sp1,DrawableObject sp2)
    {
        return SDL.SDL_IntersectRect(ref sp1.renderposition, ref sp2.renderposition , out _) == SDL.SDL_bool.SDL_TRUE;
    }

    public static bool MouseOver(DrawableObject Target)
    {
        if (Target is Circle)
        {
            return Math.Sqrt(Math.Pow((Target.Rx - Input.Mouse.X), 2) + Math.Pow((Target.Ry - Input.Mouse.Y), 2)) <= ((Circle)Target).Radius;
        }
        return SDL.SDL_PointInRect(ref Input.Mouse.position, ref Target.renderposition) == SDL.SDL_bool.SDL_TRUE;
    }

    /// <summary>
    /// 두 객체의 거리를 구합니다.
    /// </summary>
    /// <param name="sp1">첫번째 객체</param>
    /// <param name="sp2">두번째 객체</param>
    /// <returns>거리</returns>
    public static double Distance(DrawableObject sp1, DrawableObject sp2)
    {
        int x = sp1.renderposition.x - sp2.renderposition.x, y = sp1.renderposition.y - sp2.renderposition.y;
        return Math.Sqrt(x * x + y * y);
    }
}

/// <summary>
/// 너비와 높이를 얻을수 있는 객체에게만 상속되는 인터페이스입니다.
/// </summary>
public interface CanGetLenght
{
    public int Width { get; }
    public int Height { get; }
}


/// <summary>
/// 부드러운 움직임을 구현하기 위한 편리한 기능이 모여있습니다.
/// </summary>
public static class Animation
{
    internal class LinkedListForAnimation : LinkedList<Info.General>
    {
        public void Add(Info.General info)
        {
            if (info.EndTime <= Framework.RunningTime)
            {
                info.Done();
                return;
            }
            //this.AddLast(info);
            AddTarget.Enqueue(info);
        }

        Queue<Info.General> AddTarget = new();
        Queue<Info.General> RemoveTarget = new();

        public new void Remove(Info.General info)
        {
            RemoveTarget.Enqueue(info);
        }

        public void Update()
        {
            while (AddTarget.Count != 0) base.AddLast(AddTarget.Dequeue());
            foreach (var a in AnimationQueue) if (a is not null) lock (a) { a.Update(); }
            while(RemoveTarget.Count!=0) base.Remove(RemoveTarget.Dequeue());
        }
    }

    internal static LinkedListForAnimation AnimationQueue = new();

    public static void Add(Info.General info)
    {
        AnimationQueue.Add(info);
    }

    public static void Add(InfoForGroup.General group)
    {
        group.Add();
    }

    /// <summary>
    /// 특정한 애니메이션 정보를 담는 클래스가 모여있습니다.
    /// </summary>
    public static class Info
    {
        /// <summary>
        /// 기본적인 애니메이션 정보를 담는 클래스입니다.
        /// </summary>
        public abstract class General
        {
            public General(BaseObject zo,double? st,double animatime,uint RepeatCount = 1,Action<BaseObject>? fff = null,FunctionForAnimation? ffa = null)
            {
                Target = zo;
                FunctionForFinish = fff;
                this.RepeatCount = RepeatCount;
                if (ffa is not null) AnimationCalculator = ffa;
                StartTime = st is null ? Framework.RunningTime : (double)st;
                AnimationTime = animatime;
            }

            public BaseObject Target { get; internal set; } = null!;
            public double StartTime { get; internal set; }
            public double EndTime { get; internal set; }
            double animatime;
            public double AnimationTime { get => animatime; set
                {
                    animatime = value;
                    this.EndTime = this.StartTime + value;
                }
            }
            public bool Finished { get; internal set; } = false;
            public Action<BaseObject>? FunctionForFinish { get; internal set; } = null;
            public FunctionForAnimation AnimationCalculator { get; internal set; } = Animation.Type.Default;
            public uint RepeatCount = 0;

            internal double Progress = 0d;

            internal bool CheckTime()
            {
                Progress = Framework.RunningTime;
                if (Progress <= StartTime) return true;
                if (Progress >= EndTime)
                {
                    if (RepeatCount != 1)
                    {
                        this.StartTime = EndTime;
                        this.EndTime = StartTime + AnimationTime;
                        if (RepeatCount != 0) RepeatCount--;
                        Progress = AnimationCalculator((Progress - StartTime) / AnimationTime);
                        return false;
                    }
                    Done();
                    return true;
                }
                Progress = AnimationCalculator((Progress - StartTime) / AnimationTime);
                return false;
            }

            internal abstract void Update();
            /// <summary>
            /// 애니메이션을 즉시 마무리 한뒤 끝내버립니다.
            /// </summary>
            public virtual void Done()
            {
                this.Finished = true;
                if (FunctionForFinish is not null) FunctionForFinish(Target);
                AnimationQueue.Remove(this);
            }

            /// <summary>
            /// 애니메이션을 마무리 없이 즉시 종료합니다.
            /// </summary>
            /// <param name="CallFinishFunction">애니메이션 완료시 호출해야될 함수 호출 여부 </param>
            public virtual void Stop(bool CallFinishFunction = false)
            {
                this.Finished = true;
                if (CallFinishFunction && FunctionForFinish is not null) FunctionForFinish(Target);
                AnimationQueue.Remove(this);
            }

            /// <summary>
            /// 애니메이션을 취소합니다. (이전 상태로 되돌립니다.)
            /// </summary>
            public virtual void Undo()
            {
                this.Finished = false;
            }

            /// <summary>
            /// 시간을 수정하여 애니메이션을 재개합니다. (애니메이션이 이미 종료된 상태여야 합니다.)
            /// </summary>
            /// <param name="StartTime">시작 시간</param>
            /// <param name="EndTime">종료 시간</param>
            /// <returns>재개 성공 여부</returns>
            [Obsolete("불안정")]
            public bool ResumeAt(double StartTime,double EndTime)
            {
                if (this.Finished is false) return false;
                this.Finished = false;
                this.StartTime = StartTime;
                this.EndTime = EndTime;
                AnimationQueue.Add(this);
                return true;
            }
        }
        /// <summary>
        /// 움직임과 관련된 정보를 담는 클래스
        /// </summary>
        public class Movement : General
        {
            /// <summary>
            /// 특정 대상을 원하는 (절대적) 위치로 부드럽게 움직입니다.
            /// </summary>
            /// <param name="Target">대상 (모든 Zenerety 렌더링 지원 객체)</param>
            /// <param name="X">이동할 X좌표</param>
            /// <param name="Y">이동할 Y좌표</param>
            /// <param name="StartTime">시작 시간 (null 일경우 현재 프레임워크 실행시간으로 설정 (즉시 시작))</param>
            /// <param name="AnimationTime">이동 시간</param>
            /// <param name="FunctionWhenFinished">완료되었을때 실행할 함수 (null 일경우 아무것도 하지 않음)</param>
            /// <param name="TimeClaculator">애니메이션 계산기 (null 일경우 기본)</param>
            /// <param name="RepeatCount">반복 횟수, 0일경우 무한</param>
            public Movement(BaseObject Target,int? X = null,int? Y = null,double? StartTime = null, double AnimationTime = 1000,uint RepeatCount = 1, FunctionForAnimation? TimeClaculator = null, Action<BaseObject>? FunctionWhenFinished = null) : base(Target,StartTime,AnimationTime,RepeatCount,FunctionWhenFinished,TimeClaculator) {
                BX = Target.X;
                BY = Target.Y;
                if (X is null)
                {
                    MX = false;
                } else
                {
                    MX = true;
                    AX = (int)X;
                    LX = AX - BX;
                }

                if (Y is null)
                {
                    MY = false;
                } else
                {
                    MY = true;
                    AY = (int)Y;
                    LY = AY - BY;
                }
            }

            bool MX, MY;
            int BX, BY, AX, AY, LX, LY;
            internal override void Update()
            {
                if (CheckTime()) return;
                if(MX) Target.X = BX + (int)(LX * Progress);
                if(MY) Target.Y = BY + (int)(LY * Progress);
            }

            
            public override void Done()
            {
                if(MX) Target.X = AX;
                if(MY) Target.Y = AY;
                base.Done();
            }

            public override void Undo()
            {
                if(MX) Target.X = BX;
                if(MY) Target.Y = BY;
                base.Done();
            }

            public void EditEndPoint(int? X,int? Y)
            {
                if (X is null)
                {
                    MX = false;
                }
                else
                {
                    MX = true;
                    AX = (int)X;
                    LX = AX - BX;
                }

                if (Y is null)
                {
                    MY = false;
                }
                else
                {
                    MY = true;
                    AY = (int)Y;
                    LY = AY - BY;
                }
                Update();
            }
        }

        /// <summary>
        /// 회전과 관련된 정보를 담는 클래스
        /// </summary>
        public class Rotation : General
        {
            public Rotation(ExtendDrawableObject Target,double Rotate,double? StartTime,double AnimationTime, uint RepeatCount = 1,  FunctionForAnimation? TimeClaculator = null, Action<BaseObject>? FunctionWhenFinished = null) : base(Target, StartTime, AnimationTime,RepeatCount, FunctionWhenFinished, TimeClaculator)
            {
                BR = Target.Rotation;
                RL = Rotate;
                AR = BR + RL;
            }

            double BR, RL, AR;

            internal override void Update()
            {
                if (CheckTime()) return;
                ((ExtendDrawableObject)Target).Rotation = BR + RL * Progress;
            }

            public override void Done()
            {
                ((ExtendDrawableObject)Target).Rotation = AR;
                base.Done();
            }

            public override void Undo()
            {
                ((ExtendDrawableObject)Target).Rotation = BR;
                base.Undo();
            }
        }

        /// <summary>
        /// 투명도와 관련된 정보를 담는 클래스
        /// </summary>
        public class Opacity : General
        {
            public Opacity(DrawableObject Target, byte TargetOpacity, double? StartTime = null, double AnimationTime = 1000, uint RepeatCount = 1, FunctionForAnimation? TimeCalculator = null, Action<BaseObject>? FunctionWhenFinished = null) : base(Target, StartTime, AnimationTime, RepeatCount, FunctionWhenFinished, TimeCalculator)
            {
                this.BO = Target.Opacity;
                this.AO = TargetOpacity;
                this.RO = (short)(AO - BO);
            }

            byte BO, AO;
            short RO;

            internal override void Update()
            {
                if (CheckTime()) return;
                ((DrawableObject)Target).Opacity = (byte)(BO + RO * Progress);
            }

            public override void Done()
            {
                ((DrawableObject)Target).Opacity = AO;
                base.Done();
            }

            public override void Undo()
            {
                ((DrawableObject)Target).Opacity = BO;
                base.Undo();
            }

            public virtual void Modify(byte opacity)
            {
                if (this.Finished)
                {
                    this.BO = ((Available.Opacity)this.Target).Opacity;
                }
                this.AO = opacity;
                this.RO = (short)(AO - BO);
            }

            public virtual double? ResetStartTime { set
                {
                    this.EndTime = (this.StartTime = value ?? Framework.RunningTime) + this.AnimationTime;
                }
            }
        }
    }

    /// <summary>
    /// 그룹용 애니메이션 정보를 담는 클래스가 모여있습니다.
    /// </summary>
    public static class InfoForGroup
    {
        public abstract class General
        {
            protected ObjectList targets;
            protected double starttime;
            protected double animationtime;

            public bool ApplySubGroup = false;

            /// <summary>
            /// 반복횟수 (기본 1회, 0 일경우 무제한)
            /// </summary>
            public uint RepeatCount = 1;
            /// <summary>
            /// 애니메이션 시간 계산기
            /// </summary>
            public FunctionForAnimation TimeCalculator = Animation.Type.Default;

            protected List<Info.General> ApplyTargets = new();

            public General(Group target, double? StartTime, double AnimationTime) {
                this.targets = target.Objects;
                this.starttime = StartTime ?? Framework.RunningTime;
                this.starttime = AnimationTime;
            }

            /// <summary>
            /// 시작 시간 (null 일경우 Framework.RunningTime 적용)
            /// </summary>
            public double? StartTime
            {
                get => starttime;
                set => starttime = (value ?? Framework.RunningTime);
            }
            /// <summary>
            /// 움직이는 시간 (밀리초 기준)
            /// </summary>
            public double AnimationTime
            {
                get => animationtime;
                set => animationtime = value;
            }

            internal virtual void Add()
            {
                if (this.ApplyTargets.Count != 0)
                {
                    this.ApplyTargets.Clear();
                }
            }

            /// <summary>
            /// 애니메이션을 즉시 마무리 한뒤 끝내버립니다.
            /// </summary>
            public void Done()
            {
                for (int i =0;i<this.ApplyTargets.Count;i++)
                {
                    this.ApplyTargets[i].Done();
                }
            }

            /// <summary>
            /// 애니메이션을 마무리 없이 즉시 종료합니다.
            /// </summary>
            public void Stop()
            {
                for (int i = 0; i < this.ApplyTargets.Count; i++)
                {
                    this.ApplyTargets[i].Stop();
                }
            }

            /// <summary>
            /// 애니메이션이 끝났는지에 대한 여부입니다.
            /// (적용된 객체가 하나도 없을경우 시간 상으로 계산됩니다.)
            /// </summary>
            public bool Finished => this.ApplyTargets.Count == 0 ? (this.starttime + this.animationtime <= Framework.RunningTime) : this.ApplyTargets[0].Finished;
        }

        /// <summary>
        /// 투명도에 대한 애니메이션 정보. (투명도 적용이 불가능한 객체는 제외됩니다.)
        /// </summary>
        public class Opacity : General
        {
            public byte TargetOpacity;

            public Opacity(Group target,byte opacity,double? StartTime=null,double AnimationTime=1000) : base(target,StartTime,AnimationTime)
            {
                this.TargetOpacity = opacity;
            }

            internal override void Add()
            {
                base.Add();
                AddOnGroup(this.targets);
            }

            internal virtual void AddOnGroup(ObjectList targets)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] is not DrawableObject)
                    {
                        if (this.ApplySubGroup && targets[i] is Group) AddOnGroup(((Group)targets[i]).Objects);
                        continue;
                    }
                    var ai = new Info.Opacity((DrawableObject)targets[i], this.TargetOpacity, this.starttime, this.animationtime, this.RepeatCount, this.TimeCalculator);
                    Animation.Add(ai);
                    this.ApplyTargets.Add(ai);
                }
            }
        }
    }

    public static class Type
    {
        public static double Default(double x) => x;

        public static double EaseInSine(double x)
        {
            return 1d - Math.Cos((x * Math.PI) * 0.5d);
        }

        public static double EaseOutSine(double x)
        {
            return Math.Sin((x * Math.PI) * 0.5d);
        }

        public static double EaseInOutSine(double x)
        {
            return -(Math.Cos(Math.PI * x) - 1d) * 0.5d;
        }

        public static double EaseInQuad(double x)
        {
            return x * x;
        }

        public static double EaseOutQuad(double x)
        {
            x = 1d - x;
            return 1 - x * x;
        }

        public static double EaseInOutQuad(double x)
        {
            return x < 0.5d ? 2d * x * x : 1d - Math.Pow(-2d * x + 2d, 2d) * 0.5d;
        }
    }

    public static class Available
    {
        public interface Opacity
        {
            public byte Opacity { get; set; }
        }

        public interface Size
        {
            public Size2D Size { get; set; }
        }


    }
}

public class RectSize
{
    internal SDL.SDL_Rect size;
    public int X { get => size.x; set => size.x = value; }
    public int Y { get => size.y; set => size.y = value; }
    public int Width { get => size.w; set => size.w = value; }
    public int Height { get => size.h; set => size.h = value; }
    public RectSize(int x = 0,int y = 0, int w = 0, int h = 0)
    {
        size = new() { x = x, y = y, w = w, h = h };
    }
}

public class EventForScheduler
{
    public Action Function { get; internal set; }
    public double StartTime { get; internal set; }

    public EventForScheduler(Action Function,double StartTime=0)
    {
        this.Function = Function;
        this.StartTime = Framework.RunningTime + StartTime;
    }
}

public class Scheduler
{
    public LinkedList<EventForScheduler> Queue { get; internal set; } = new();
    internal Queue<EventForScheduler> CallList = new();

    /// <summary>
    /// 곧 호출해야할 함수들을 호출리스트에 넣습니다.
    /// </summary>
    internal void Update()
    {
        
    }

    /// <summary>
    /// 
    /// </summary>
    internal void Refresh()
    {

    }
}

/// <summary>
/// 객체가 그릴수 있는 텍스쳐의 추상 클래스입니다.
/// </summary>
public abstract class DrawableTexture : IDisposable
{
    public bool FixedRenderRange = false;

    internal bool needresettexture = false;

    internal IntPtr texture;

    internal SDL.SDL_Rect src = new();

    internal SDL.SDL_Point absolutesrc = new();

    public int Width => absolutesrc.x;

    public int Height => absolutesrc.y;

    /// <summary>
    /// 원본 이미지 크기에 맞게 조절되고 있는지에 대한 여부입니다.
    /// RenderRange에 null 이외의 값을 넣을 경우 이 변수는 false가 됩니다.
    /// </summary>
    public bool AutoRange { get; internal set; } = true;

    internal byte alpha = 255;

    public byte Opacity
    {
        get
        {
            return alpha;
        }
        set
        {
            alpha = value;
            if (this.texture != IntPtr.Zero) SDL.SDL_SetTextureAlphaMod(texture, alpha);
        }
    }

    public void SetRenderRange(int x,int y,int width,int height)
    {
        src.x = x; src.y = y; src.w = width; src.h = height;
        needresettexture = true;
    }

    public RectSize? RenderRange
    {
        get { if (AutoRange) return null; return new(src.x, src.y, src.w, src.h); }
        set
        {
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
        if (this.texture == IntPtr.Zero) return;
        if (SDL.SDL_SetTextureBlendMode(this.texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND) < 0) throw new JyunrcaeaFrameworkException($"텍스쳐의 블랜더 모드 설정에 실패하였습니다. SDL Error: {SDL.SDL_GetError()}");
        if (alpha != 255) SDL.SDL_SetTextureAlphaMod(texture, alpha);
    }

    public virtual void Dispose()
    {
        SDL.SDL_DestroyTexture(texture);
    }


}

class EffectForImage
{
    internal static PaintOnMemory Bluring(ImageOnMemory image)
    {
        byte r, g, b, a;
        int maxw = image.Width - 1;
        int maxh = image.Height - 1;

        double[,] RedMap = new double[image.Width, image.Height];
        double[,] BlueMap = new double[image.Width, image.Height];
        double[,] GreenMap = new double[image.Width, image.Height];
        double[,] AlphaMap = new double[image.Width, image.Height];

        void Add(int x, int y, double n)
        {
            RedMap[x, y] += r * n;
            BlueMap[x, y] += b * n;
            GreenMap[x, y] += g * n;
            AlphaMap[x, y] += a * n;
        }

        //1단계
        void OneBlur(int x, int y)
        {
            image.GetRGBA(x, y, out r, out g, out b, out a);
            Add(x, y, 0.25);
            if (x != 0)
            {
                Add(x - 1, y, 0.125);
                if (y != 0)
                {
                    Add(x - 1, y - 1, 0.0625);
                }
                if (y != maxh)
                {
                    Add(x - 1, y + 1, 0.0625);
                }
            }
            if (y != 0)
            {
                Add(x, y - 1, 0.125);
            }
            if (x != maxw)
            {
                Add(x + 1, y, 0.125);
                if (y != 0)
                {
                    Add(x + 1, y - 1, 0.0625);
                }
                if (y != maxh)
                {
                    Add(x + 1, y + 1, 0.0625);
                }
            }
            if (y != maxh)
            {
                Add(x, y + 1, 0.125);
            }
        }

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                OneBlur(x, y);
            }
        }
        //가장자리 처리
        image.GetRGBA(1, 1, out r, out g, out b, out a);
        Add(0, 0, 0.4375);
        image.GetRGBA(maxw - 1, maxh - 1, out r, out g, out b, out a);
        Add(maxw, maxh, 0.4375);
        image.GetRGBA(1, maxh - 1, out r, out g, out b, out a);
        Add(0, maxh, 0.4375);
        image.GetRGBA(maxw - 1, 1, out r, out g, out b, out a);
        Add(maxw, 0, 0.4375);

        //가로 처리
        for (int x = 1; x < maxw; x++)
        {
            image.GetRGBA(x, 1, out r, out g, out b, out a);
            Add(x, 0, 0.25);
            image.GetRGBA(x, maxh - 1, out r, out g, out b, out a);
            Add(x, maxh, 0.25);
        }
        //세로 처리
        for (int y = 1; y < maxh; y++)
        {
            image.GetRGBA(1, y, out r, out g, out b, out a);
            Add(0, y, 0.25);
            image.GetRGBA(maxw - 1, y, out r, out g, out b, out a);
            Add(maxw, y, 0.25);
        }

        PaintOnMemory paint = new(image.Width, image.Height);

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                //점찍기
                paint.Point(x, y, (byte)RedMap[x, y], (byte)GreenMap[x, y], (byte)BlueMap[x, y], (byte)AlphaMap[x, y]);
            }
        }

        return paint;
    }
}

public class PaintOnMemory : IDisposable
{
    IntPtr surface;
    SDL.SDL_Surface sur;
    SDL.SDL_PixelFormat format;

    internal IntPtr Address => surface;

    public PaintOnMemory(int width = 0,int height = 0)
    {
        surface = SDL.SDL_CreateRGBSurfaceWithFormat(0, width, height, 32, SDL.SDL_PIXELFORMAT_ARGB8888);
        sur = System.Runtime.InteropServices.Marshal.PtrToStructure<SDL.SDL_Surface>(surface);
        format = System.Runtime.InteropServices.Marshal.PtrToStructure<SDL.SDL_PixelFormat>(sur.format);
        SDL.SDL_SetSurfaceBlendMode(surface, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
    }

    public unsafe void Point(int x,int y,Color color)
    {
        Point(x, y, color.colorbase.r, color.colorbase.g, color.colorbase.b, color.colorbase.a);
    }

    public unsafe void Point(int x, int y, byte r,byte g,byte b,byte a)
    {
        SDL.SDL_LockSurface(surface);
        byte* pixel_arr = (byte*)sur.pixels;
        pixel_arr[y * sur.pitch + x * format.BytesPerPixel + 0] = b;
        pixel_arr[y * sur.pitch + x * format.BytesPerPixel + 1] = g;
        pixel_arr[y * sur.pitch + x * format.BytesPerPixel + 2] = r;
        pixel_arr[y * sur.pitch + x * format.BytesPerPixel + 3] = a;
        SDL.SDL_UnlockSurface(surface);
    }

    public unsafe Color GetPixel(int x,int y)
    {
        uint key = *(UInt32*)((byte*)sur.pixels + y * sur.pitch + x * format.BytesPerPixel);
        Color color = new();
        SDL.SDL_GetRGBA(key, sur.format, out color.colorbase.r, out color.colorbase.g, out color.colorbase.b, out color.colorbase.a);
        return color;
    }

    public TextureFromMemory GetTexture()
    {
        return new(this.surface);
    }

    public void Rectanlge(int x,int y,int w,int h)
    {
        
    }

    public ImageOnMemory GetImage()
    {
        return new(SDL.SDL_DuplicateSurface(this.surface));
    }

    public void Dispose()
    {
        SDL.SDL_FreeSurface(surface);
        surface = IntPtr.Zero;
    }
}

public class ImageOnMemory : IDisposable
{
    SDL.SDL_Surface surface;
    IntPtr surface_ptr;

    public ImageOnMemory(IntPtr address)
    {
        Init(address);
    }

    private void Init(IntPtr address)
    {
        surface_ptr = address;
        if (surface_ptr == IntPtr.Zero) throw new JyunrcaeaFrameworkException("이미지를 불러오는데 실패하였습니다.");
        surface = System.Runtime.InteropServices.Marshal.PtrToStructure<SDL.SDL_Surface>(surface_ptr);
        format = System.Runtime.InteropServices.Marshal.PtrToStructure<SDL.SDL_PixelFormat>(surface.format);
        bpp = format.BytesPerPixel;
        unsafe
        {
            switch (bpp)
            {
                case 1:
                    pp = Process.One;
                    break;
                case 2:
                    pp = Process.Two;
                    break;
                case 3:
                    pp = Process.Three;
                    break;
                case 4:
                    pp = Process.Four;
                    break;
                default:
                    pp = Process.Default;
                    break;
            }
        }
    }

    private void Init(ImageOnMemory me)
    {
        this.surface = me.surface;
        this.surface_ptr = me.surface_ptr;
        this.pp = me.pp;
        this.format = me.format;
        this.bpp = me.bpp;
    }

    public ImageOnMemory(string path) : this(SDL_image.IMG_Load(path))
    {

    }

    public int Width => surface.w; public int Height => surface.h;

    Process.PixelProcesser pp = null!;

    SDL.SDL_PixelFormat format;
    int bpp;

    public unsafe UInt32 GetPixel(int x,int y)
    {
        return pp((byte*)surface.pixels + y * surface.pitch + x * bpp);
    }

    public void GetRGBA(int x,int y,out byte r,out byte g,out byte b,out byte a)
    {
        uint p = GetPixel(x, y);
        SDL.SDL_GetRGBA(p, surface.format, out r, out g, out b, out a);
    }

    public ImageOnMemory GetResizedImage(int width,int height)
    {
        IntPtr result = SDL.SDL_CreateRGBSurfaceWithFormat(0, width, height, 32, SDL.SDL_PIXELFORMAT_ARGB8888);
        SDL.SDL_Rect origin = new() { x = 0, y = 0, w = surface.w, h = surface.h };
        SDL.SDL_Rect targetsize = new() { x = 0, y = 0, w = width, h = height };
        SDL.SDL_LowerBlitScaled(surface_ptr, ref origin, result,ref targetsize);
        if (result == IntPtr.Zero) throw new JyunrcaeaFrameworkException("크기 조정에 실패하였습니다.");
        return new(result);
    }

    public int BlurProgress { get; internal set; } = -1;

    public void Resize(int width,int height)
    {
        var result = GetResizedImage(width, height);
        this.Release();
        this.Init(result);
    }

    public void Blur()
    {
        var result = EffectForImage.Bluring(this);
        this.Release();
        this.Init(result.GetImage());
        result.Dispose();
    }

    public TextureFromMemory GetTexture()
    {
        if (surface_ptr == IntPtr.Zero) throw new JyunrcaeaFrameworkException("실패. 불러오지 않은 이미지, 또는 이미 해제된 이미지를 텍스쳐로 변환할려고 했습니다.");
        return new TextureFromMemory(surface_ptr);
    }

    public void Dispose()
    {
        Release();
    }

    private void Release()
    {
        if (surface_ptr == IntPtr.Zero) return;
        SDL.SDL_FreeSurface(surface_ptr);
        surface_ptr = IntPtr.Zero;
    }

    private static unsafe class Process {
        internal delegate UInt32 PixelProcesser(byte* p);

        internal static UInt32 One(byte* p)
        {
            return *p;
        }

        internal static UInt32 Two(byte* p)
        {
            return *(UInt16*)p;
        }

        internal static UInt32 Three(byte* p)
        {
            return (UInt32)(p[0] | p[1] << 8 | p[2] << 16);
        }

        internal static UInt32 Four(byte* p)
        {
            return *(UInt32*)p;
        }

        internal unsafe static UInt32 Default(byte* p)
        {
            throw new JyunrcaeaFrameworkException("can't read pixel");
        }
    }
}

public class TextureFromMemory : DrawableTexture
{
    public TextureFromMemory(IntPtr surface)
    {
        this.texture = SDL.SDL_CreateTextureFromSurface(Framework.renderer,surface);
        if (this.texture == IntPtr.Zero)
        {
            throw new JyunrcaeaFrameworkException("이미지를 불러오는데 실패하였습니다.");
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

    public override void Dispose()
    {
        SDL.SDL_DestroyTexture(this.texture);
        this.absolutesrc.x = this.absolutesrc.y = 0;
        this.needresettexture = true;
        this.texture = IntPtr.Zero;
    }
}

/// <summary>
/// 이미지 파일을 통해 불러오는 텍스쳐입니다.
/// </summary>
public class TextureFromFile : DrawableTexture
{
    public string filename = string.Empty;

    public TextureFromFile(string filename)
    {
        this.filename = filename;
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

    public TextureFromFile() { }

    public override void Dispose()
    {
        SDL.SDL_DestroyTexture(this.texture);
        this.absolutesrc.x = this.absolutesrc.y = 0;
        this.needresettexture = true;
        this.texture = IntPtr.Zero;
    }
}

public class TextureFromStringForXPM : DrawableTexture
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

public class TextureFromText : DrawableTexture, IDisposable
{
    public string Fontfile,Text;
    public int Size { get; internal set; }
    public Color Color;
    public Color? BackgroundColor;
    public bool Blended = true;
    public uint WarpLength = 0;
    public bool ResourceReady => fontsource != IntPtr.Zero;

    public TextureFromText(string Fontfile,int Size,string Text,Color Color,Color? BackgroundColor = null,bool Blended = true) {
        this.Fontfile = Fontfile;
        this.Size = Size;
        this.Color = Color;
        this.BackgroundColor = BackgroundColor;
        this.Text = Text;
        this.Blended = Blended;

        fontsource = SDL_ttf.TTF_OpenFont(Fontfile , Size);
        if (fontsource == IntPtr.Zero)
            throw new JyunrcaeaFrameworkException($"불러올수 없는 글꼴 파일 SDL Error: {SDL.SDL_GetError()}");
        Rendering();
        Ready();
    }

    public void ReRender()
    {
        if (this.texture != IntPtr.Zero) Dispose();
        Rendering();
    }

    public void Resize(int size)
    {
        if (this.Size == size) return;
        if (SDL_ttf.TTF_SetFontSize(fontsource,this.Size = size) != 0) throw new JyunrcaeaFrameworkException($"글꼴 크기를 조정하는데 실패하였습니다. (SDL Error: {SDL.SDL_GetError()})");
    }

    SDL.SDL_Surface surface;
    IntPtr buffer;
    IntPtr fontsource;

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
                buffer = SDL_ttf.TTF_RenderUTF8_Blended_Wrapped(fontsource, Text, Color.colorbase,this.WarpLength);
            }
            else
            {
                buffer = SDL_ttf.TTF_RenderUTF8_Solid_Wrapped(fontsource, Text, Color.colorbase, this.WarpLength);
            }
        }
        else
        {
            buffer = SDL_ttf.TTF_RenderUTF8_Shaded_Wrapped(fontsource, Text, Color.colorbase, BackgroundColor.colorbase, this.WarpLength);
        }
        if (buffer == IntPtr.Zero)
        {
            throw new JyunrcaeaFrameworkException($"텍스쳐를 렌더링 하는데 실패하였습니다. (SDL Error: {SDL.SDL_GetError()})");
        }
        surface = SDL.PtrToStructure<SDL.SDL_Surface>(buffer);
        this.absolutesrc.x = surface.w;
        this.absolutesrc.y = surface.h;
        this.texture = SDL.SDL_CreateTextureFromSurface(Framework.renderer, buffer);
        SDL.SDL_FreeSurface(buffer);
        if (this.texture == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"렌더링 된 텍스트를 텍스쳐로 변환하는데 실패하였습니다. {SDL.SDL_GetError()}");
        this.needresettexture = true;
        if (!this.FixedRenderRange)
        {
            this.src.w = this.absolutesrc.x;
            this.src.h = this.absolutesrc.y;
        }
    }

    public override void Dispose()
    {
        SDL.SDL_DestroyTexture(this.texture);
        this.texture = IntPtr.Zero;
        SDL_ttf.TTF_CloseFont(fontsource);
    }
}

public class TextureFromTextFileForXPM : DrawableTexture
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

public class Font : IDisposable
{
    /// <summary>
    /// 기본 글꼴파일의 경로를 설정합니다.
    /// Text 객체 생성시 글꼴파일 경로를 null로 설정할경우 이 경로가 채택됩니다.
    /// </summary>
    public static string DefaultPath = string.Empty;

    internal IntPtr fontsource = IntPtr.Zero;

    internal int sz = 1;

    /// <summary>
    /// 글꼴의 크기입니다.
    /// </summary>
    public int Size { get => sz; set {
            if (SDL_ttf.TTF_SetFontSize(this.fontsource, this.sz = value) == -1) throw new JyunrcaeaFrameworkException($"폰트 로드에 실패했습니다. SDL_TTF Error: {SDL_ttf.TTF_GetError()}");
        }
    }

    /// <summary>
    /// 글꼴을 불러옵니다.
    /// </summary>
    /// <param name="filename">글꼴 파일 경로</param>
    /// <param name="size">글자 크기 (높이 기준)</param>
    public Font(string filename,int size)
    {
        this.fontsource = SDL_ttf.TTF_OpenFont(filename, this.sz = size);
    }

    /// <summary>
    /// 불러온 글꼴을 메모리에서 해제합니다.
    /// </summary>
    public void Dispose() {
        SDL_ttf.TTF_CloseFont(this.fontsource);
    }
}

public enum HorizontalPositionType
{
    Middle = 0,
    Left = 1,
    Right = 2
}

public enum VerticalPositionType
{
    Middle = 0,
    Top = 1,
    Bottom = 2
}

[Flags]
public enum FontStyle
{
    Normal = SDL_ttf.TTF_STYLE_NORMAL,
    Bold = SDL_ttf.TTF_STYLE_BOLD,
    Italic = SDL_ttf.TTF_STYLE_ITALIC,
    Underline = SDL_ttf.TTF_STYLE_UNDERLINE,
    Strikethrough = SDL_ttf.TTF_STYLE_STRIKETHROUGH
}

/// <summary>
/// 객체에 대한 자세한 정보를 얻어냅니다.
/// 객체는 렌더링 때 좌표나 크기 등 값들이 새로고침 되므로, 업데이트 도중에 변경사항이 있어도 그 사항이 적용된 값을 제공하지 않는다는점 주의해주세요.
/// </summary>
public static class DetailOfObject
{
    public interface Size
    {
        public int DisplayedWidth { get; }
        public int DisplayedHeight { get; }
    }

    public static int DrawWidth(DrawableObject obj)
    {
        return obj.RealWidth;
    }

    public static int DrawHeight(DrawableObject obj)
    {
        return obj.RealHeight;
    }

    /// <summary>
    /// 실제 렌더링 위치를 알아냅니다. (객체의 왼쪽 위 모서리의 좌표)
    /// </summary>
    /// <param name="obj">객체</param>
    /// <param name="x">X 좌표</param>
    /// <param name="y">Y 좌표</param>
    public static void RealPosition(DrawableObject obj,out int x,out int y)
    {
        x = obj.Rx;
        y = obj.Ry;
    }

    /// <summary>
    /// 하위 객체의 위치를 다시 맞춥니다.
    /// </summary>
    [Obsolete("정확하지 않음")]
    public static void ResetPosition(Group target)
    {
        if (target.Parent is not null)
        {
            Stack<Group> top = new();
            top.Push(target.Parent);
            Group? g;
            while ((g = top.First().Parent) is not TopGroup)
            {
                if (g is null) break;
                top.Push(g);
            }
            int x=0, y=0;
            while (top.Count != 0)
            {
                g = top.Pop();
                x += g.Rx;
                y += g.Ry;
            }
            Framework.DrawPos.x = x;
            Framework.DrawPos.y = y;
        }
        Framework.Positioning(target);
    }
}


/// <summary>
/// 쥰르케아 프레임워크 내에 발생하는 예외적인 오류입니다.
/// 프레임워크의 예외 오류는 작동 원리만 잘 파악하면 예방할수 있습니다.
/// </summary>
public class JyunrcaeaFrameworkException : Exception
{
    public JyunrcaeaFrameworkException() { }
    /// <summary>
    /// 쥰르케아 프레임워크 예외 오류
    /// </summary>
    /// <param name="message">오류 내용</param>
    public JyunrcaeaFrameworkException(string message) : base(message) { }
}