#define WINDOWS
using SDL2;

namespace JyunrcaeaFramework
{
#if DEBUG
    /// <summary>
    /// 프레임워크 개발용 테스를 하기 위한 기능이 모여있습니다.
    /// 프레임워크 기여자가 아닌이상 쓸 필요는 없습니다.
    /// </summary>
    public static class Debug
    {
        /// <summary>
        /// SDL2 버전
        /// </summary>
        public static Version SDL2Version
        {
            get
            {
                SDL.SDL_GetVersion(out var v);
                return new(v.major, v.minor, v.patch);
            }
        }

        /// <summary>
        /// SDL2 렌더러
        /// </summary>
        public static IntPtr Renderer => Framework.renderer;
        /// <summary>
        /// SDL2 윈도우
        /// </summary>
        public static IntPtr Window => Framework.window;

        /// <summary>
        /// 텍스트 렌더링 된 횟수
        /// </summary>
        public static ulong TextRenderCount = 0;

        /// <summary>
        /// ODD 를 실행할지에 대한 여부입니다.
        /// </summary>
        public static bool ObjectDrawDebuging = false;  
        /// <summary>
        /// ODD 실행시 객체의 테두리를 표시할 색입니다.
        /// </summary>
        public static Color ObjectDrawDebugingLineColor = new(255, 50, 50);
        /// <summary>
        /// ODD 실행시 장면의 테두리를 표시할 색입니다.
        /// </summary>
        public static Color SceneDrawDebugingLineColor = new(50, 255, 50);

        internal static void ZeneretyODD(Group group)
        {
            for (int i = 0; i < group.Objects.Count; i++)
            {
                if (group.Objects[i] is Group)
                {
                    ZeneretyODD((Group)group.Objects[i]);
                    continue;
                }

                if (group.Objects[i] is ZeneretyDrawableObject)
                {
                    SDL.SDL_SetRenderDrawColor(Framework.renderer, ObjectDrawDebugingLineColor.colorbase.r, ObjectDrawDebugingLineColor.colorbase.g, ObjectDrawDebugingLineColor.colorbase.b, ObjectDrawDebugingLineColor.colorbase.a);
                    SDL.SDL_RenderDrawRect(Framework.renderer, ref ((ZeneretyDrawableObject)group.Objects[i]).renderposition);
                }

            }
        }
    }
#endif

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
        /// 이벤트(Update, Quit 등)를 멀티 코어(또는 스레드)로 처리할지에 대한 여부입니다.
        /// true 로 하게 될경우 모든 장면속 이벤트 함수가 동시에 실행됩니다!
        /// 장면 갯수가 적은 경우 사용하지 않는걸 권장합니다.
        /// (Zenerety 렌더링에는 적용되지 않습니다.)
        /// </summary>
        public static bool MultiCoreProcess = false;

        /// <summary>
        /// 현재 프레임워크의 버전을 알려줍니다.
        /// </summary>
        public static readonly Version Version = new(0, 6, 2);
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
                        if (e.key.repeat != 0) return 0;
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
                                return 0;
                            default:
                                return 1;
                        }
                    }, IntPtr.Zero); 
            }
            #region 믹서, 윈플래그, 텍스쳐 쉐어링, 창 크기, 디스플레이 모드, 힌트
            SDL_mixer.Mix_HookMusicFinished(Music.Finished);
            if ((winflg & SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP) == SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP) Window.fullscreenoption = true;
            if ((winflg & SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS) == SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS) Window.windowborderless = true;
            TextureSharing.resourcelist = new();
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
        //internal static SDL.SDL_Event sdle;
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


        public static void RunningLoop()
        {
            SDL.SDL_Event e;
            while (Running)
            {
                if (EventMultiThreading)
                {
                    while (SDL.SDL_PollEvent(out var ae) == 1)
                    {
                        AsyncEventProcess(ae);
                    }
                } else
                {
                    while (SDL.SDL_PollEvent(out e) == 1) EventProcess(e);
                }
                Framework.Function.Draw();
            }
        }

        public static bool EventMultiThreading = false;

        public static async void AsyncEventProcess(SDL.SDL_Event e)
        {
            //SDL.SDL_Event e;
            //SDL.SDL_PollEvent(out e);
            await Task.Run(() => EventProcess(e));
        }

        public static void EventProcess(SDL.SDL_Event e)
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
                    Framework.Function.KeyDown((Input.Keycode)e.key.keysym.sym);
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
                    Framework.Function.KeyUp((Input.Keycode)e.key.keysym.sym);
                    break;
                case SDL.SDL_EventType.SDL_TEXTINPUT:
                    unsafe
                    {
                        Input.TextInput.InputedText += new string((sbyte*)e.text.text);
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
        /// 새로운 렌더링 방식(Zenerety)을 사용할지에 대한 여부입니다.
        /// </summary>
        public static bool NewRenderingSolution = false;

        internal static Stack<ZeneretySize> DrawPosStack = new();

        internal static SDL.SDL_Rect DrawPos = new() { x = 0, y = 0 };

        internal static SDL.SDL_Rect RenderRange = new();

        internal static void Rendering(Group group)
        {
            SDL.SDL_Rect? before = null;
            if (group.RenderRange is not null)
            {
                before = RenderRange;
                RenderRange = new() { x = group.Rx, y = group.Ry, w = group.RenderRange.Width, h = group.RenderRange.Height };
                SDL.SDL_RenderSetViewport(Framework.renderer, ref RenderRange);
            }
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
                        SDL.SDL_Rect r = ((ZeneretyDrawableObject)group.Objects[i]).renderposition;
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
                    SDL.SDL_RenderFillRect(Framework.renderer, ref ((ZeneretyDrawableObject)group.Objects[i]).renderposition);
                    continue;
                }

                if (group.Objects[i] is Image)
                {
                    SDL.SDL_RenderCopyEx(Framework.renderer, ((Image)group.Objects[i]).Texture.texture, ref ((Image)group.Objects[i]).Texture.src, ref ((ZeneretyDrawableObject)group.Objects[i]).renderposition, ((Image)group.Objects[i]).Rotation, IntPtr.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
                    continue;
                }

                if (group.Objects[i] is Text)
                {
                    SDL.SDL_RenderCopyEx(Framework.renderer, ((Text)group.Objects[i]).TFT.texture, ref ((Text)group.Objects[i]).TFT.src, ref ((ZeneretyDrawableObject)group.Objects[i]).renderposition, ((Text)group.Objects[i]).Rotation, IntPtr.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
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
            if (group.RenderRange is not null)
            {
                before = RenderRange;
                RenderRange = new()
                {
                    x = wx,
                    y = hy,
                    w = group.RenderRange.Width,
                    h = group.RenderRange.Height
                };
                wx = 0;
                hy = 0;
            }

            for (int i = 0; i < group.Objects.Count; i++)
            {
                DrawPos.x = wx;
                DrawPos.y = hy;

                if (group.Objects[i] is Group)
                {
                    Positioning((Group)group.Objects[i]);
                    continue;
                }

                if (group.Objects[i] is ZeneretyDrawableObject)
                {
                    ZeneretyDrawableObject zdo = (ZeneretyDrawableObject)group.Objects[i];
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

    public delegate void ZeneretyEvent();

    public delegate void ZeneretyKeyboardEvent(Input.Keycode e);

    public delegate void ZeneretyClickEvent(Input.Mouse.Key e);

    public class TopGroup : Group
    {
        public TopGroup()
        {
            this.Ancestor = this;
            this.Parent = this;
        }

        internal EventList EventManager = new();

        internal override void PushEvent(ZeneretyObject target)
        {
            EventManager.Add(target);
        }
    }

    public class EventList
    {
        internal Queue<ZeneretyObject> ObjectQueue = new();

        internal List<Events.Resized> Resized = new();
        internal List<Events.Resize> Resize = new();
        internal List<Events.Update> Update = new();
        internal List<Events.KeyDown> KeyDown = new();
        internal List<Events.KeyUp> keyUp = new();
        internal List<Events.MouseMove> mouseMoves = new();
        internal List<Events.MouseKeyDown> mouseKeyDowns = new();
        internal List<Events.MouseKeyUp> mouseKeyUps = new();

        public void Add(ZeneretyObject obj)
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
    /// Zenerety 렌더링에 사용될 크기 조정 클래스입니다.
    /// </summary>
    public class ZeneretySize
    {
        public int Width, Height;
        public ZeneretySize(int Width = 0, int Height = 0)
        {
            this.Width = Width;
            this.Height = Height;
        }
    }

    public class ZeneretyRenderRange : ZeneretySize
    {
        HorizontalPositionType DrawX;
        VerticalPositionType DrawY;

        public ZeneretyRenderRange(int Width=0,int Height=0,HorizontalPositionType DrawX = HorizontalPositionType.Middle,VerticalPositionType DrawY = VerticalPositionType.Middle) : base(Width,Height)
        {
            this.Width = Width;
            this.Height = Height;
            this.DrawX = DrawX;
            this.DrawY = DrawY;
        }
    }

    public class Text : ZeneretyExtendObject, Events.Update, Events.Resize
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
    /// Zenerety 렌더링에 사용될 배율 조정 클래스입니다.
    /// </summary>
    public class ZeneretyScale
    {
        public double X, Y;
        public ZeneretyScale(double x=1d,double y=1d)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// Zenerety 렌더링 용 객체 
    /// </summary>
    public class ZeneretyObject
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
    /// Zenerety 렌더링 용 그릴수 있는 객체
    /// </summary>
    public abstract class ZeneretyDrawableObject : ZeneretyObject
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

        public abstract byte Opacity { get; set; }

        internal int dxw=0, dyh=0;

        /// <summary>
        /// 렌더링 될 이미지의 절대적 크기
        /// (null 로 설정시 원본 이미지의 크기를 따릅니다.)
        /// </summary>
        public ZeneretySize? AbsoluteSize = null;

        /// <summary>
        /// 이미지의 너비 및 높이의 배율
        /// </summary>
        public ZeneretyScale Scale = new();

        /// <summary>
        /// 창 크기에 맞춰 자동으로 크기 조정을 사용할지에 대한 여부입니다.
        /// </summary>
        public bool RelativeSize = true;

        /// <summary>
        /// 실제 렌더링 범위
        /// </summary>
        internal SDL.SDL_Rect renderposition = new();

        public HorizontalPositionType DrawX = HorizontalPositionType.Middle;
        public VerticalPositionType DrawY = VerticalPositionType.Middle;
    }

    /// <summary>
    /// Zenerety 렌더링 용 그리기도 가능하고 회전, 뒤집기 등이 가능한 확장된 객체
    /// </summary>
    public abstract class ZeneretyExtendObject : ZeneretyDrawableObject
    {
        /// <summary>
        /// 회전값
        /// </summary>
        public double Rotation = 0;

        public int AbsoluteWidth => this.RealWidth;
        public int AbsoluteHeight => this.RealHeight;
    }

    /// <summary>
    /// Zenerety 렌더링 방식에 사용되는 그룹 객체입니다.
    /// </summary>
    public class Group : ZeneretyObject, Events.Update, Events.Resize
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
        public ZeneretyList Objects { get; protected set; } = null!;

        internal virtual void ResetPosition(ZeneretySize Position)
        {
            if (this.RenderRange is not null)
            {
                Position.Width = this.RenderRange.Width;
                Position.Height = this.RenderRange.Height;
            }
            for (int i = 0; i < Objects.Count; i++)
            {
                Objects[i].Cx = (int)(Position.Width * Objects[i].CxD);
                Objects[i].Cy = (int)(Position.Height * Objects[i].CyD);
                if (Objects[i] is Group) ((Group)Objects[i]).ResetPosition(Position);
            }
        }

        public ZeneretyRenderRange? RenderRange = null;

        

        internal int RealWidth
        {
            get
            {
                if (RenderRange is null)
                {
                    if (base.Parent is null) return Window.Width;
                    return base.Parent.RealWidth;
                }
                return RenderRange.Width;
            }
        }

        internal int RealHeight
        {
            get
            {
                if (RenderRange is null)
                {
                    if (base.Parent is null) return Window.Height;
                    return base.Parent.RealHeight;
                }
                return RenderRange.Height;
            }
        }

        /// <summary>
        /// 오브젝트 준비
        /// </summary>
        public virtual void Prepare()
        {
            for (int i = 0; i < this.Objects.Count; i++)
            {
                this.Ancestor!.PushEvent(this.Objects[i]);
                //this.PushEvent(this.Objects[i]);

                if (this.Objects[i] is Group)
                {
                    ((Group)this.Objects[i]).Objects.Ancestor =((Group)this.Objects[i]).Ancestor = this.Ancestor;
                    ((Group)this.Objects[i]).Prepare();
                    continue;
                }

                if (this.Objects[i] is Image)
                {
                    ((Image)this.Objects[i]).Texture.Ready();
                    continue;
                }

                if (this.Objects[i] is Text)
                {
                    ((Text)Objects[i]).TFT.Ready();
                    continue;
                }
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
                    ((Image)this.Objects[i]).Texture.Free();
                }

                if (this.Objects[i] is Text)
                {
                    ((Text)this.Objects[i]).TFT.Free();
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

        internal virtual void PushEvent(ZeneretyObject target)
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
    /// 직사각형을 그리는 객체입니다. (Zenerety 렌더링 전용)
    /// </summary>
    public class Box : ZeneretyDrawableObject
    {
        public Box(int width = 0,int height = 0,Color? color = null)
        {
            this.Size = new(width, height);
            this.Color = color is null ? Color.White : color;
        }

        /// <summary>
        /// 직사각형의 너비와 높이
        /// </summary>
        public ZeneretySize Size;

        /// <summary>
        /// 출력할 색상
        /// </summary>
        public Color Color;

        public override byte Opacity { get => Color.Alpha; set => Color.Alpha = value; }

        internal override int RealWidth =>  (int)(Size.Width * Scale.X * (this.RelativeSize ? Window.AppropriateSize : 1));
        internal override int RealHeight => (int)(Size.Height * Scale.Y * (this.RelativeSize ? Window.AppropriateSize : 1));
    }

    /// <summary>
    /// 이미지를 출력하는 객체입니다. (Zenerety 렌더링 전용)
    /// </summary>
    public class Image : ZeneretyExtendObject
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
    /// 원을 그리는 객체입니다. (Zenerety 렌더링 전용)
    /// </summary>
    public class Circle : ZeneretyDrawableObject
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
    /// 그려지지 않는 가짜 객체입니다. (Zenerety 렌더링 전용)
    /// </summary>
    public class GhostBox : ZeneretyDrawableObject
    {
        public GhostBox(int width=0,int height=0)
        {
            this.Size = new(width, height);
        }
        public ZeneretySize Size;

        public override byte Opacity { get; set; } = 0;

        internal override int RealWidth => (int)(Size.Width * Scale.X * (this.RelativeSize ? Window.AppropriateSize : 1));
        internal override int RealHeight => (int)(Size.Height * Scale.Y * (this.RelativeSize ? Window.AppropriateSize : 1));
    }

    /// <summary>
    /// 모서리가 둥근 직사각형을 그리는 객체입니다. (Zenerety 렌더링 전용)
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
        public ZeneretyObject target;
        public int index;

        public AddTarget(ZeneretyObject obj,int pos = -1)
        {
            this.index = pos;
            this.target = obj;
        }
    }

    /// <summary>
    /// Zenerety 객체를 위한 리스트입니다.
    /// 추가 및 제거시 자동으로 리소스를 할당/해제 해줍니다.
    /// </summary>
    public class ZeneretyList : List<ZeneretyObject>, IDisposable
    {
        internal TopGroup? Ancestor;

        Group Parent;
        public ZeneretyList(Group group)
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

        void AddProcedure(ZeneretyObject obj)
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
        public new void Add(ZeneretyObject obj)
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

        public new void AddRange(IEnumerable<ZeneretyObject> objs)
        {
            foreach(var a in objs)
            {
                Add(a);
            }
        }

        public void AddRange(params ZeneretyObject[] objs)
        {
            for (int i =0;i < objs.Length;i++)
            {
                this.Add(objs[i]);
            }
        }

        public void AddRange(int index = 0,params ZeneretyObject[] objs)
        {
            for (int i = 0; i < objs.Length; i++)
            {
                this.Insert(index + i, objs[i]);
            }
        }

        public new void Insert(int index,ZeneretyObject zo)
        {
            if (zo.Parent is not null) throw new JyunrcaeaFrameworkException("이미 다른 부모 객체에게 상속된 객체입니다.");
            zo.Parent = this.Parent;
            if (this.Parent.ResourceReady) {
                AddProcedure(zo);
                this.AddList.Enqueue(new(zo, index));
            }
            else base.Insert(index, zo);
        }


        public bool Switch(ZeneretyObject target,int index = -1)
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
        public new bool Remove(ZeneretyObject obj)
        {
            if(base.Remove(obj) is false) return false;
            if (obj is Group)
            {
                ((Group)obj).Release();
                return true;
            }
            if (obj is Image)
            {
                ((Image)obj).Texture.Free();
                return true;
            }
            if (obj is Text)
            {
                ((Text)obj).TFT.Free();
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
                    ((Image)obj).Texture.Free();
                    return;
                }
                if (obj is Text)
                {
                    ((Text)obj).TFT.Free();
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

        /// <summary>
        /// Zenerety 렌더링 방식에 사용되는 장면 탐색기입니다.
        /// </summary>
        public static TopGroup Target = new();

        internal static SDL.SDL_DisplayMode dm;

        internal static List<SceneInterface> scenes = new();

        internal static List<Thread> threads = new();
        /// <summary>
        /// 장면을 추가합니다.
        /// </summary>
        /// <param name="scene">장면</param>
        /// <returns>장면의 위치</returns>
        public static int AddScene(SceneInterface scene)
        {
            scenes.Add(scene);
            threads.Add(null!);
            if (Framework.Running) scene.Start();
            return scenes.Count - 1;
        }
        /// <summary>
        /// 원하는 위치의 장면을 제거합니다.
        /// </summary>
        /// <param name="index">장면의 위치</param>
        public static void RemoveScene(int index)
        {
            scenes[index].Stop();
            scenes.RemoveAt(index);
            threads.RemoveAt(index);
        }
        /// <summary>
        /// 특정 장면을 제거합니다.
        /// </summary>
        /// <param name="scene"></param>
        public static void RemoveScene(SceneInterface scene)
        {
            scene.Stop();
            scenes.Remove(scene);
            threads.RemoveAt(0);
        }

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
        Events.MouseFocusOut
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
            for (int i=0;i<group.Objects.Count;i++)
            {
                if (group.Objects[i] is Group)
                {
                    Prepare((Group)group.Objects[i]);
                    continue;
                }
                 
                if (group.Objects[i] is Image)
                {
                    ((Image)group.Objects[i]).Texture.Ready();
                }
            }
        }

        //Stop, Free 와 비슷
        internal static void Release(Group group)
        {
            for (int i = 0; i < group.Objects.Count; i++)
            {
                if (group.Objects[i] is Group)
                {
                    Release((Group)group.Objects[i]);
                    continue;
                }

                if (group.Objects[i] is Image)
                {
                    ((Image)group.Objects[i]).Texture.Free();
                }


            }
        }

        /// <summary>
        /// 'Framework.Run' 함수를 호출시 실행되는 함수입니다.
        /// </summary>
        public override void Start()
        {
            if (Framework.NewRenderingSolution)
            {
                Display.Target.Prepare();
            }
            else
            {

                if (Framework.MultiCoreProcess)
                {
                    Parallel.For(0, Display.scenes.Count, (i, _) => Display.scenes[i].Start());
                }
                else
                {
                    for (int i = 0; i < Display.scenes.Count; i++)
                    {
                        Display.scenes[i].Start();
                    }
                }

            }
        }
        /// <summary>
        /// 'Framework.Stop' 함수를 호출시 실행되는 함수입니다.
        /// </summary>
        public override void Stop()
        {
            if (Framework.NewRenderingSolution)
            {
                Display.Target.Release();
            }
            else
            {

                if (Framework.MultiCoreProcess)
                {
                    Parallel.For(0, Display.scenes.Count, (i, _) => Display.scenes[i].Stop());
                }
                else
                {
                    for (ist = 0; ist < Display.scenes.Count; ist++)
                    {
                        Display.scenes[ist].Stop();
                    }
                }

            }
        }

        private int iu, id, ir, iwm, ird, ist, ifd;

        private float iratio;
        /// <summary>
        /// 창의 크기가 조절될경우 실행되는 함수입니다.
        /// (창 크기 조절이 완전히 끝날때 실행되진 않습니다.)
        /// 창이 처음 생성될때는 실행되지 않습니다.
        /// </summary>
        public override void Resize()
        {
            iratio = (float)Window.size.w / (float)Window.default_size.x;
            if (iratio * Window.default_size.y > Window.size.h)
            {
                iratio = (float)Window.size.h / (float)Window.default_size.y;
            }
            Window.AppropriateSize = iratio;

            if (Framework.NewRenderingSolution)
            {
                Display.Target.Resize();
                //for (int i = 0;i<Display.Target.EventManager.Resize.Count;i++)
                //{
                //    Display.Target.EventManager.Resize[i].Resize();
                //}
                return;
            }

            for (ir = 0; ir < Display.scenes.Count; ir++)
            {
                if (!Display.scenes[ir].EventRejection) Display.scenes[ir].Resize();
            }
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

            if (Framework.NewRenderingSolution)
            {
                Framework.RenderRange = Window.size;
                SDL.SDL_RenderSetViewport(Framework.renderer,ref Window.size);
                Framework.Rendering(Display.Target);
#if DEBUG
                if (Debug.ObjectDrawDebuging) Debug.ZeneretyODD(Display.Target);
#endif
            }
            else
            {

                for (id = 0; id < Display.scenes.Count; id++)
                {
                    if (!Display.scenes[id].Hide) Display.scenes[id].Draw();
                }
    #if DEBUG
                if (Debug.ObjectDrawDebuging)
                {
                    ODD();
                }
    #endif

            }
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
            if (Framework.NewRenderingSolution)
            {
                Display.Target.ResetPosition(new(Window.Width,Window.Height));
                Framework.RenderRange = Window.size;
                Framework.Positioning(Display.Target);
                Display.Target.Update(ms);
                Animation.AnimationQueue.Update();
                Framework.RenderRange = Window.size;
                Framework.Positioning(Display.Target);
                updatetime = updatems;
                return;
            }

            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => { if (Display.scenes[i].UpdateRejection) return; Display.scenes[i].Update(ms); });
            }
            else {
                for (iu = 0; iu < Display.scenes.Count; iu++)
                {
                    if (!Display.scenes[iu].UpdateRejection) Display.scenes[iu].Update(ms);
                }
            }

            updatetime = updatems;
        }
        /// <summary>
        /// 창 크기 조절이 완전히 끝날때 호출되는 함수입니다.
        /// </summary>
        public virtual void Resized()
        {
            //Resize();

            if (Framework.NewRenderingSolution)
            {
                for (int i = 0; i < Display.Target.EventManager.Resized.Count; i++)
                {
                    Display.Target.EventManager.Resized[i].Resized();
                }
                return;
            }

            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => Display.scenes[i].Resized());
            }
            else {
                for (ird = 0; ird < Display.scenes.Count; ird++)
                {
                    if (!Display.scenes[ird].EventRejection) Display.scenes[ird].Resized();
                }
            }

        }

        int winrestore, winmax, winmin;

        public virtual void WindowMaximized()
        {
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => {
                    if(!Display.scenes[i].EventRejection) Display.scenes[i].WindowMaximized();
                });
            } else
            {
                for (winmax=0;winmax<Display.scenes.Count;winmax++)
                {
                    if (!Display.scenes[winmax].EventRejection) Display.scenes[winmax].WindowMaximized();
                }
            }
        }
        public virtual void WindowMinimized()
        {
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => {
                    if (!Display.scenes[i].EventRejection) Display.scenes[i].WindowMinimized();
                });
            }
            else
            {
                for (winmin = 0; winmin < Display.scenes.Count; winmin++)
                {
                    if (!Display.scenes[winmin].EventRejection) Display.scenes[winmin].WindowMinimized();
                }
            }
        }
        public virtual void WindowRestore()
        {
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => {
                    if (!Display.scenes[i].EventRejection) Display.scenes[i].WindowRestore();
                });
            }
            else
            {
                for (winrestore = 0; winrestore < Display.scenes.Count; winrestore++)
                {
                    if (!Display.scenes[winrestore].EventRejection) Display.scenes[winrestore].WindowRestore();
                }
            }
        }
        /// <summary>
        /// 창 위치가 조정될떄 호출되는 함수입니다.
        /// </summary>
        public virtual void WindowMove()
        {
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => Display.scenes[i].WindowMove());
            }
            else
            {
                for (iwm = 0; iwm < Display.scenes.Count; iwm++)
                {
                    if (!Display.scenes[iwm].EventRejection) Display.scenes[iwm].WindowMove();
                }
            }

        }

        static int iwq;
        /// <summary>
        /// 창 나가기 버튼을 클릭했을때 호출되는 함수입니다.
        /// </summary>
        public virtual void WindowQuit()
        {

            if (!Framework.NewRenderingSolution)
            {
                if (Framework.MultiCoreProcess)
                {
                    Parallel.For(0, Display.scenes.Count, (i, _) => Display.scenes[i].WindowQuit());
                }
                else
                {
                    for (iwq = 0; iwq < Display.scenes.Count; iwq++)
                        if (!Display.scenes[iwq].EventRejection) Display.scenes[iwq].WindowQuit();
                }
            }

            if (Window.FrameworkStopWhenClose) Framework.Stop();
        }
        /// <summary>
        /// 파일이 드래그 드롭될때 호출되는 함수입니다.
        /// </summary>
        /// <param name="filename"></param>
        public virtual void DropFile(string filename)
        {
            for (ifd = 0; ifd < Display.scenes.Count; ifd++)
                if (!Display.scenes[ifd].EventRejection) Display.scenes[ifd].DropFile(filename);
        }

        static int ikd;
        /// <summary>
        /// 키보드의 특정 키가 눌렸을때 실행되는 함수입니다.
        /// </summary>
        /// <param name="e"></param>
        public virtual void KeyDown(Input.Keycode e)
        {
            if (Framework.NewRenderingSolution)
            {
                ikd = Display.Target.EventManager.KeyDown.Count;
                for(int i=0;i<ikd;i++)
                {
                    Display.Target.EventManager.KeyDown[i].KeyDown(e);
                }
                return;
            }
            for (ikd = 0; ikd < Display.scenes.Count; ikd++)
                if (!Display.scenes[ikd].EventRejection) Display.scenes[ikd].KeyDown(e);
        }

        static int imm;
        /// <summary>
        /// 마우스가 움직일때 호출되는 함수입니다.
        /// </summary>
        public virtual void MouseMove()
        {
            SDL.SDL_GetMouseState(out Input.Mouse.position.x, out Input.Mouse.position.y);
            if (Framework.NewRenderingSolution)
            {
                int len = Display.Target.EventManager.mouseMoves.Count;
                for (int i=0; i <len; i++)
                {
                    Display.Target.EventManager.mouseMoves[i].MouseMove();
                }
                return;
            }
            for (imm = 0; imm < Display.scenes.Count; imm++)
            {
                if (!Display.scenes[imm].EventRejection) Display.scenes[imm].MouseMove();
            }
        }

        static int imd, imu, iku;

        public virtual void MouseKeyDown(Input.Mouse.Key key)
        {
            if (Framework.NewRenderingSolution)
            {
                int len = Display.Target.EventManager.mouseKeyDowns.Count;
                for (int i = 0; i < len; i++)
                {
                    Display.Target.EventManager.mouseKeyDowns[i].MouseKeyDown(key);
                }
                return;
            }
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => { if (!Display.scenes[i].EventRejection) Display.scenes[i].MouseKeyDown(key); });
            }
            else
            {
                for (imd = 0; imd < Display.scenes.Count; imd++)
                {
                    if (!Display.scenes[imd].EventRejection) Display.scenes[imd].MouseKeyDown(key);
                }
            }
        }

        public virtual void MouseKeyUp(Input.Mouse.Key key)
        {
            if (Framework.NewRenderingSolution)
            {
                int len = Display.Target.EventManager.mouseKeyUps.Count;
                for (int i = 0; i < len; i++)
                {
                    Display.Target.EventManager.mouseKeyUps[i].MouseKeyUp(key);
                }
                return;
            }
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => { if (!Display.scenes[i].EventRejection) Display.scenes[i].MouseKeyUp(key); });
            }
            else
            {
                for (imu = 0; imu < Display.scenes.Count; imu++)
                {
                    if (!Display.scenes[imu].EventRejection) Display.scenes[imu].MouseKeyUp(key);
                }
            }
        }

        public virtual void KeyUp(Input.Keycode key)
        {
            if (Framework.NewRenderingSolution)
            {
                iku = Display.Target.EventManager.keyUp.Count;
                for (int i = 0; i < iku; i++)
                {
                    Display.Target.EventManager.keyUp[i].KeyUp(key);
                }
                return;
            }
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => { if (!Display.scenes[i].EventRejection) Display.scenes[i].KeyUp(key); });
            }
            else
            {
                for (iku = 0; iku < Display.scenes.Count; iku++)
                {
                    if (!Display.scenes[iku].EventRejection) Display.scenes[iku].KeyUp(key);
                }
            }
        }

        int kfi, kfo;

        public virtual void KeyFocusIn()
        {
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => Display.scenes[i].KeyFocusIn());
            }
            else
            {
                for (kfi = 0; kfi < Display.scenes.Count; kfi++)
                    if (!Display.scenes[kfi].EventRejection) Display.scenes[kfi].KeyFocusIn();
            }
        }

        public virtual void KeyFocusOut()
        {
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => Display.scenes[i].KeyFocusOut());
            }
            else
            {
                for (kfo = 0; kfo < Display.scenes.Count; kfo++)
                    if (!Display.scenes[kfo].EventRejection) Display.scenes[kfo].KeyFocusOut();
            }
        }

        int mfi, mfo;

        public virtual void MouseFocusIn()
        {
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => Display.scenes[i].MouseFocusIn());
            }
            else
            {
                for (mfi = 0; mfi < Display.scenes.Count; mfi++)
                    if (!Display.scenes[mfi].EventRejection) Display.scenes[mfi].MouseFocusIn();
            }
        }

        public virtual void MouseFocusOut()
        {
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => Display.scenes[i].MouseFocusOut());
            }
            else
            {
                for (mfo = 0; mfo < Display.scenes.Count; mfo++)
                    if (!Display.scenes[mfo].EventRejection) Display.scenes[mfo].MouseFocusOut();
            }
        }

        public virtual void DisplayChange()
        {
            if (Window.Fullscreen)
            {
                Window.Resize(Display.MonitorWidth, Display.MonitorHeight);
            }
            if (Display.FrameLateLimit == 0) Display.FrameLateLimit = 0;
        }

#if DEBUG
        internal override void ODD()
        {
            SDL.SDL_SetRenderDrawColor(Framework.renderer, Debug.ObjectDrawDebugingLineColor.Red, Debug.ObjectDrawDebugingLineColor.Green, Debug.ObjectDrawDebugingLineColor.Blue, Debug.ObjectDrawDebugingLineColor.Alpha);
            for (int i = 0; i < Display.scenes.Count; i++)
            {
                if (!Display.scenes[i].Hide) Display.scenes[i].ODD();
            }
        }
#endif
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

        public AudioOption(byte Channals = 8, bool TryLowChannals = true, int ChunkSize = 2048, int Hz = 48000)
        {
            trylow = TryLowChannals;
            ch = Channals;
            cs = ChunkSize;
            hz = Hz;
        }
    }

    public abstract class PlayableSound
    {
        internal IntPtr sound;

        internal abstract void Ready();

        internal abstract void Free();
    }

    public class Music : PlayableSound
    {
        string filename;
        bool quickplay = true;
        bool nowused = false;
        public bool PlayReady
        {
            get => quickplay;
            set
            {
                if (quickplay == value) return;
                if (nowused)
                {
                    quickplay = value; return;
                }
                if (quickplay && this.sound == IntPtr.Zero)
                {
                    this.Ready();
                }
                else if (!quickplay && this.sound != IntPtr.Zero)
                {
                    this.Free();
                }
                quickplay = value;
            }
        }

        public bool Using
        {
            get => nowused;
            internal set
            {
                if (value == nowused) return;
                if (nowused = value)
                {
                    if (this.sound != IntPtr.Zero) return;
                    this.Ready();
                } else
                {
                    if (quickplay || this.sound == IntPtr.Zero) return;
                    this.Free();
                }
            }
        }

        public Music(string FileName)
        {
            filename = FileName;
        }

        internal override void Ready()
        {
            this.sound = SDL_mixer.Mix_LoadMUS(filename);
            if (this.sound == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"음악을 불러오는데 실패하였습니다. SDL mixer Error: {SDL_mixer.Mix_GetError()}");
        }

        internal override void Free()
        {
            SDL_mixer.Mix_FreeMusic(this.sound);
        }

        public static bool Play(Music music)
        {
            if (music.sound == IntPtr.Zero) music.Ready();
            playingmusic = music;
            if (SDL_mixer.Mix_PlayMusic(music.sound, 1) == -1) return false;
            return true;
        }

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
        public string Title { get {
                //ready 되지 않기 위해 따로 로드 (만약 quickplay 가 true라면 이미 this.sound가 intptr.zero 가 아닐것임)
                if (this.sound == IntPtr.Zero) {
                    IntPtr ptr = SDL_mixer.Mix_LoadMUS(this.filename);
                    if (ptr == IntPtr.Zero) throw new JyunrcaeaFrameworkException("음악 파일 로드에 실패했습니다.");
                    string tt = SDL_mixer.Mix_GetMusicTitle(ptr);
                    SDL_mixer.Mix_FreeMusic(ptr);
                    return tt;
                }
                return SDL_mixer.Mix_GetMusicTitle(this.sound);
            }
        }

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
            if (NowPlaying != null) NowPlaying.Free();
            if (MusicFinished != null) MusicFinished();
        }

        public static FunctionWhenMusicFinished? MusicFinished = null;
    }

    public delegate Music? FunctionWhenMusicFinished();

    /// <summary>
    /// 객체의 기본이 되는 객체 인터페이스입니다. (사실 추상 클래스이긴 하지만...)
    /// </summary>
    public abstract class ObjectInterface
#if  DEBUG
        : Events.ODDInterface
#endif
    {
        public abstract void Resize();
        public abstract void Stop();
        internal abstract void Draw();
        public abstract void Start();
        public bool Hide = false;

        //public abstract int X { get; set; }
        //public abstract int Y { get; set; }
    }
    /// <summary>
    /// 장면의 기본이 되는 장면 인터페이스입니다. 객체 인터페이스와 모든 이벤트 인터페이스를 상속하고 있습니다. 
    /// </summary>
    public abstract class SceneInterface : ObjectInterface, AllEventInterface
    {
        public RectSize? RenderRange = null;

        /// <summary>
        /// Update 을 제외한 모든 이벤트를 무시합니다.
        /// (Start 그리고 Stop 도 제외됩니다.)
        /// </summary>
        public bool EventRejection = false;

        /// <summary>
        /// Update를 무시합니다.
        /// (주의: 애니메이션이 작동하지 않을수 있습니다.)
        /// </summary>
        public bool UpdateRejection = false;

        public abstract void DropFile(string filename);

        public abstract void Resized();

        public abstract void Update(float millisecond);

        public abstract void WindowMove();

        public abstract void KeyDown(Input.Keycode e);

        public abstract void MouseMove();

        public abstract void WindowQuit();

        public abstract void KeyUp(Input.Keycode e);

        public abstract void MouseKeyDown(Input.Mouse.Key e);

        public abstract void MouseKeyUp(Input.Mouse.Key e);

        public abstract void WindowMaximized();

        public abstract void WindowMinimized();

        public abstract void WindowRestore();

        public abstract void KeyFocusIn();

        public abstract void KeyFocusOut();

        public abstract void MouseFocusIn();

        public abstract void MouseFocusOut();
    }

    public interface DefaultObjectPositionInterface
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    /// <summary>
    /// 장면에 쓰일 객체 추상 클래스입니다.
    /// </summary>
    public abstract class DrawableObject : ObjectInterface, DefaultObjectPositionInterface
    {
        internal object? inheritobj = null;
        public object? InheritedObject => inheritobj;

        //internal int addx=0, dddy=0;

        internal SDL.SDL_Rect dst = new();

        //internal RenderPositionForScene scenepos = null!;

        internal void ResetPosition()
        {
            if (this.ox == HorizontalPositionType.Left) this.originpos.x = 0;
            else this.originpos.x = (this.ox == HorizontalPositionType.Middle ? (int)Window.wh : (int)Window.size.w);

            if (this.oy == VerticalPositionType.Top) this.originpos.y = 0;
            else this.originpos.y = (this.oy == VerticalPositionType.Middle ? (int)Window.hh : (int)Window.size.h);
            this.needresetposition = false;
            this.needresetdrawposition = true;
        }

        internal virtual void ResetDrawPosition()
        {
            this.needresetdrawposition = false;
        }

        internal int mx = 0, my = 0;

        public int X
        {
            get => mx;
            set
            {
                mx = value;
                this.needresetdrawposition = true;
            }
        }

        public int Y
        {
            get => my;
            set
            {
                my = value;
                this.needresetdrawposition = true;
            }
        }

        public int AbsoluteX => this.originpos.x + mx;

        public int AbsoluteY => this.originpos.y + my;

        internal SDL.SDL_Point originpos = new();

        internal bool needresetposition = false;

        internal bool needresetdrawposition = false;
        /// <summary>
        /// 수평 원점 위치
        /// </summary>
        private protected HorizontalPositionType ox = HorizontalPositionType.Middle;
        /// <summary>
        /// 수직 원점 위치
        /// </summary>
        private protected VerticalPositionType oy = VerticalPositionType.Middle;
        /// <summary>
        /// 좌우 그리는 방향
        /// </summary>
        private protected HorizontalPositionType dx = HorizontalPositionType.Middle;
        /// <summary>
        /// 상하 그리는 방향
        /// </summary>
        private protected VerticalPositionType dy = VerticalPositionType.Middle;
        private protected bool needresetsize = true;
        /// <summary>
        /// 수평 원점을 설정합니다.
        /// </summary>
        public HorizontalPositionType OriginX
        {
            get => ox;
            set
            {
                ox = value;
                needresetposition = true;
            }
        }
        /// <summary>
        /// 수직 원점을 설정합니다.
        /// </summary>
        public VerticalPositionType OriginY
        {
            get => oy;
            set
            {
                oy = value;
                needresetposition = true;
            }
        }

        public HorizontalPositionType DrawX
        {
            get => dx;
            set
            {
                dx = value;
                needresetposition = true;
            }
        }

        public VerticalPositionType DrawY
        {
            get => dy;
            set
            {
                dy = value;
                needresetposition = true;
            }
        }
    }

    /// <summary>
    /// 이벤트 인터페이스가 모여있는 곳입니다.
    /// (인터페이스 이름과 그 안에 있는 함수와 이름이 같습니다. 편하게 코딩하세요!)
    /// </summary>
    public class Events
    {
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
            public void KeyDown(Input.Keycode key);
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
            public void KeyUp(Input.Keycode key);
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

    /// <summary>
    /// 객체를 담을수 있는 대표적인 장면입니다. 
    /// </summary>
    public class Scene : SceneInterface, DefaultObjectPositionInterface
    {
        int mx=0, my=0;
        bool needupdatepos = false;

        /// <summary>
        /// X 좌표 입니다.
        /// 상속된 객체도 같이 움직입니다.
        /// </summary>
        public int X
        {
            get => mx;
            set
            {
                mx = value;
                needupdatepos = true;
            }
        }
        /// <summary>
        /// Y 좌표 입니다.
        /// 상속된 객체도 같이 움직입니다.
        /// </summary>
        public int Y
        {
            get => my;
            set
            {
                my = value;
                needupdatepos = true;
            }
        }

        /// <summary>
        /// 이 장면이 가지고 있는 모든 객체
        /// </summary>
        public DrawableObject[] SpriteList => sprites.ToArray();

        private protected List<DrawableObject> sprites = new();
        List<Events.DropFile> drops = new();
        List<Events.Resized> resizes = new();
        List<Events.Update> updates = new();
        List<Events.WindowMove> windowMovedInterfaces = new();
        List<Events.KeyDown> keyDownEvents = new();
        List<Events.MouseMove> mouseMoves = new();
        List<Events.WindowQuit> windowQuits = new();
        List<Events.KeyUp> keyUpEvents = new();
        List<Events.MouseKeyDown> mouseButtonDownEvents = new();
        List<Events.MouseKeyUp> mouseButtonUpEvents = new();
        List<Events.WindowRestore> windowRestores = new();
        List<Events.WindowMaximized> windowMaximizeds = new();
        List<Events.WindowMinimized> windowMinimizeds = new();
        List<Events.KeyFocusIn> keyfocusin = new();
        List<Events.KeyFocusOut> keyfocusout = new();
        List<Events.MouseFocusIn> mousefocusin = new();
        List<Events.MouseFocusOut> mousefocusout = new();

        internal void AddAtEventList(DrawableObject NewSprite)
        {
            if (NewSprite is Events.DropFile) drops.Add((Events.DropFile)NewSprite);
            if (NewSprite is Events.Resized) resizes.Add((Events.Resized)NewSprite);
            if (NewSprite is Events.Update) updates.Add((Events.Update)NewSprite);
            if (NewSprite is Events.WindowMove) windowMovedInterfaces.Add((Events.WindowMove)NewSprite);
            if (NewSprite is Events.KeyDown) keyDownEvents.Add((Events.KeyDown)NewSprite);
            if (NewSprite is Events.MouseMove) mouseMoves.Add((Events.MouseMove)NewSprite);
            if (NewSprite is Events.WindowQuit) windowQuits.Add((Events.WindowQuit)NewSprite);
            if (NewSprite is Events.KeyUp) keyUpEvents.Add((Events.KeyUp)NewSprite);
            if (NewSprite is Events.MouseKeyDown) mouseButtonDownEvents.Add((Events.MouseKeyDown)NewSprite);
            if (NewSprite is Events.MouseKeyUp) mouseButtonUpEvents.Add((Events.MouseKeyUp)NewSprite);
            if (NewSprite is Events.WindowMaximized) windowMaximizeds.Add((Events.WindowMaximized)NewSprite);
            if (NewSprite is Events.WindowMinimized) windowMinimizeds.Add((Events.WindowMinimized)NewSprite);
            if (NewSprite is Events.WindowRestore) windowRestores.Add((Events.WindowRestore)NewSprite);
            if (NewSprite is Events.KeyFocusIn) keyfocusin.Add((Events.KeyFocusIn)NewSprite);
            if (NewSprite is Events.KeyFocusOut) keyfocusout.Add((Events.KeyFocusOut)NewSprite);
            if (NewSprite is Events.MouseFocusIn) mousefocusin.Add((Events.MouseFocusIn)NewSprite);
            if (NewSprite is Events.MouseFocusOut) mousefocusout.Add((Events.MouseFocusOut)NewSprite);
        }

        internal void RemoveAtEventList(DrawableObject RemovedObject)
        {
            if (RemovedObject is Events.DropFile) drops.Remove((Events.DropFile)RemovedObject);
            if (RemovedObject is Events.Resized) resizes.Remove((Events.Resized)RemovedObject);
            if (RemovedObject is Events.Update) updates.Remove((Events.Update)RemovedObject);
            if (RemovedObject is Events.WindowMove) windowMovedInterfaces.Remove((Events.WindowMove)RemovedObject);
            if (RemovedObject is Events.KeyDown) keyDownEvents.Remove((Events.KeyDown)RemovedObject);
            if (RemovedObject is Events.MouseMove) mouseMoves.Remove((Events.MouseMove)RemovedObject);
            if (RemovedObject is Events.WindowQuit) windowQuits.Remove((Events.WindowQuit)RemovedObject);
            if (RemovedObject is Events.KeyUp) keyUpEvents.Remove((Events.KeyUp)RemovedObject);
            if (RemovedObject is Events.MouseKeyDown) mouseButtonDownEvents.Remove((Events.MouseKeyDown)RemovedObject);
            if (RemovedObject is Events.MouseKeyUp) mouseButtonUpEvents.Remove((Events.MouseKeyUp)RemovedObject);
            if (RemovedObject is Events.WindowMaximized) windowMaximizeds.Remove((Events.WindowMaximized)RemovedObject);
            if (RemovedObject is Events.WindowMinimized) windowMinimizeds.Remove((Events.WindowMinimized)RemovedObject);
            if (RemovedObject is Events.WindowRestore) windowRestores.Remove((Events.WindowRestore)RemovedObject);
            if (RemovedObject is Events.KeyFocusIn) keyfocusin.Remove((Events.KeyFocusIn)RemovedObject);
            if (RemovedObject is Events.KeyFocusOut) keyfocusout.Remove((Events.KeyFocusOut)RemovedObject);
            if (RemovedObject is Events.MouseFocusIn) mousefocusin.Remove((Events.MouseFocusIn)RemovedObject);
            if (RemovedObject is Events.MouseFocusOut) mousefocusout.Remove((Events.MouseFocusOut)RemovedObject);
        } 

        /// <summary>
        /// 장면 위에 그릴수 있는 객체를 원하는 범위에 추가합니다. 
        /// </summary>
        /// <param name="NewSprite">그릴수 있는 객체(sprite, textbox, rectangle 등)</param>
        /// <param name="Index">추가할 범위 (음수일경우 맨 마지막에서 횟수만큼 앞으로 가서 추가합니다, 즉 -2 일경우 마지막(-1)에서 앞으로 한칸입니다.)</param>
        /// <exception cref="JyunrcaeaFrameworkException">잘못된 위치값을 넣었거나, 이미 다른 장면에 객체를 추가한 경우 발생하는 예외입니다.</exception>
        public void AddSprite(DrawableObject NewSprite, int Index)
        {
            if (NewSprite.InheritedObject != null) throw new JyunrcaeaFrameworkException("이 객체는 이미 다른 장면에 추가되었습니다.");
            NewSprite.inheritobj = this;
            AddAtEventList(NewSprite);
            if (Index < 0)
            {
                if (Index == -1) {
                    sprites.Add(NewSprite);
                    //objectlist.Add(sp);
                }
                else
                {
                    int i = sprites.Count + Index;
                    if (i < 0) throw new JyunrcaeaFrameworkException($"존재하지 않는 범위입니다. (입력한 범위: {Index}, 선택된 범위: {i}, 리스트 갯수: {sprites.Count})");
                    sprites.Insert(i, NewSprite);
                    //objectlist.Insert(i, sp);
                }
            }
            else {
                sprites.Insert(Index, NewSprite);
                //objectlist.Insert(index, sp);
            }
            if (Framework.Running)
                NewSprite.Start();
        }
        /// <summary>
        /// 장면에 있는 객체를 삭제합니다.
        /// </summary>
        /// <param name="RemovedObject">삭제할 동일 객체</param>
        /// <returns>잘 제거된 경우 true를 반환, 만약 장면에 존재하지 않는 객체일경우 false를 반환합니다.</returns>
        public bool RemoveSprite(DrawableObject RemovedObject)
        {
            if (!sprites.Remove(RemovedObject)) return false;
            RemovedObject.inheritobj = null;
            RemoveAtEventList(RemovedObject);
            RemovedObject.Stop();
            return true;
        }

        /// <summary>
        /// 장면 위에 그릴수 있는 객체를 (추가된 객체들 뒤에) 추가합니다.
        /// </summary>
        /// <param name="sp">그릴수 있는 객체(sprite, textbox, rectangle 등)</param>
        /// <returns>객체가 리스트에 저장된 위치를 반환합니다.</returns>
        public int AddSprite(DrawableObject sp)
        {
            AddSprite(sp, -1);
            return sprites.Count - 1;
        }

        /// <summary>
        /// 장면 위에 그릴수 있는 객체 여러개를 추가합니다.
        /// </summary>
        /// <param name="sp">그릴수 있는 객체들(sprite, textbox, rectangle 등)</param>
        public void AddSprites(params DrawableObject[] sp)
        {
            if (sp.Length == 0) return;
            for (int i = 0; i < sp.Length; i++)
            {
                AddSprite(sp[i]);
            }
        }

        //int mx = 0, my = 0;
        //bool position_changed = false;

        ////장면의 X 좌표.
        //public override int X {
        //    get => mx;
        //    set { position_changed = true; mx = value; } }

        //public override int Y { get => my; set { position_changed = true; my = value; } }

#if DEBUG
        internal override void ODD()
        {
            SDL.SDL_SetRenderDrawColor(Framework.renderer, Debug.ObjectDrawDebugingLineColor.Red, Debug.ObjectDrawDebugingLineColor.Green, Debug.ObjectDrawDebugingLineColor.Blue, Debug.ObjectDrawDebugingLineColor.Alpha);
            for (int i = 0; i < sprites.Count; i++)
            {
                if (sprites[i].Hide) continue;
                sprites[i].ODD();
            }
            SDL.SDL_SetRenderDrawColor(Framework.renderer, Debug.SceneDrawDebugingLineColor.Red, Debug.SceneDrawDebugingLineColor.Green, Debug.SceneDrawDebugingLineColor.Blue, Debug.SceneDrawDebugingLineColor.Alpha);
            if (this.RenderRange == null) SDL.SDL_RenderDrawRect(Framework.renderer, ref Window.size);
            else SDL.SDL_RenderDrawRect(Framework.renderer, ref this.RenderRange.size);
        }
#endif

        public override void WindowMaximized()
        {
            for (int i=0;i<windowMaximizeds.Count;i++)
            {
                windowMaximizeds[i].WindowMaximized();
            }
        }

        public override void WindowMinimized()
        {
            for (int i = 0; i < windowMinimizeds.Count; i++)
            {
                windowMinimizeds[i].WindowMinimized();
            }
        }

        public override void WindowRestore()
        {
            for (int i = 0; i < windowRestores.Count; i++)
            {
                windowRestores[i].WindowRestore();
            }
        }

        public override void Start()
        {
            for (int i = 0; i < sprites.Count; i++)
                sprites[i].Start();
        }

        public override void Resize()
        {
            for (int i = 0; i < sprites.Count; i++)
                sprites[i].Resize();
        }

        public override void Stop()
        {
            for (int i = 0; i < sprites.Count; i++)
                sprites[i].Stop();
        }

        internal virtual void UpdatePosToObjects()
        {
            for (int i = 0; i < sprites.Count; i++)
                sprites[i].needresetdrawposition = true;
            needupdatepos = false;
        }

        public override void Update(float ms)
        {
            for (int i = 0; i < updates.Count; i++)
                updates[i].Update(ms);
        }

        internal override void Draw()
        {
            if (this.Hide) return;
            if (needupdatepos) UpdatePosToObjects();
            if (this.RenderRange == null) SDL.SDL_RenderSetViewport(Framework.renderer, ref Window.size);
            else SDL.SDL_RenderSetViewport(Framework.renderer, ref this.RenderRange.size);
            for (int i = 0; i < this.sprites.Count; i++)
            {
                if (sprites[i].Hide) continue;
                //if (this.resetpos) sprites[i].needresetposition = true;
                sprites[i].Draw();
            }
            //if (this.resetpos) resetpos = false;
        }

        public override void DropFile(string filename)
        {
            for (int i = 0; i < drops.Count; i++)
                drops[i].DropFile(filename);
        }

        public override void Resized()
        {
            for (int i = 0; i < resizes.Count; i++)
                resizes[i].Resized();
        }

        public override void WindowMove()
        {
            for (int i = 0; i < windowMovedInterfaces.Count; i++)
                windowMovedInterfaces[i].WindowMove();
        }

        public override void KeyDown(Input.Keycode e)
        {
            for (int i = 0; i < keyDownEvents.Count; i++)
                keyDownEvents[i].KeyDown(e);
        }

        public override void MouseMove()
        {
            for (int i = 0; i < mouseMoves.Count; i++)
                mouseMoves[i].MouseMove();
        }

        public override void WindowQuit()
        {
            for (int i = 0; i < windowQuits.Count; i++)
                windowQuits[i].WindowQuit();
        }

        public override void KeyUp(Input.Keycode e)
        {
            for (int i = 0; i < keyUpEvents.Count; i++)
                keyUpEvents[i].KeyUp(e);
        }

        public override void MouseKeyDown(Input.Mouse.Key e)
        {
            for (int i = 0; i < mouseButtonDownEvents.Count; i++)
                mouseButtonDownEvents[i].MouseKeyDown(e);
        }

        public override void MouseKeyUp(Input.Mouse.Key e)
        {
            for (int i = 0; i < mouseButtonUpEvents.Count; i++)
                mouseButtonUpEvents[i].MouseKeyUp(e);
        }

        public override void KeyFocusIn()
        {
            for (int i = 0; i < keyfocusin.Count; i++)
                keyfocusin[i].KeyFocusIn();
        }

        public override void KeyFocusOut()
        {
            for (int i = 0; i < keyfocusout.Count; i++)
                keyfocusout[i].KeyFocusOut();
        }

        public override void MouseFocusIn()
        {
            for (int i = 0; i < mousefocusin.Count; i++)
                mousefocusin[i].MouseFocusIn();
        }

        public override void MouseFocusOut()
        {
            for (int i = 0; i < mousefocusout.Count; i++)
                mousefocusout[i].MouseFocusOut();
        }
    }

    /// <summary>
    /// 직접 도형 및 이미지를 그리는 장면입니다.
    /// 많은 요소들을 빠르게 그려야될때 쓰기 좋습니다.
    /// </summary>
    public abstract class Canvas : SceneInterface
    {
        List<DrawableTexture> textures = new();

        bool ready = false;

        /// <summary>
        /// 이 캔버스에서 사용할 텍스쳐를 추가합니다.
        /// </summary>
        /// <param name="t">텍스쳐</param>
        public void AddUsingTexture(DrawableTexture t)
        {
            textures.Add(t);
            if (ready)
            {
                t.Ready();
            }
        }

        /// <summary>
        /// 이 캔버스에서 추가해놓은 사용하지 않을 텍스쳐를 제거합니다.
        /// </summary>
        /// <param name="t">텍스쳐</param>
        /// <returns></returns>
        public bool RemoveUsingTexture(DrawableTexture t)
        {
            if (!textures.Remove(t)) return false;
            if (ready)
            {
                t.Free();
            }
            return true;
        }

        /// <summary>
        /// 캔버스 전용 그림도구입니다.
        /// </summary>
        protected static class Renderer
        {
            public enum BlendType
            {
                None = SDL.SDL_BlendMode.SDL_BLENDMODE_NONE,
                Blend = SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND,
                Add = SDL.SDL_BlendMode.SDL_BLENDMODE_ADD,
                Mul = SDL.SDL_BlendMode.SDL_BLENDMODE_MUL,
                Invalid = SDL.SDL_BlendMode.SDL_BLENDMODE_INVALID
            }

            public static bool Rectangle(RectSize size, Color color)
            {
                SDL.SDL_SetRenderDrawColor(Framework.renderer, color.Red, color.Green, color.Blue, color.Alpha);
                return SDL.SDL_RenderFillRect(Framework.renderer, ref size.size) == 0;
            }

            public static bool Rectangle(int width, int height, int x, int y, byte red, byte green, byte blue, byte alpha)
            {
                SDL.SDL_SetRenderDrawColor(Framework.renderer, red, green, blue, alpha);
                SDL.SDL_Rect rt = new() { w = width, h = height, x = x, y = y };
                return SDL.SDL_RenderFillRect(Framework.renderer, ref rt) == 0;
            }

            public static bool RoundedRectangle(short X, short Y,short Width, short Height,  short Radius, byte Red=255, byte Green=255, byte Blue =255, byte Alpha = 255)
            {
                return SDL_gfx.roundedBoxRGBA(Framework.renderer,X,Y, (short)(X + Width), (short)(Y+Height),Radius,Red,Green,Blue,Alpha) == 0;
            }

            public static bool RoundedRectangle(RectSize Size = default!, short Radius = 0, Color Color = default!)
            {
                return SDL_gfx.roundedBoxRGBA(Framework.renderer, (short)Size.size.x, (short)Size.size.y, (short)(Size.size.x + Size.size.w), (short)(Size.size.y + Size.size.h), Radius, Color.Red, Color.Green, Color.Blue, Color.Alpha) == 0;
            }

            public static bool RoundedRectangleLine(short width, short height, short x, short y, short radius, byte red, byte green, byte blue, byte alpha)
            {
                return SDL_gfx.roundedRectangleRGBA(Framework.renderer, x, y, (short)(x + width), (short)(y + height), radius, red, green, blue, alpha) == 0;
            }

            public static bool Texture(DrawableTexture texture, RectSize size)
            {
                return SDL.SDL_RenderCopy(Framework.renderer, texture.texture, ref texture.src, ref size.size) == 0;
            }

            public static bool BlendMode(BlendType blendType)
            {
                return SDL.SDL_SetRenderDrawBlendMode(Framework.renderer, (SDL.SDL_BlendMode)blendType) == 0;
            }

            /// <summary>
            /// 속이 채워진 원을 출력합니다.
            /// </summary>
            /// <param name="X">원의 중심 x좌표</param>
            /// <param name="Y">원의 중심 y좌표</param>
            /// <param name="Radius">반지름</param>
            /// <param name="Red">빨간색</param>
            /// <param name="Green">초록색</param>
            /// <param name="Blue">파랑색</param>
            /// <param name="Alpha">투명도</param>
            /// <returns>성공 여부</returns>
            public static bool Circle(short X,short Y,short Radius,byte Red,byte Green,byte Blue,byte Alpha)
            {
                return SDL_gfx.filledCircleRGBA(Framework.renderer, X, Y, Radius, Red, Green, Blue, Alpha) == 0;
            }

            /// <summary>
            /// 원의 테두리를 출력합니다.
            /// </summary>
            /// <param name="X">원의 중심 x좌표</param>
            /// <param name="Y">원의 중심 y좌표</param>
            /// <param name="Radius">반지름</param>
            /// <param name="Red">빨간색</param>
            /// <param name="Green">초록색</param>
            /// <param name="Blue">파랑색</param>
            /// <param name="Alpha">투명도</param>
            /// <returns>성공 여부</returns>
            public static bool CircleLine(short X, short Y, short Radius, byte Red, byte Green, byte Blue, byte Alpha)
            {
                return SDL_gfx.circleRGBA(Framework.renderer, X, Y, Radius, Red, Green, Blue, Alpha) == 0;
            }

            /// <summary>
            /// 속이 채워진 삼각형을 그립니다.
            /// </summary>
            /// <param name="AX">첫번쨰 꼭짓점의 X좌표</param>
            /// <param name="AY">첫번쨰 꼭짓점의 Y좌표</param>
            /// <param name="BX">두번쨰 꼭짓점의 X좌표</param>
            /// <param name="BY">두번쨰 꼭짓점의 Y좌표</param>
            /// <param name="CX">세번쨰 꼭짓점의 X좌표</param>
            /// <param name="CY">세번쨰 꼭짓점의 Y좌표</param>
            /// <param name="Red">빨간색</param>
            /// <param name="Green">초록색</param>
            /// <param name="Blue">파란색</param>
            /// <param name="Alpha">투명도 (255가 불투명)</param>
            /// <returns></returns>
            public static bool Triangle(short AX, short AY, short BX, short BY, short CX, short CY,byte Red, byte Green, byte Blue, byte Alpha)
            {
                return SDL_gfx.filledTrigonRGBA(Framework.renderer, AX, AY, BX, BY, CX, CY, Red, Green, Blue, Alpha) == 0;
            }
        }

        public abstract void Render();

        internal override void Draw()
        {
            if (this.RenderRange == null) SDL.SDL_RenderDrawRect(Framework.renderer, ref Window.size);
            else SDL.SDL_RenderDrawRect(Framework.renderer, ref this.RenderRange.size);
            Render();
            SDL.SDL_SetRenderDrawBlendMode(Framework.renderer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
        }

        public override void DropFile(string filename)
        {
        }

        public override void KeyDown(Input.Keycode e)
        {
        }

        public override void KeyUp(Input.Keycode e)
        {
        }

        public override void MouseKeyDown(Input.Mouse.Key e)
        {
        }

        public override void MouseKeyUp(Input.Mouse.Key e)
        {
        }

        public override void MouseMove()
        {
        }

        public override void Resize()
        {
        }

        public override void Resized()
        {
        }

        public override void Start()
        {
            ready = true;
            for (int i = 0; i < textures.Count; i++)
            {
                textures[i].Ready();
            }
        }

        public override void Stop()
        {
            ready = false;
            for (int i = 0; i < textures.Count; i++)
            {
                textures[i].Free();
            }
        }

        public override void Update(float millisecond)
        {
        }

        public override void WindowMove()
        {
        }

        public override void WindowQuit()
        {
        }

        public override void WindowRestore()
        {
        }

        public override void WindowMinimized()
        {
        }

        public override void WindowMaximized()
        {
        }

        public override void KeyFocusIn()
        {
        }

        public override void KeyFocusOut()
        {
        }

        public override void MouseFocusIn()
        {
        }

        public override void MouseFocusOut()
        {
        }

#if DEBUG
        internal override void ODD()
        {
            SDL.SDL_SetRenderDrawColor(Framework.renderer, Debug.SceneDrawDebugingLineColor.Red, Debug.SceneDrawDebugingLineColor.Green, Debug.SceneDrawDebugingLineColor.Blue, Debug.SceneDrawDebugingLineColor.Alpha);
            if (this.RenderRange == null) SDL.SDL_RenderDrawRect(Framework.renderer, ref Window.size);
            else SDL.SDL_RenderDrawRect(Framework.renderer, ref this.RenderRange.size);
        }
#endif
    }

    /// <summary>
    /// 직사각형을 출력하는 객체입니다.
    /// </summary>
    public class Rectangle : DrawableObject, CanGetLenght
    {
        public Rectangle() { }

        public int Width { get => dst.w; set {
                dst.w = value;
                needresetsize = true;
            }
        }
        public int Height { get => dst.h; set {
                dst.h = value;
                needresetsize = true;
            }
        }

        public uint UWidth { get => (uint)dst.w; set
            {
                dst.w = (int)value;
                needresetsize = true;
            }
        }

        public uint UHeight { get => (uint)dst.h; set
            {
                dst.h = (int)value;
                needresetsize = true;
            }
        }

        public short Radius = 0;

        int px = 0, py = 0;

        public Color Color = new();

        //internal SDL.SDL_Rect dst = new() {  w = 0, h = 0, x = 0, y = 0 };

        public Rectangle(uint Width, uint Height)
        {
            this.dst.w = (int)Width;
            this.dst.h = (int)Height;
            needresetsize = true;
        }

        public Rectangle(int Width, int Height)
        {
            this.dst.w = Width;
            this.dst.h = Height;
            needresetsize = true;
        }

        internal override void ResetDrawPosition()
        {
            this.dst.x = this.px + this.originpos.x + this.mx + ((DefaultObjectPositionInterface)this.inheritobj!).X;
            this.dst.y = this.py + this.originpos.y + this.my + ((DefaultObjectPositionInterface)this.inheritobj!).Y;
            needresetdrawposition = false;
        }

        internal override void Draw()
        {
            if (needresetposition) this.ResetPosition();
            if (needresetsize) this.ResetSize();
            if (needresetdrawposition) ResetDrawPosition();
            if (this.Radius == 0)
            {
                if (SDL.SDL_SetRenderDrawColor(Framework.renderer, this.Color.Red, this.Color.Green, this.Color.Blue, this.Color.Alpha) < 0) throw new JyunrcaeaFrameworkException($"색 변경에 실패하였습니다. (SDL Error: {SDL.SDL_GetError()})");
                if (SDL.SDL_RenderFillRect(Framework.renderer, ref dst) == -1) throw new JyunrcaeaFrameworkException($"직사각형 렌더링에 실패하였습니다. (SDL Error: {SDL.SDL_GetError()})");
            }
            else
            {
                if (SDL_gfx.roundedBoxRGBA(Framework.renderer, (short)dst.x, (short)dst.y, (short)(dst.x+dst.w), (short)(dst.y+dst.h),this.Radius,this.Color.Red,this.Color.Green,this.Color.Blue,this.Color.Alpha) != 0) throw new JyunrcaeaFrameworkException("둥근 직사각형 렌더링에 실패하였습니다. ()");
            }
        }

#if DEBUG
        internal override void ODD()
        {
            SDL.SDL_RenderDrawRect(Framework.renderer, ref dst);
        }
#endif

        private void ResetSize()
        {
            if (this.dx != HorizontalPositionType.Right) this.px = (int)(this.dst.w * (this.dx == HorizontalPositionType.Middle ? -0.5f : -1f));
            else
                this.px = 0;
            if (this.dy != VerticalPositionType.Bottom) this.py = (int)(this.dst.h * (this.dy == VerticalPositionType.Middle ? -0.5f : -1f));
            else
                this.py = 0;
            this.needresetsize = false;
            this.needresetdrawposition = true;
        }

        public override void Resize()
        {
            this.needresetposition = true;
            this.needresetsize = true;
        }

        public override void Start()
        {
            this.needresetposition = true;
            this.needresetsize = true;
        }

        public override void Stop()
        {

        }
    }

    /// <summary>
    /// 그려지지 않는 유령 객체입니다.
    /// 특정 위치와 크기를 설정한 다음 해당 객체와 닿았는가 판단하거나, 거리 등을 계산할때 쓰이도록 만들어둔 계산용 객체입니다.
    /// </summary>
    public class GhostObject : DrawableObject, CanGetLenght
    {

        int px = 0, py = 0;

        public GhostObject(int X = 0, int Y = 0, int Width = 0, int Height = 0)
        {
            this.dst = new() { x = X, y = Y, w = Width, h = Height };
        }

        public override void Start()
        {

        }

        public int Width
        {
            get => dst.w; set
            {
                dst.w = value;
                needresetsize = true;
            }
        }
        public int Height
        {
            get => dst.h; set
            {
                dst.h = value;
                needresetsize = true;
            }
        }

        public uint UWidth
        {
            get => (uint)dst.w; set
            {
                dst.w = (int)value;
                needresetsize = true;
            }
        }

        public uint UHeight
        {
            get => (uint)dst.h; set
            {
                dst.h = (int)value;
                needresetsize = true;
            }
        }

        private void ResetSize()
        {
            if (this.dx != HorizontalPositionType.Right) this.px = (int)(this.dst.w * (this.dx == HorizontalPositionType.Middle ? -0.5f : -1f));
            else
                this.px = 0;
            if (this.dy != VerticalPositionType.Bottom) this.py = (int)(this.dst.h * (this.dy == VerticalPositionType.Middle ? -0.5f : -1f));
            else
                this.py = 0;
            this.needresetsize = false;
            this.needresetdrawposition = true;
        }

        public override void Resize()
        {
            this.needresetposition = true;
            this.needresetsize = true;
        }

        internal override void Draw()
        {
            if (needresetposition) ResetPosition();
            if (needresetsize) ResetSize();
            if (needresetdrawposition)
            {
                this.dst.x = this.px + this.originpos.x + this.mx;
                this.dst.y = this.py + this.originpos.y + this.my;
                this.needresetdrawposition = false;
            }
        }

        public override void Stop()
        {

        }

#if DEBUG
        internal override void ODD()
        {
            SDL.SDL_RenderDrawRect(Framework.renderer, ref dst);
        }
#endif
    }

    /// <summary>
    /// 오브젝트들 끼리 묶는 용도로 이용됩니다.
    /// </summary>
    public class GroupObject : DrawableObject
    {
        List<DrawableObject> sprites = new();

        public SceneInterface? InheritedScene
        {
            get
            {
                if (this.inheritobj == null) return null;
                if (this.inheritobj is GroupObject)
                {
                    return ((GroupObject)this.inheritobj).InheritedScene;
                }
                return (SceneInterface)this.inheritobj;
            }
        }

        internal void InheritToScene(DrawableObject obj)
        {
            if (this.inheritobj is GroupObject)
            {
                ((GroupObject)this.inheritobj).InheritToScene(obj);
                return;
            }
            if (this.inheritobj is Scene)
            {
                ((Scene)this.inheritobj).AddAtEventList(obj);
                return;
            }
            throw new JyunrcaeaFrameworkException("장면 또는 그룹 객체에 상속되지 않은 그룹 객체입니다.\n그룹 객체가 소유한 객체들을 장면에 추가할수 없습니다.");
        }

        public GroupObject(int X = 0, int Y = 0)
        {
            this.X = X;
            this.Y = Y;
        }

        public int AddSprite(DrawableObject NewSprite)
        {
            this.sprites.Add(NewSprite);
            NewSprite.inheritobj = this;
            if (alreadyload)
            {
                InheritToScene(NewSprite);
                NewSprite.Start();
            }
            return this.sprites.Count - 1;
        }

        bool alreadyload = false;

        public override void Start()
        {
            for (int i=0;i<sprites.Count;i++)
            {
                InheritToScene(sprites[i]);
                sprites[i].Start();
            }
            alreadyload = true;
        }

        public override void Resize()
        {
            this.needresetposition = true;
            this.needresetsize = true;
            for (r = 0;r<sprites.Count;r++)
            {
                sprites[r].needresetdrawposition = true;
                sprites[r].Resize();
            }
        }

        int d,r;

        //SDL.SDL_Rect drawrect=new();

        internal override void Draw()
        {
            if (needresetposition) ResetPosition();
            if (needresetdrawposition)
            {
                this.dst.x = this.originpos.x + this.mx;
                this.dst.y = this.originpos.y + this.my;
                this.needresetdrawposition = false;
            }
            for (d = 0;d<sprites.Count;d++)
            {
                sprites[d].Draw();
            }
        }

        public override void Stop()
        {
            alreadyload = false;
            for(int i=0;i<sprites.Count;i++)
            {
                sprites[i].Stop();
            }
        }

#if DEBUG
        internal override void ODD()
        {
            SDL.SDL_RenderDrawRect(Framework.renderer, ref dst);
            for (d=0;d<sprites.Count;d++)
            {
                sprites[d].ODD();
            }
        }
#endif
    }

    public class GroupObjectForAnimation : GroupObject, Events.Update
    {
        /// <summary>
        /// 객체의 이동을 관리합니다.
        /// </summary>
        public MoveAnimationManager MoveAnimationState = new();
        /// <summary>
        /// 원하는 좌표로 부드럽게 이동합니다.
        /// </summary>
        /// <param name="x">도착지점의 x좌표</param>
        /// <param name="y">도착지점의 y좌표</param>
        /// <param name="AnimationTime">이동시간 (밀리초)</param>
        /// <param name="StartupDelay">시작 지연시간 (밀리초)</param>
        public void Move(int? x, int? y, float AnimationTime = 0f, float StartupDelay = 0f)
        {

            if (AnimationTime == 0f && StartupDelay == 0f)
            {
                if (x != null) base.X = (int)x;
                if (y != null) base.Y = (int)y;
                return;
            }
            MoveAnimationState.bpx = base.X;
            MoveAnimationState.bpy = base.Y;
            MoveAnimationState.Start(x, y, AnimationTime, StartupDelay);
        }

        public virtual void Update(float ms)
        {
            if (!MoveAnimationState.Complete) { MoveAnimationState.Update(ref base.mx, ref base.my); base.needresetdrawposition = true; }
        }
    }

    /// <summary>
    /// 이미지를 출력하는 객체입니다.
    /// </summary>
    public class Sprite : DrawableObject, IDisposable, CanGetLenght
    {

        DrawableTexture targettexture = null!;

        /// <summary>
        /// 출력할 이미지를 지정합니다.
        /// </summary>
        public DrawableTexture Texture
        {
            get => targettexture;
            set
            {
                if (this.targettexture.texture != IntPtr.Zero)
                {
                    this.targettexture.Free();
                    (targettexture = value).Ready();
                    dst.w = (int)(targettexture.src.w * this.sz);
                    dst.h = (int)(targettexture.src.h * this.sz);
                    this.ResetPosition();
                    this.ResetSize();
                }
                else targettexture = value;
            }
        }

        public byte TextureOpacity
        {
            get => targettexture.Opacity;
            set => targettexture.Opacity = value;
        }
        int px = 0, py = 0;
        /// <summary>
        /// 수평 원점을 설정합니다.
        /// </summary>
        /// 
        public double Rotation = 0;
        /// <summary>
        /// 해당 객체의 너비
        /// (설정시 Size가 -1로 초기화됩니다.)
        /// (설정한 이후, 그 길이가 계속 고정됩니다. 해제할려면 Size 값을 수정하세요.)
        /// </summary>
        public int Width { get => dst.w; set { dst.w = value; sz = -1; this.targettexture.needresettexture = true; } }
        /// <summary>
        /// 해당 객체의 높이
        /// (설정시 Size가 -1로 초기화됩니다.)
        /// (설정한 이후, 그 길이가 계속 고정됩니다. 해제할려면 Size 값을 수정하세요.)
        /// </summary>
        public int Height { get => dst.h; set { dst.h = value; sz = -1; this.targettexture.needresettexture = true; } }

        double sz = 1;
        /// <summary>
        /// 원본 크기에 비례해 크기를 설정합니다.
        /// (설정시 Width,Height가 Size값에 맞게 초기화됩니다.)
        /// </summary>
        public double Size
        {
            get => sz; set
            {
                sz = value;
                dst.w = (int)(targettexture.src.w * value);
                dst.h = (int)(targettexture.src.h * value);
                needresetsize = true;
            }
        }

        bool fh = false, fv = false;
        /// <summary>
        /// 좌우로 뒤집을지 결정합니다.
        /// </summary>
        public bool FlipHorizontal { get => fh; set => flip = ((fh = value) ? SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE) | (fv ? SDL.SDL_RendererFlip.SDL_FLIP_VERTICAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE); }
        /// <summary>
        /// 상하로 뒤집을지 결정합니다.
        /// </summary>
        public bool FlipVertical { get => fv; set => flip = (fh ? SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE) | ((fv = value) ? SDL.SDL_RendererFlip.SDL_FLIP_VERTICAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE); }

        SDL.SDL_RendererFlip flip = SDL.SDL_RendererFlip.SDL_FLIP_NONE;

        /// <summary>
        /// 이미지를 불러옵니다.
        /// 실패시 오류가 납니다.
        /// </summary>
        /// <exception cref="JyunrcaeaFrameworkException">이미지 불러오기 실패시</exception>
        public override void Start()
        {
            this.targettexture.Ready();
            dst.w = (int)( targettexture.src.w * this.sz);
            dst.h = (int)(targettexture.src.h * this.sz);
            this.ResetPosition();
            this.ResetSize();
        }

        /// <summary>
        /// 이미지를 그릴 위치를 계산하고 저장합니다.
        /// 당장 계산된 위치를 이용해야된다면 먼저 호출해주세요.
        /// </summary>
        public override void Resize()
        {
            this.needresetposition = true;
            this.needresetsize = true;
        }

        void ResetSize()
        {
            if (this.dx != HorizontalPositionType.Right) this.px = (int)(this.dst.w * (this.dx == HorizontalPositionType.Middle ? -0.5f : -1f));
            else this.px = 0;
            if (this.dy != VerticalPositionType.Bottom) this.py = (int)(this.dst.h * (this.dy == VerticalPositionType.Middle ? -0.5f : -1f));
            else this.py = 0;
            needresetsize = false;
            needresetdrawposition = true;
        }

        public void Dispose()
        {
            Stop();
        }

        public override void Stop()
        {
            this.targettexture.Free();
        }

        internal override void ResetDrawPosition()
        {
            this.dst.x = this.px + this.originpos.x + this.mx + ((DefaultObjectPositionInterface)this.inheritobj!).X;
            this.dst.y = this.py + this.originpos.y + this.my + ((DefaultObjectPositionInterface)this.inheritobj!).Y;
            this.needresetdrawposition = false;
        }

        internal override void Draw()
        {
            if (this.targettexture.needresettexture)
            {
                if (this.Size != -1)
                {
                    dst.w = (int)(targettexture.src.w * this.sz);
                    dst.h = (int)(targettexture.src.h * this.sz);
                }
                needresetsize =true;
                this.targettexture.needresettexture = false;
            }
            if (needresetposition) ResetPosition();
            if (needresetsize) ResetSize();
            if (needresetdrawposition) ResetDrawPosition();
            SDL.SDL_RenderCopyEx(Framework.renderer, this.targettexture.texture,ref this.targettexture.src,ref this.dst, Rotation, IntPtr.Zero, flip);
        }

        public Sprite() { }

        public Sprite(DrawableTexture texture)
        {
            this.targettexture = texture;
        }

        public Sprite(string ImageFilename)
        {
            this.targettexture = new TextureFromFile(ImageFilename);
        }

#if DEBUG
        internal override void ODD()
        {
            SDL.SDL_RenderDrawRect(Framework.renderer,ref dst);
        }
#endif
    }

    #region 애니메이션용 객체들
    //ㅈㅂ 다중상속 나와라...

    public class TextboxForAnimation : TextBox, ObjectWithSupportAnimation
    {
        public TextboxForAnimation(string filename,int size,string text="") : base(filename,size,text)
        {

        }

        public TextboxForAnimation(string filename,int size,Color FontColor,Color? BackgroundColor) : base(filename,size,FontColor,BackgroundColor) { }
        /// <summary>
        /// 객체의 이동을 관리합니다.
        /// </summary>
        public MoveAnimationManager MoveAnimationState = new();
        /// <summary>
        /// 객체의 투명도 변화를 관리합니다.
        /// </summary>
        public OpacityAnimationManager OpacityAnimationState = new();
        /// <summary>
        /// 원하는 좌표로 부드럽게 이동합니다.
        /// </summary>
        /// <param name="x">도착지점의 x좌표</param>
        /// <param name="y">도착지점의 y좌표</param>
        /// <param name="AnimationTime">이동시간 (밀리초)</param>
        /// <param name="StartupDelay">시작 지연시간 (밀리초)</param>
        public void Move(int? x, int? y, float AnimationTime = 0f, float StartupDelay = 0f)
        {
            
            if (AnimationTime == 0f && StartupDelay == 0f)
            {
                if(x != null) base.X = (int)x;
                if(y != null) base.Y = (int)y;
                return;
            }
            MoveAnimationState.bpx = base.X;
            MoveAnimationState.bpy = base.Y;
            MoveAnimationState.Start(x, y, AnimationTime, StartupDelay);
        }
        /// <summary>
        /// 원하는 투명도로 자연스럽게 변화합니다.
        /// </summary>
        /// <param name="Opacity">바뀔 투명도값</param>
        /// <param name="AnimationTime">이동시간 (밀리초)</param>
        /// <param name="StartupDelay">시작 지연시간 (밀리초)</param>
        public void Opacity(byte Opacity, float AnimationTime = 0f, float StartupDelay = 0f)
        {
            if (AnimationTime == 0f && StartupDelay == 0f)
            {
                base.TextOpacity = Opacity;
                return;
            }
            OpacityAnimationState.beforealpha = base.TextOpacity;
            OpacityAnimationState.Start(Opacity, AnimationTime, StartupDelay);
        }

        public virtual void Update(float ms)
        {
            //float nowtime = Framework.frametimer.ElapsedTicks * 0.0001f;
            if (!MoveAnimationState.Complete) { MoveAnimationState.Update(ref base.mx, ref base.my); base.needresetdrawposition = true; }
            if (!OpacityAnimationState.Complete) { base.TextOpacity = OpacityAnimationState.Update(); if (OpacityAnimationState.Complete && OpacityAnimationState.CompleteFunction != null) OpacityAnimationState.CompleteFunction(); }
        }
    }

    public interface ObjectWithSupportAnimation : Events.Update
    {
        public void Opacity(byte Opacity, float AnimationTime = 0f, float StartupDelay = 0f);

        public void Move(int? x, int? y, float AnimationTime = 0f, float StartupDelay = 0f);
    }

    public class RectangleForAnimation : Rectangle , ObjectWithSupportAnimation
    {
        public RectangleForAnimation(int Width = 0,int Height = 0) : base(Width,Height)
        {

        }

        /// <summary>
        /// 객체의 이동을 관리합니다.
        /// </summary>
        public MoveAnimationManager MoveAnimationState = new();
        /// <summary>
        /// 객체의 투명도 변화를 관리합니다.
        /// </summary>
        public OpacityAnimationManager OpacityAnimationState = new();
        /// <summary>
        /// 원하는 좌표로 부드럽게 이동합니다.
        /// </summary>
        /// <param name="x">도착지점의 x좌표</param>
        /// <param name="y">도착지점의 y좌표</param>
        /// <param name="AnimationTime">이동시간 (밀리초)</param>
        /// <param name="StartupDelay">시작 지연시간 (밀리초)</param>
        public void Move(int? x, int? y, float AnimationTime = 0f, float StartupDelay = 0f)
        {
            if (AnimationTime == 0f && StartupDelay == 0f)
            {
                if (x != null) base.X = (int)x;
                if (y != null) base.Y = (int)y;
                return;
            }
            MoveAnimationState.bpx = base.X;
            MoveAnimationState.bpy = base.Y;
            MoveAnimationState.Start(x, y, AnimationTime, StartupDelay);
        }
        /// <summary>
        /// 원하는 투명도로 자연스럽게 변화합니다.
        /// </summary>
        /// <param name="Opacity">바뀔 투명도값</param>
        /// <param name="AnimationTime">이동시간 (밀리초)</param>
        /// <param name="StartupDelay">시작 지연시간 (밀리초)</param>
        public void Opacity(byte Opacity, float AnimationTime = 0f, float StartupDelay = 0f)
        {
            if (AnimationTime == 0f && StartupDelay == 0f)
            {
                base.Color.Alpha = Opacity;
                return;
            }
            OpacityAnimationState.beforealpha = base.Color.Alpha;
            OpacityAnimationState.Start(Opacity, AnimationTime, StartupDelay);
        }

        public virtual void Update(float ms)
        {
            float nowtime = Framework.frametimer.ElapsedTicks * 0.0001f;
            if (!MoveAnimationState.Complete) { MoveAnimationState.Update(ref base.mx, ref base.my); base.needresetdrawposition = true; }
            if (!OpacityAnimationState.Complete) { base.Color.Alpha = OpacityAnimationState.Update(); if (OpacityAnimationState.Complete && OpacityAnimationState.CompleteFunction != null) OpacityAnimationState.CompleteFunction(); }
        }
    }

    public class SpriteForAnimation : Sprite, ObjectWithSupportAnimation
    {
        public SpriteForAnimation()
        {

        }

        public SpriteForAnimation(DrawableTexture texture) : base(texture) { }
        /// <summary>
        /// 객체의 이동을 관리합니다.
        /// </summary>
        public MoveAnimationManager MoveAnimationState = new();
        /// <summary>
        /// 객체의 투명도 변화를 관리합니다.
        /// </summary>
        public OpacityAnimationManager OpacityAnimationState = new();
        /// <summary>
        /// 원하는 좌표로 부드럽게 이동합니다.
        /// </summary>
        /// <param name="x">도착지점의 x좌표</param>
        /// <param name="y">도착지점의 y좌표</param>
        /// <param name="AnimationTime">이동시간 (밀리초)</param>
        /// <param name="StartupDelay">시작 지연시간 (밀리초)</param>
        public void Move(int? x,int? y,float AnimationTime = 0f,float StartupDelay = 0f)
        {
            if (AnimationTime == 0f && StartupDelay == 0f)
            {
                if (x != null) base.X = (int)x;
                if (y != null) base.Y = (int)y;
                return;
            }
            MoveAnimationState.bpx = base.X;
            MoveAnimationState.bpy = base.Y;
            MoveAnimationState.Start(x, y, AnimationTime, StartupDelay);
        }


        /// <summary>
        /// 원하는 투명도로 자연스럽게 변화합니다.
        /// </summary>
        /// <param name="Opacity">바뀔 투명도값</param>
        /// <param name="AnimationTime">이동시간 (밀리초)</param>
        /// <param name="StartupDelay">시작 지연시간 (밀리초)</param>
        public void Opacity(byte Opacity,float AnimationTime = 0f,float StartupDelay = 0f)
        {
            if (AnimationTime == 0f && StartupDelay == 0f)
            {
                this.TextureOpacity = Opacity;
                return;
            }
            OpacityAnimationState.beforealpha = base.TextureOpacity;
            OpacityAnimationState.Start(Opacity,AnimationTime, StartupDelay);
        }

        public virtual void Update(float ms)
        {
            if (!MoveAnimationState.Complete) { MoveAnimationState.Update(ref base.mx, ref base.my); base.needresetdrawposition = true; }
            if (!OpacityAnimationState.Complete) { base.TextureOpacity = OpacityAnimationState.Update(); if (OpacityAnimationState.CompleteFunction != null) OpacityAnimationState.CompleteFunction(); }
        }


    }

    #endregion
    /// <summary>
    /// 투명도를 관리하는 객체입니다.
    /// </summary>
    public class OpacityAnimationManager
    {
        public float StartTime { get; internal set; } = 0;
        public float ArrivalTime { get; internal set; } = 0;
        internal byte beforealpha;
        public byte TargetOpacity { get; internal set; }
        public FunctionForAnimation CalculationFunction = Animation.Type.Default;
        //음수도 저장해야되므로 그냥 int
        int distance;
        public float AnimationTime { get; internal set; } = 0;
        public bool Complete { get; internal set; } = true;
        public Action? CompleteFunction = null;

        internal void Start(byte alpha,float animationtime,float startupdelay = 0f)
        {
            //시작 시간 구하기
            StartTime = Framework.RunningTimeToFloat + startupdelay;
            //종료 시간 구하기
            ArrivalTime = StartTime + animationtime;
            //이동시간 저장
            this.AnimationTime = animationtime;
            //목표 투명도 값 저장
            TargetOpacity = alpha;
            //차이값 저장
            /*
             예시:
                현재 50, 목표 100 일경우
                100 - 50 = 50, 으로 distance 변수엔 50이 저장되고
                시간에 맞춰서 조금씩 합계하는 식으로 애니메이션 구현
            반대로:
                현재 100 목표 50 일경우
                50 - 100 = -50 이므로
                똑같이 시간에 맞춰서 기본값에 더하면 된다.
                그리고 이래서 distance는 byte변수가 아닌거임, 음수도 저장되야됨
             */
            distance = alpha - beforealpha;
            Complete = false;
        }

        internal byte Update()
        {
            float nowtime = Framework.RunningTimeToFloat - StartTime;
            if (nowtime <= 0f) return beforealpha;

            if (AnimationTime <= nowtime)
            {
                Complete = true;
                return TargetOpacity;
            }

            return (byte)(beforealpha + Math.Round(distance * CalculationFunction(nowtime / AnimationTime)));
        }
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
    /// 움직임을 관리하는 객체입니다.
    /// </summary>
    public class MoveAnimationManager
    {
        internal int bpx, bpy;
        int dx, dy;
        public float StartTime { get; internal set; }
        public bool Complete { get; internal set; } = true;
        public int TargetPositionX { get; internal set; } = 0;
        public int TargetPositionY { get; internal set; } = 0;
        public float AnimationTime { get; internal set; } = 0;
        public float ArrivalTime { get; internal set; } = 0;
        bool gx = true, gy = true;
        public FunctionForAnimation CalculationFunction = Animation.Type.Default;

        public Action? CompleteFunction = null;

        internal void Start(int? x, int? y, float AnimationTime, float StartupDelay = 0f)
        {
            if (x == null && y == null) return;
            this.StartTime = Framework.RunningTimeToFloat + StartupDelay;
            Complete = false;
            if (x == null) gx = false;
            else { gx = true; TargetPositionX = (int)x; dx = (int)x - bpx; }
            if (y == null) gy = false;
            else { gy = true; TargetPositionY = (int)y; dy = (int)y - bpy; }
            this.AnimationTime = AnimationTime;
            this.ArrivalTime = this.StartTime + AnimationTime;
        }

        public void ModifyArrivalPoint(int? x,int? y)
        {
            if (x == null && y == null) AnimationTime = StartTime = 0f;
            if (x == null) gx = false;
            else { gx = true; TargetPositionX = (int)x; dx = (int)x - bpx; }
            if (y == null) gy = false;
            else { gy = true; TargetPositionY = (int)y; dy = (int)y - bpy; }
        }

        internal void Update(ref int x, ref int y)
        {
            float nowtime = Framework.RunningTimeToFloat - StartTime;
            if (nowtime <= 0f) return;
            
            if (AnimationTime <= nowtime)
            {
                x = TargetPositionX; y = TargetPositionY;
                Complete = true;
                if (CompleteFunction != null) CompleteFunction();
                return;
            }

            double ratio = CalculationFunction(nowtime / AnimationTime);
            if (gx) x = bpx + (int)(dx * ratio);
            if (gy) y = bpy + (int)(dy * ratio);
        }
    }
    /// <summary>
    /// 글자를 출력하는 객체입니다.
    /// </summary>
    public class TextBox : DrawableObject, IDisposable, CanGetLenght
    {
        public TextBox(string filename,int size,string text = "")
        {
            this.FontfileName = filename;
            this.fontsize = size;
            this.txt = text;
        }

        public TextBox(string filename,int size,Color font_color,Color? background_color = null)
        {
            this.FontfileName = filename;
            this.fontsize = size;
            this.fc = font_color;
            this.bc = background_color;
        }

        string FontfileName = string.Empty;

        int fontsize = 16;

        IntPtr fontsource = IntPtr.Zero;

        FontStyle fs = FontStyle.Normal;

        public FontStyle FontStyle
        {
            get => fs;
            set
            {
                fs = value;
                if (fontsource == IntPtr.Zero) return;
                SDL_ttf.TTF_SetFontStyle(this.fontsource, (int)fs);
                
            }
        }

        //SDL.SDL_Rect dst = new();

        SDL.SDL_Rect src = new();

        public bool blend = true;

        public bool Blended { get => blend; set { rerender = true; blend = value; } }

        string txt = string.Empty;

        bool rerender = false;

        public string Text { get => txt; set
            {
                if (this.txt == value) return;
                this.txt = value;
                this.rerender = true;
            }
        }

        public int Height
        {
            get
            {
                if (rerender) TextRender();
                if (needresetsize) Reset();
                return this.dst.h;
            }
        }

        public int Width
        {
            get
            {
                if (rerender) TextRender();
                if (needresetsize) Reset();
                return this.dst.w;
            }
        }

        int px = 0, py = 0;

        float sz = 1;
        /// <summary>
        /// 렌더링 된 이미지의 크기를 설정합니다. (해상도가 늘거나 줄어들진 않습니다. 그대신 Size 보다 빠르게 처리됩니다.)
        /// </summary>
        public float Scale
        {
            get { return sz; }
            set
            {
                sz = value;
                dst.w = (int)(src.w * value);
                dst.h = (int)(src.h * value);
                needresetsize = true;
            }
        }

        /// <summary>
        /// 글꼴의 크기(높이 기준)를 설정합니다.
        /// (해상도가 조절됩니다.)
        /// </summary>
        public int Size
        {
            get => fontsize;
            set
            {
                fontsize = value;
                if (this.fontsource == IntPtr.Zero) return;
                if (SDL_ttf.TTF_SetFontSize(this.fontsource,fontsize) == -1) throw new JyunrcaeaFrameworkException($"글꼴 크기 조정에 실패하였습니다. (SDL ttf Error: {SDL_ttf.TTF_GetError()}, font size: {fontsize}, source: {this.fontsource.ToString()})");
                rerender = true;
            }
        }

        byte alpha=255;
        public byte TextOpacity { get => alpha; set {
                alpha = value;
                if (this.tt != IntPtr.Zero) SDL.SDL_SetTextureAlphaMod(tt, value);
            }
        }

        Color fc = new();

        Color? bc = null;
        /// <summary>
        /// 글자색
        /// </summary>
        public Color FontColor { get =>this.fc; set {
                this.fc = value;
                this.rerender = true;
            }
        }
        /// <summary>
        /// 글자 배경색 (null로 설정시 없음)
        /// </summary>
        public Color? BackgroundColor
        {
            get => this.bc;
            set
            {
                this.bc = value;
                this.rerender = true;
            }
        }
        /// <summary>
        /// 회전값
        /// </summary>
        public double Rotation = 0;
        bool fh = false, fv = false;
        /// <summary>
        /// 가로로 뒤집기 여부
        /// </summary>
        public bool FlipHorizontal { get => fh; set => flip = ((fh = value) ? SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE) | (fv ? SDL.SDL_RendererFlip.SDL_FLIP_VERTICAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE); }
        /// <summary>
        /// 세로로 뒤집기 여부
        /// </summary>
        public bool FlipVertical { get => fv; set => flip = (fh ? SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE) | ((fv = value) ? SDL.SDL_RendererFlip.SDL_FLIP_VERTICAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE); }
        SDL.SDL_RendererFlip flip = SDL.SDL_RendererFlip.SDL_FLIP_NONE;
        IntPtr tt = IntPtr.Zero;
        /// <summary>
        /// 
        /// </summary>
        uint wraplenght = 0;

        /// <summary>
        /// 글자의 너비를 제한시키며, 너비를 초과한 글자는 내려씁니다.
        /// 0으로 설정할경우 제한되지 않습니다.
        /// </summary>
        public uint WrapLenght { get => wraplenght; set { rerender = true; wraplenght = value; } }   

        public void Reset()
        {
            needresetsize = false;
            if (this.dx != HorizontalPositionType.Right) this.px = (int)(this.dst.w * (this.dx == HorizontalPositionType.Middle ? -0.5f : -1f));
            else this.px = 0;
            if (this.dy != VerticalPositionType.Bottom) this.py = (int)(this.dst.h * (this.dy == VerticalPositionType.Middle ? -0.5f : -1f));
            else this.py = 0;
            needresetdrawposition = true;
        }

        internal override void ResetDrawPosition()
        {
            this.dst.x = this.px + this.originpos.x + this.mx + ((DefaultObjectPositionInterface)this.inheritobj!).X;
            this.dst.y = this.py + this.originpos.y + this.my + ((DefaultObjectPositionInterface)this.inheritobj!).Y;
            needresetdrawposition = false;
        }

        internal override void Draw()
        {
            if (rerender) TextRender();
            if (needresetsize) Reset();
            if (needresetposition) ResetPosition();
            if (needresetdrawposition) ResetDrawPosition();

            if (this.tt != IntPtr.Zero)
            if(SDL.SDL_RenderCopyEx(Framework.renderer, this.tt, ref this.src, ref this.dst, Rotation, IntPtr.Zero, flip) != 0) throw new JyunrcaeaFrameworkException("sdl error: " + SDL.SDL_GetError());
        }

#if DEBUG
        internal override void ODD()
        {
            SDL.SDL_RenderDrawRect(Framework.renderer, ref dst);
        }
#endif

        public override void Resize()
        {
            this.needresetposition = true;
            this.needresetsize = true;
        }

        public void TextRender()
        {
            this.needresetsize = true;
            this.rerender = false;
            if (this.tt != IntPtr.Zero)
            {
                SDL.SDL_DestroyTexture(this.tt);
            }
            if (this.txt == string.Empty)
            {
                src.w = src.h = 0;
                this.tt = IntPtr.Zero;
                return;
            }
            IntPtr surface = (this.bc == null) ?
              (Blended ? SDL_ttf.TTF_RenderUTF8_Blended_Wrapped(this.fontsource, this.txt, fc.colorbase,wraplenght) : SDL_ttf.TTF_RenderUTF8_Solid_Wrapped(this.fontsource, this.txt, fc.colorbase,wraplenght)) : SDL_ttf.TTF_RenderUTF8_Shaded_Wrapped(this.fontsource, this.txt, fc.colorbase, this.bc.colorbase,wraplenght);
            if (surface == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"텍스트 렌더링에 실패하였습니다. SDL ttf Error : {SDL_ttf.TTF_GetError()}");
            this.tt = SDL.SDL_CreateTextureFromSurface(Framework.renderer, surface);
            SDL.SDL_FreeSurface(surface);
            if (this.tt == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"텍스쳐로 변환하는데 실패했습니다. SDL Error : {SDL.SDL_GetError()}");
            if (SDL.SDL_SetTextureBlendMode(this.tt, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND) < 0) throw new JyunrcaeaFrameworkException($"텍스쳐의 블랜더 모드 설정에 실패하였습니다. SDL Error: {SDL.SDL_GetError()}");
            if (alpha != 255) SDL.SDL_SetTextureAlphaMod(tt, alpha);
#if DEBUG
            Debug.TextRenderCount++;
#endif
            SDL.SDL_QueryTexture(this.tt, out _, out _, out src.w, out src.h);
            dst.w = (int)(src.w * this.sz);
            dst.h = (int)(src.h * this.sz);
        }

        public override void Stop()
        {
            SDL_ttf.TTF_CloseFont(this.fontsource);
            if (this.tt != IntPtr.Zero) { SDL.SDL_DestroyTexture(this.tt); this.tt = IntPtr.Zero; }
        }

        public override void Start()
        {
            this.fontsource = SDL_ttf.TTF_OpenFont(this.FontfileName, this.fontsize);
            if (this.fontsource == IntPtr.Zero) throw new JyunrcaeaFrameworkException(
                $"글꼴 파일을 불러오는데 실패하였습니다. 파일 경로: '{this.FontfileName}', SDL_ttf Error: {SDL_ttf.TTF_GetError()}"
                );
            SDL_ttf.TTF_SetFontStyle(this.fontsource, (int)fs);
            this.Scale = this.sz;
            this.rerender = true;
            this.needresetposition = true;
        }

        public void Dispose()
        {
            this.Stop();
        }
    }

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

        internal SDL.SDL_Color colorbase = new();

        public static readonly Color White = new();
        public static readonly Color Black = new(0,0,0,255);
        public static readonly Color Gray = new(127, 127, 127);
        
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
        public static class TextInput
        {
            public static string InputedText = string.Empty;

            public static int CursorPosition = 0;
            public static int SelectionLenght = 0;
            public static string SelectedText = string.Empty;

            static bool ti = false;
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
            a = 'a',
            b = 'b',
            c = 'c',
            d = 'd',
            e = 'e',
            f = 'f',
            g = 'g',
            h = 'h',
            i = 'i',
            j = 'j',
            k = 'k',
            l = 'l',
            m = 'm',
            n = 'n',
            o = 'o',
            p = 'p',
            q = 'q',
            r = 'r',
            s = 's',
            t = 't',
            u = 'u',
            v = 'v',
            w = 'w',
            x = 'x',
            y = 'y',
            z = 'z',

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
            return SDL.SDL_IntersectRect(ref sp1.dst, ref sp2.dst, out _) == SDL.SDL_bool.SDL_TRUE;
        }
        /// <summary>
        /// 두 객체가 서로 겹쳐진 부분을 알아냅니다. (직사각형 기준)
        /// </summary>
        /// <param name="sp1">첫번째 객체</param>
        /// <param name="sp2">두번째 객체</param>
        /// <returns>겹쳐진 부분을 반환합니다. 만약 겹쳐진 부분이 없으면 null을 반환합니다.</returns>
        public static RectSize? OverlapPart(DrawableObject sp1,DrawableObject sp2)
        {
            if (SDL.SDL_IntersectRect(ref sp1.dst, ref sp2.dst, out var r) == SDL.SDL_bool.SDL_FALSE) return null;
            return new(r.x, r.y, r.w, r.h);
        }
        /// <summary>
        /// 마우스가 객체에 닿았는지 판단합니다.
        /// </summary>
        /// <param name="Sprite">객체</param>
        /// <returns></returns>
        public static bool MouseOver(DrawableObject Sprite)
        {

            if (
                Sprite.inheritobj == null ||
                ((Sprite.inheritobj is GroupObject) ?
                    ((GroupObject)Sprite.inheritobj).InheritedScene!.RenderRange == null :
                    ((SceneInterface)Sprite.InheritedObject!).RenderRange == null )
                )
                return SDL.SDL_PointInRect(ref Input.Mouse.position, ref Sprite.dst) == SDL.SDL_bool.SDL_TRUE;
            SDL.SDL_Rect part = new()
            {
                w = Sprite.dst.w,
                h = Sprite.dst.h,
                x = ((SceneInterface)Sprite.InheritedObject!).RenderRange!.size.x + Sprite.dst.x,
                y = ((SceneInterface)Sprite.InheritedObject!).RenderRange!.size.y + Sprite.dst.y
            };
            return SDL.SDL_PointInRect(ref Input.Mouse.position, ref part) == SDL.SDL_bool.SDL_TRUE;
        }

        public static bool MouseOver(ZeneretyDrawableObject Target)
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
        public static double Distance(DrawableObject sp1,DrawableObject sp2)
        {
            int x = sp1.AbsoluteX - sp2.AbsoluteX, y = sp1.AbsoluteY - sp2.AbsoluteY;
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

                if (Framework.MultiCoreProcess)
                {
                    Parallel.ForEach(AnimationQueue, (a) => a.Update());
                    return;
                }
                foreach (var a in AnimationQueue) a.Update();

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
                public General(ZeneretyObject zo,double? st,double animatime,uint RepeatCount = 1,Action<ZeneretyObject>? fff = null,FunctionForAnimation? ffa = null)
                {
                    Target = zo;
                    FunctionForFinish = fff;
                    this.RepeatCount = RepeatCount;
                    if (ffa is not null) AnimationCalculator = ffa;
                    StartTime = st is null ? Framework.RunningTime : (double)st;
                    AnimationTime = animatime;
                }

                public ZeneretyObject Target { get; internal set; } = null!;
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
                public Action<ZeneretyObject>? FunctionForFinish { get; internal set; } = null;
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
                public Movement(ZeneretyObject Target,int? X = null,int? Y = null,double? StartTime = null, double AnimationTime = 1000,uint RepeatCount = 1, FunctionForAnimation? TimeClaculator = null, Action<ZeneretyObject>? FunctionWhenFinished = null) : base(Target,StartTime,AnimationTime,RepeatCount,FunctionWhenFinished,TimeClaculator) {
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
                public Rotation(ZeneretyExtendObject Target,double Rotate,double? StartTime,double AnimationTime, uint RepeatCount = 1,  FunctionForAnimation? TimeClaculator = null, Action<ZeneretyObject>? FunctionWhenFinished = null) : base(Target, StartTime, AnimationTime,RepeatCount, FunctionWhenFinished, TimeClaculator)
                {
                    BR = Target.Rotation;
                    RL = Rotate;
                    AR = BR + RL;
                }

                double BR, RL, AR;

                internal override void Update()
                {
                    if (CheckTime()) return;
                    ((ZeneretyExtendObject)Target).Rotation = BR + RL * Progress;
                }

                public override void Done()
                {
                    ((ZeneretyExtendObject)Target).Rotation = AR;
                    base.Done();
                }

                public override void Undo()
                {
                    ((ZeneretyExtendObject)Target).Rotation = BR;
                    base.Undo();
                }
            }

            /// <summary>
            /// 투명도와 관련된 정보를 담는 클래스
            /// </summary>
            public class Opacity : General
            {
                public Opacity(ZeneretyDrawableObject Target, byte TargetOpacity, double? StartTime = null, double AnimationTime = 1000, uint RepeatCount = 1, FunctionForAnimation? TimeCalculator = null, Action<ZeneretyObject>? FunctionWhenFinished = null) : base(Target, StartTime, AnimationTime, RepeatCount, FunctionWhenFinished, TimeCalculator)
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
                    ((ZeneretyDrawableObject)Target).Opacity = (byte)(BO + RO * Progress);
                }

                public override void Done()
                {
                    ((ZeneretyDrawableObject)Target).Opacity = AO;
                    base.Done();
                }

                public override void Undo()
                {
                    ((ZeneretyDrawableObject)Target).Opacity = BO;
                    base.Undo();
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
                protected ZeneretyList targets;
                protected double starttime;
                protected double animationtime;

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
                    for (int i = 0;i < this.targets.Count; i++)
                    {
                        if (this.targets[i] is not ZeneretyDrawableObject) continue;
                        var ai = new Info.Opacity((ZeneretyDrawableObject)this.targets[i], this.TargetOpacity, this.starttime, this.animationtime, this.RepeatCount, this.TimeCalculator);
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


    }

    /// <summary>
    /// 여러 객체에서 같은 이미지를 쓸수 있도록 텍스쳐를 공유하는 곳입니다.
    /// </summary>
    public static class TextureSharing
    {
        internal static List<SharedTextureAccessKey> resourcelist = null!;

        public static TextureFromSharedTexture Add(string filename)
        {
            SharedTextureAccessKey key = new(new TextureFromFile(filename));
            resourcelist.Add(key);
            return new(key);
        }

        public static void Remove(SharedTextureAccessKey key)
        {
            key.ForceFree();
            resourcelist.Remove(key);
        }

        internal static async void AllFree()
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < resourcelist.Count; i++)
                {
                    resourcelist[i].ForceFree();
                }
                resourcelist.Clear();
                resourcelist = null!;
            });
        }
    }

    public class SharedTextureAccessKey
    {
        TextureFromFile source = null!;

        public SharedTextureAccessKey(TextureFromFile texture)
        {
            source = texture;
        }

        public uint used { get; internal set; } = 0;

        internal void Ready()
        {
            if (used == 0)
                this.source.Ready();
            used++;
        }

        internal void Free()
        {
            if (used == 0) return;
            if (used == 1)
                this.source.Free();
            used--;
        }

        internal void ForceFree()
        {
            this.source.Free();
        }

        internal IntPtr texture => source.texture;

        internal SDL.SDL_Rect size => source.src;
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

    //internal class RenderPositionForScene
    //{
    //    internal int X,Y;
    //    internal RenderPositionForScene(int x=0,int y=0)
    //    {
    //        this.X = x;
    //        this.Y = y;
    //    }
    //}
    /// <summary>
    /// 객체가 그릴수 있는 텍스쳐의 추상 클래스입니다.
    /// </summary>
    public abstract class DrawableTexture
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

        internal virtual void Ready()
        {
            if (this.texture == IntPtr.Zero) return;
            if (SDL.SDL_SetTextureBlendMode(this.texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND) < 0) throw new JyunrcaeaFrameworkException($"텍스쳐의 블랜더 모드 설정에 실패하였습니다. SDL Error: {SDL.SDL_GetError()}");
            if (alpha != 255) SDL.SDL_SetTextureAlphaMod(texture, alpha);
            
        }

        internal abstract void Free();


    }
    /// <summary>
    /// 공유 텍스쳐를 통해 불러오는 텍스쳐입니다.
    /// </summary>
    public class TextureFromSharedTexture : DrawableTexture
    {
        

        internal SharedTextureAccessKey key = null!;

        public TextureFromSharedTexture(SharedTextureAccessKey key)
        {
            this.key = key;
        }

        internal override void Ready()
        {
            key.Ready();
            this.texture = key.texture;
            this.src = key.size;
            this.absolutesrc.x = key.size.w;
            this.absolutesrc.y = key.size.h;
            base.Ready();
        }

        internal override void Free()
        {
            key.Free();
            this.texture = IntPtr.Zero;
            this.src.w = this.src.h = 0;
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
        }

        internal override void Ready()
        {
            SDL.SDL_QueryTexture(this.texture, out _, out _, out this.absolutesrc.x, out this.absolutesrc.y);
            this.needresettexture = true;
            if (!this.FixedRenderRange)
            {
                this.src.w = this.absolutesrc.x;
                this.src.h = this.absolutesrc.y;
            }
            base.Ready();
        }

        internal override void Free()
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
        }

        public TextureFromFile() { }

        internal override void Ready()
        {
            if ((this.texture = SDL_image.IMG_LoadTexture(Framework.renderer, filename)) == IntPtr.Zero) throw new JyunrcaeaFrameworkException("SDL image Error: " + SDL.SDL_GetError());
            SDL.SDL_QueryTexture(this.texture, out _, out _, out this.absolutesrc.x, out this.absolutesrc.y);
            this.needresettexture = true;
            if (!this.FixedRenderRange)
            {
                this.src.w = this.absolutesrc.x;
                this.src.h = this.absolutesrc.y;
            }
            base.Ready();
        }

        internal override void Free()
        {
            SDL.SDL_DestroyTexture(this.texture);
            this.absolutesrc.x = this.absolutesrc.y = 0;
            this.needresettexture = true;
            this.texture = IntPtr.Zero;
        }
    }

    public class TextureFromStringForXPM : DrawableTexture
    {
        public TextureFromStringForXPM() { }

        public TextureFromStringForXPM(string[] xpmdata)
        {
            this.StringForXPM = xpmdata;
        }

        public string[] StringForXPM = null!;

        internal override void Ready()
        {
            IntPtr surface = SDL_image.IMG_ReadXPMFromArray(StringForXPM);
            if (surface == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"불러올수 없는 XPM 문자열 SDL Error: {SDL.SDL_GetError()}");
            this.texture = SDL.SDL_CreateTextureFromSurface(Framework.renderer,surface);
            SDL.SDL_FreeSurface(surface);
            if (this.texture == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"텍스쳐로 변환 실패함 SDL Error: {SDL.SDL_GetError()}");
            this.needresettexture = true;
            SDL.SDL_QueryTexture(this.texture, out _, out _, out this.absolutesrc.x, out this.absolutesrc.y);
            if (!this.FixedRenderRange)
            {
                this.src.w = this.absolutesrc.x;
                this.src.h = this.absolutesrc.y;
            }
            base.Ready();
        }

        internal override void Free()
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
        }

        public void ReRender()
        {
            if (this.texture != IntPtr.Zero) Free();
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

        internal override void Ready()
        {
            fontsource = SDL_ttf.TTF_OpenFont(Fontfile, Size);
            if (fontsource == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"불러올수 없는 글꼴 파일 SDL Error: {SDL.SDL_GetError()}");
            //SDL_ttf.TTF_SetFontHinting(fontsource, SDL_ttf.TTF_HINTING_MONO);
            Rendering();
            base.Ready();
        }

        internal void Rendering()
        {
            if (this.texture != IntPtr.Zero)
            {
                Free();
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

        internal override void Free()
        {
            SDL.SDL_DestroyTexture(this.texture);
            this.texture = IntPtr.Zero;
        }

        public void Dispose()
        {
            SDL_ttf.TTF_CloseFont(fontsource);
            if (this.texture != IntPtr.Zero) Free();
        }
    }

    public class TextureFromTextFileForXPM : DrawableTexture
    {
        public TextureFromTextFileForXPM() { }

        public TextureFromTextFileForXPM(string FilePath)
        {
            this.FilePath = FilePath;
        }

        public string FilePath = string.Empty;

        internal override void Ready()
        {
            string[] data = File.ReadAllLines(FilePath);
            //Array.Resize(ref data, data.Length + 1);
            //data[data.Length - 1] = null!;
            IntPtr surface = SDL_image.IMG_ReadXPMFromArray(data);
            if (surface == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"불러올수 없는 XPM 문자열 SDL Error: {SDL.SDL_GetError()}");
            this.texture = SDL.SDL_CreateTextureFromSurface(Framework.renderer, surface);
            SDL.SDL_FreeSurface(surface);
            if (this.texture == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"텍스쳐로 변환 실패함 SDL Error: {SDL.SDL_GetError()}");
            base.Ready();
        }

        internal override void Free()
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
        /// <summary>
        /// 화면에 출력되는 실제 픽셀 가로길이를 알아냅니다.
        /// </summary>
        /// <param name="obj">객체</param>
        /// <returns>가로 길이</returns>
        public static int DrawWidth(DrawableObject obj)
        {
            return obj.dst.w;
        }

        /// <summary>
        /// 화면에 출력되는 실제 픽셀 세로길이를 알아냅니다.
        /// </summary>
        /// <param name="obj">객체</param>
        /// <returns>세로 길이</returns>
        public static int DrawHeight(DrawableObject obj)
        {
            return obj.dst.h;
        }

        /// <summary>
        /// Canvas 기준, 실제 렌더링 위치를 알아냅니다. (객체의 왼쪽 위 모서리의 좌표)
        /// </summary>
        /// <param name="obj">객체</param>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        public static void RealPosition(DrawableObject obj,out int x,out int y)
        {
            if (obj.needresetposition)
            {
                obj.ResetPosition();
            }
            if (obj.needresetdrawposition)
            {
                obj.ResetDrawPosition();
            }
            x = obj.dst.x;
            y = obj.dst.y;
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
}