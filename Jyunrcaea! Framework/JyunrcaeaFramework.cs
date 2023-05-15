#define WINDOWS
using SDL2;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.SymbolStore;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace JyunrcaeaFramework
{
#if DEBUG
    public static class Debug
    {
        public static Version SDL2Version
        {
            get
            {
                SDL.SDL_GetVersion(out var v);
                return new(v.major, v.minor, v.patch);
            }
        }

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

        //public static bool CheckDrawTime = false;
        //internal static float[] drawtimelist = Array.Empty<float>();
        //public static float[] DrawTimeList => drawtimelist;

        static Queue<string> logginglist = new();

        public static int LogCount => logginglist.Count;

        public static void Log(string loglist,params object?[] obj)
        {
            logginglist.Enqueue($"{Framework.RunningTimeToSecond}:{string.Format(loglist,obj)}");
        }

        public static string GetLog => logginglist.Dequeue();
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
        /// CPU 사용량의 줄이는 수준을 조정합니다.
        /// 높을수록 사용량을 더 많이 줄입니다. 그대신 프레임이 더욱 불안정해질수 있습니다.
        /// </summary>
        public static byte SavingPerformanceLevel { get => (byte)((1600 - savelevel) / 2); set {
                savelevel = 1600 - value * 2;
            } 
        }

        internal static int savelevel = 1600;
        /// <summary>
        /// 업데이트와 렌더링을 서로 다른 스레드에서 작업시킵니다.
        /// 프레임이 낮은 상태에선 잘하면 나쁘지 않은 성능을 얻을순 있지만, SDL2 라이브러리에 문제를 일으킬수 있습니다.
        /// </summary>
        public static bool ThreadRender = false;
        /// <summary>
        /// 이벤트(Update, Quit 등)를 멀티 코어(또는 스레드)로 처리할지에 대한 여부입니다.
        /// true 로 하게 될경우 모든 장면속 이벤트 함수가 동시에 실행됩니다!
        /// 장면 갯수가 적은 경우 사용하지 않는걸 권장합니다.
        /// </summary>
        public static bool MultiCoreProcess = false;
        /// <summary>
        /// 현재 프레임워크의 버전을 알려줍니다.
        /// </summary>
        public static readonly System.Version Version = new(0, 6, 1,4);
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
        /// <param name="render_option">렌더러 옵션</param>
        /// <exception cref="JyunrcaeaFrameworkException">초기화 실패시</exception>
        public static void Init(string title, uint width, uint height, int? x = null, int? y = null, WindowOption option = default, RenderOption render_option = default, AudioOption audio_option = default)
        {
            #region 값 검사
            if (audio_option.ch > 8) throw new JyunrcaeaFrameworkException("지원하지 않는 스테레오 ( AudioOption.Channls > 8)");
            #endregion
            #region SDL 라이브러리 초기화
            if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) != 0)
            {
                throw new JyunrcaeaFrameworkException($"SDL2 라이브러리 초기화에 실패하였습니다. SDL Error: {SDL.SDL_GetError()}");
            }
            window = SDL.SDL_CreateWindow(title, x ?? SDL.SDL_WINDOWPOS_CENTERED, y ?? SDL.SDL_WINDOWPOS_CENTERED, (int)width, (int)height, option.option);
            if (window == IntPtr.Zero)
            {
                throw new JyunrcaeaFrameworkException($"창 초기화에 실패하였습니다. SDL Error: {SDL.SDL_GetError()}");
            }
            renderer = SDL.SDL_CreateRenderer(window, -1, render_option.option);
            if (renderer == IntPtr.Zero)
            {
                SDL.SDL_DestroyWindow(window);
                throw new JyunrcaeaFrameworkException($"렌더 초기화에 실패하였습니다. SDL Error: {SDL.SDL_GetError()}");
            }
            if (SDL.SDL_SetRenderDrawBlendMode(renderer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND) < 0) throw new JyunrcaeaFrameworkException($"렌더러의 블랜더 모드 설정에 실패하였습니다. SDL Error: {SDL.SDL_GetError()}");
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
#if WINDOWS
            SDL.SDL_SetEventFilter((_, eventPtr) =>
            {
                // ReSharper disable once PossibleNullReferenceException
                var e = (SDL.SDL_Event)System.Runtime.InteropServices.Marshal.PtrToStructure(eventPtr, typeof(SDL.SDL_Event))!;
                //if (e.type == SDL.SDL_EventType.SDL_KEYDOWN && Input.TextInput.Enable && e.key.keysym.sym == SDL.SDL_Keycode.SDLK_BACKSPACE)  
                if (e.key.repeat != 0) return 0;
                if (e.type != SDL.SDL_EventType.SDL_WINDOWEVENT) return 1;
                switch (e.window.windowEvent)
                {
                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                        Window.size.w = e.window.data1;
                        Window.size.h = e.window.data2;
                        Window.wh = Window.size.w * 0.5f;
                        Window.hh = Window.size.h * 0.5f;
                        Framework.Function.Resize();
                        break;
                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MOVED:
                        Window.position.x = e.window.data1;
                        Window.position.y = e.window.data2;
                        Framework.Function.WindowMove();
                        break;
                    default:
                        return 1;
                }
                Framework.Function.Draw();
                return 1;
            }, IntPtr.Zero);
#endif

            SDL_mixer.Mix_HookMusicFinished(Music.Finished);
            if ((option.option & SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP) == SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP) Window.fullscreenoption = true;
            if ((option.option & SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS) == SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS) Window.windowborderless = true;
            TextureSharing.resourcelist = new();
            Window.size = new() { w = Window.default_size.x = (int)width, h = Window.default_size.y = (int)height };
            Window.wh = width * 0.5f;
            Window.hh = height * 0.5f;
            if (SDL.SDL_GetDisplayMode(0, 0, out Display.dm) < 0)
            {
                throw new JyunrcaeaFrameworkException("디스플레이 정보를 갖고오는데 실패했습니다.\nDisplay 클래스 내에 있는 일부 함수와 전체화면 전환이 작동하지 못합니다.");
            }
            SDL.SDL_SetHint(SDL.SDL_HINT_WINDOWS_NO_CLOSE_ON_ALT_F4, "1");
            SDL.SDL_SetHint(SDL.SDL_HINT_VIDEO_MINIMIZE_ON_FOCUS_LOSS, "1");
            SDL.SDL_SetHint(SDL.SDL_HINT_IME_SHOW_UI, "0");
            SDL.SDL_EventState(SDL.SDL_EventType.SDL_DROPTEXT,SDL.SDL_ENABLE);
        }
        internal static bool running = false;
        internal static SDL.SDL_Event sdle;
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
        /// <exception cref="JyunrcaeaFrameworkException">실행중에 호출할경우</exception>
        public static void Run(bool ShowWindow = true)
        {
            if (running) throw new JyunrcaeaFrameworkException("이 함수는 이미 실행중인 함수입니다. (함수가 종료될때까지 호출할수 없습니다.)");
            SDL.SDL_StopTextInput();
            running = true;
            Framework.Function.Start();
            FrameworkFunction.updatetime = 0;
            FrameworkFunction.endtime = Display.framelatelimit;
            frametimer.Start();
            if (ShowWindow) SDL.SDL_ShowWindow(Framework.window);
            while (running)
            {
                Framework.Function.Draw();
                #region 이벤트
                if (EventMultiThreading)
                {
                    List<Thread> tl = new();
                    while (SDL.SDL_PollEvent(out var e) == 1)
                    {
                        Thread t = new((e) => EventProcess(e)); t.Start(e);
                        tl.Add(t);
                    }
                }
                else while (SDL.SDL_PollEvent(out sdle) == 1)
                {
                    switch (sdle.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            Framework.Function.WindowQuit();
                            break;
                        case SDL.SDL_EventType.SDL_WINDOWEVENT:
                            switch (sdle.window.windowEvent)
                            {
                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                                    Function.Resized();
                                    break;
                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                                    Framework.Function.WindowQuit();
                                    break;
                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SHOWN:
                                    //Console.WriteLine("shown");
                                    break;
                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_HIDDEN:
                                    //Console.WriteLine("hidden");
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
#if !WINDOWS
                                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                                                    //SDL.SDL_GetWindowSize(window, out var w, out var h);
                                                    //Window.w = (uint)w;
                                                    //Window.h = (uint)h;
                                                    //Window.wh = w * 0.5f;
                                                    //Window.hh = h * 0.5f;
                                                    //function.Resize();
                                                    Framework.function.Resize();
                                                    break;

                                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MOVED:
                                                    SDL.SDL_GetWindowPosition(window, out var x, out var y);
                                                    Window.x = x;
                                                    Window.y = y;
                                                    function.WindowMove();
                                                    break;
#endif
                            }
                            break;
                        case SDL.SDL_EventType.SDL_DROPFILE:
                            Framework.Function.DropFile(SDL.UTF8_ToManaged(sdle.drop.file, true));
                            break;
                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            //if (Input.TextInput.Enable)
                            //{
                            //    int l;
                            //    if (sdle.key.keysym.sym == SDL.SDL_Keycode.SDLK_BACKSPACE && (l =  Input.TextInput.InputedText.Length) > 0)
                            //        Input.TextInput.InputedText = Input.TextInput.InputedText.Substring(0,l-1);
                            //    Console.WriteLine(Input.TextInput.InputedText);
                            //};
                            Framework.Function.KeyDown((Input.Keycode)sdle.key.keysym.sym);
                            break;  
                        case SDL.SDL_EventType.SDL_MOUSEMOTION:
                            Framework.Function.MouseMove();
                            break;
                        case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                            Framework.Function.MouseKeyDown((Input.Mouse.Key)sdle.button.button);
                            break;
                        case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                            Framework.Function.MouseKeyUp((Input.Mouse.Key)sdle.button.button);
                            break;
                        case SDL.SDL_EventType.SDL_KEYUP:
                            Framework.Function.KeyUp((Input.Keycode)sdle.key.keysym.sym);
                            break;
                        //case SDL.SDL_EventType.SDL_TEXTINPUT:
                        //    unsafe
                        //    {
                        //        fixed (byte * b = sdle.text.text)
                        //        {
                        //            Input.TextInput.InputedText += new string((sbyte*)b);
                        //        }
                        //    }
                        //    Console.WriteLine(Input.TextInput.InputedText);
                        //    break;
                        //case SDL.SDL_EventType.SDL_TEXTEDITING:
                        //    unsafe
                        //    {
                        //        fixed (byte* b = sdle.edit.text) Console.WriteLine("Edit Text: {0}\nCursor Pos: {1}\nSelected Line {2}",new string((sbyte*)b),sdle.edit.start,sdle.edit.length);
                        //    }
                        //    break;
                    }
                }
                #endregion
            }
            Framework.Function.Stop();
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL_mixer.Mix_Quit();
            SDL_image.IMG_Quit();
            SDL_ttf.TTF_Quit();
            SDL.SDL_Quit();
        }

        public static bool EventMultiThreading = false;

        public static void EventProcess(object? e)
        {
            switch (((SDL.SDL_Event)e!).type)
            {
                case SDL.SDL_EventType.SDL_QUIT:
                    Framework.Function.WindowQuit();
                    break;
                case SDL.SDL_EventType.SDL_WINDOWEVENT:
                    switch (sdle.window.windowEvent)
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
#if !WINDOWS
                                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                                                    //SDL.SDL_GetWindowSize(window, out var w, out var h);
                                                    //Window.w = (uint)w;
                                                    //Window.h = (uint)h;
                                                    //Window.wh = w * 0.5f;
                                                    //Window.hh = h * 0.5f;
                                                    //function.Resize();
                                                    Framework.function.Resize();
                                                    break;

                                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MOVED:
                                                    SDL.SDL_GetWindowPosition(window, out var x, out var y);
                                                    Window.x = x;
                                                    Window.y = y;
                                                    function.WindowMove();
                                                    break;
#endif
                    }
                    break;
                case SDL.SDL_EventType.SDL_DROPFILE:
                    Framework.Function.DropFile(SDL.UTF8_ToManaged(sdle.drop.file, true));
                    break;
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    Framework.Function.KeyDown((Input.Keycode)sdle.key.keysym.sym);
                    break;
                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    Framework.Function.MouseMove();
                    break;
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    Framework.Function.MouseKeyDown((Input.Mouse.Key)sdle.button.button);
                    break;
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    Framework.Function.MouseKeyUp((Input.Mouse.Key)sdle.button.button);
                    break;
                case SDL.SDL_EventType.SDL_KEYUP:
                    Framework.Function.KeyUp((Input.Keycode)sdle.key.keysym.sym);
                    break;
            }
        }
        /// <summary>
        /// 프레임워크를 중지합니다.
        /// 즉, 창을 종료합니다.
        /// </summary>
        public static void Stop()
        {
            running = false;
        }

        /// <summary>
        /// 새로운 렌더링 방식(Zenerety)을 사용할지에 대한 여부입니다.
        /// </summary>
        public static bool NewRenderingSolution = false;

        static Stack<SDL.SDL_Point> DrawPosStack = new();

        static SDL.SDL_Rect DrawPos = new() { x = 0, y = 0 };

        internal static void Rendering(Group group)
        {
            DrawPosStack.Push(new() { x = DrawPos.x, y = DrawPos.y });
            int wx = DrawPos.x + group.Rx;
            int hy = DrawPos.y + group.Ry;
            for (int i = 0; i < group.ObjectList.Count; i++)
            {
                DrawPos.x = wx;
                DrawPos.y = hy;

                if (group.ObjectList[i] is Group)
                {
                    Rendering((Group)group.ObjectList[i]);
                    continue;
                }

                if (group.ObjectList[i] is ZeneretyDrawableObject)
                {
                    ZeneretyDrawableObject zdo = (ZeneretyDrawableObject)group.ObjectList[i];
                    DrawPos.w = zdo.RealWidth;
                    DrawPos.h = zdo.RealHeight;
                    DrawPos.x += zdo.Rx;
                    DrawPos.y += zdo.Ry;
                    if (zdo is Box)
                    {
                        SDL.SDL_SetRenderDrawColor(Framework.renderer,((Box)zdo).Color.colorbase.r, ((Box)zdo).Color.colorbase.g, ((Box)zdo).Color.colorbase.b, ((Box)zdo).Color.colorbase.a);
                        SDL.SDL_RenderFillRect(Framework.renderer, ref DrawPos);
                        continue;
                    }

                    if (zdo is Image)
                    {
                        SDL.SDL_RenderCopy(Framework.renderer, ((Image)zdo).Texture.texture, ref ((Image)zdo).Texture.src, ref DrawPos);
                        continue;
                    }
                }

            }
            var ret = DrawPosStack.Pop();
            DrawPos.x = ret.x;
            DrawPos.y = ret.y;
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
        public int X { get; set; }
        public int Y { get; set; }
        internal int Rx => X;
        internal int Ry => Y;

        internal bool MoveAnimation = false;
    }

    /// <summary>
    /// Zenerety 렌더링 용 그릴수 있는 객체
    /// </summary>
    public class ZeneretyDrawableObject : ZeneretyObject
    {
        internal int rw = 0, rh = 0;
        internal int ww = 0, hh = 0;

        internal virtual int RealWidth { get; }
        internal virtual int RealHeight { get; }
    }

    /// <summary>
    /// Zenerety 렌더링 방식에 사용되는 그룹 객체입니다.
    /// </summary>
    public class Group : ZeneretyObject
    {
        /// <summary>
        /// 묶을 객체들
        /// </summary>
        public ZeneretyList ObjectList = new();

    }

    /// <summary>
    /// Zenerety 렌더링 방식에 사용되는 직사각형 객체입니다.
    /// </summary>
    public class Box : ZeneretyDrawableObject
    {
        /// <summary>
        /// 직사각형의 너비와 높이
        /// </summary>
        public ZeneretySize Size = new();
        /// <summary>
        /// 너비 및 높이의 배율
        /// </summary>
        public ZeneretyScale Scale = new();

        /// <summary>
        /// 출력할 색상
        /// </summary>
        public Color Color = new();

        internal override int RealWidth => (int)(Size.Width * Scale.X);
        internal override int RealHeight => (int)(Size.Height * Scale.Y);
    }

    public class Image : ZeneretyDrawableObject
    {
        public DrawableTexture Texture = null!;

        /// <summary>
        /// 렌더링 될 이미지의 절대적 크기
        /// (null 로 설정시 원본 이미지의 크기를 따릅니다.)
        /// </summary>
        public ZeneretySize? AbsoluteSize = null;

        /// <summary>
        /// 이미지의 너비 및 높이의 배율
        /// </summary>
        public ZeneretyScale Scale = new();

        internal override int RealWidth => (int)((AbsoluteSize is null ? Texture.Width : AbsoluteSize.Width) * Scale.X);
        internal override int RealHeight => (int)((AbsoluteSize is null ? Texture.Height : AbsoluteSize.Height) * Scale.Y);
    }

    /// <summary>
    /// Zenerety 객체를 위한 리스트입니다.
    /// 추가 및 제거시 자동으로 리소스를 할당/해제 해줍니다.
    /// </summary>
    public class ZeneretyList : List<ZeneretyObject>, IDisposable
    {
        /// <summary>
        /// 객체를 추가합니다.
        /// </summary>
        /// <param name="obj">객체</param>
        public new void Add(ZeneretyObject obj)
        {
            base.Add(obj);
            if (Framework.running)
            {
                if (obj is Group)
                {
                    FrameworkFunction.Prepare((Group)obj);
                    return;
                }
                if (obj is Image)
                {
                    ((Image)obj).Texture.Ready();
                    return;
                }
            }
        }

        /// <summary>
        /// 객체를 제거합니다.
        /// </summary>
        /// <param name="obj">객체</param>
        public new void Remove(ZeneretyObject obj)
        {
            base.Remove(obj);
            if (obj is Group)
            {
                FrameworkFunction.Release((Group)obj);
                return;
            }
            if (obj is Image)
            {
                ((Image)obj).Texture.Free();
                return;
            }
        }

        public void Dispose()
        {
            if (Framework.running)
            foreach(var obj in this)
            {
                if (obj is Group)
                {
                    FrameworkFunction.Release((Group)obj);
                    return;
                }
                if (obj is Image)
                {
                    ((Image)obj).Texture.Free();
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
        public static Group Target = new();

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
            if (Framework.running) scene.Start();
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

        internal static bool windowborderless = false;
        /// <summary>
        /// 창의 테두리 제거여부
        /// </summary>
        public static bool Borderless
        {
            get => windowborderless;
            set => SDL.SDL_SetWindowBordered(Framework.window, (windowborderless = value) ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE);
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
        Events.WindowRestore
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
            for (int i=0;i<group.ObjectList.Count;i++)
            {
                if (group.ObjectList[i] is Group)
                {
                    Prepare((Group)group.ObjectList[i]);
                    continue;
                }
                 
                if (group.ObjectList[i] is Image)
                {
                    ((Image)group.ObjectList[i]).Texture.Ready();
                }


            }
        }

        //Stop, Free 와 비슷
        internal static void Release(Group group)
        {
            for (int i = 0; i < group.ObjectList.Count; i++)
            {
                if (group.ObjectList[i] is Group)
                {
                    Release((Group)group.ObjectList[i]);
                    continue;
                }

                if (group.ObjectList[i] is Image)
                {
                    ((Image)group.ObjectList[i]).Texture.Free();
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
                Prepare(Display.Target);
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
                Release(Display.Target);
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
                if (Framework.SavingPerformance && endtime > Framework.frametimer.ElapsedTicks + Framework.savelevel) Thread.Sleep(1);
                return;
            }


            
            Update(((updatems = Framework.frametimer.ElapsedTicks) - updatetime) * 0.0001f);

            if (Framework.NewRenderingSolution)
            {
                Framework.Rendering(Display.Target);
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
                Animation.AnimationQueue.Update();
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
            Resize();
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
            for (imm = 0; imm < Display.scenes.Count; imm++)
            {
                if (!Display.scenes[imm].EventRejection) Display.scenes[imm].MouseMove();
            }
        }

        static int imd, imu, iku;

        public virtual void MouseKeyDown(Input.Mouse.Key key)
        {
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
        internal SDL.SDL_WindowFlags option = SDL.SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI;

        public WindowOption() { option = SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN; }

        public WindowOption(bool resize = true, bool borderless = false, bool fullscreen = false, bool hide = true)
        {
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
        public byte anti_level = 0;

        public RenderOption(bool sccelerated = true, bool software = false, bool vsync = false, bool anti_aliasing = true)
        {
            if (sccelerated) option |= SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED;
            if (software) option |= SDL.SDL_RendererFlags.SDL_RENDERER_SOFTWARE;
            if (vsync) option |= SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC;
            //option |= SDL.SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE;
            if (anti_aliasing)
            {
                anti_level = 2;
                if (SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "2") == 0)
                {
                    anti_level = 1;
                    if (SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "1") == 0)
                    {
                        anti_level = 0;
                    }
                }
            }
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
        public interface DropFile {
            public void DropFile(string filename);
        }

        public interface Resized
        {
            public void Resized();
        }

        public interface Update
        {
            public void Update(float millisecond);
        }

        public interface WindowMove
        {
            public void WindowMove();
        }

        public interface KeyDown
        {
            public void KeyDown(Input.Keycode key);
        }

        public interface MouseMove
        {
            public void MouseMove();
        }

        public interface WindowQuit
        {
            public void WindowQuit();
        }

        public interface MouseKeyDown
        {
            public void MouseKeyDown(Input.Mouse.Key key);
        }

        public interface MouseKeyUp
        {
            public void MouseKeyUp(Input.Mouse.Key key);
        }

        public interface KeyUp
        {
            public void KeyUp(Input.Keycode key);
        }

        public interface WindowMaximized
        {
            public void WindowMaximized();
        }

        public interface WindowMinimized
        {
            public void WindowMinimized();
        }

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

        public int X
        {
            get => mx;
            set
            {
                mx = value;
                needupdatepos = true;
            }
        }

        public int Y
        {
            get => my;
            set
            {
                my = value;
                needupdatepos = true;
            }
        }

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
            if (Framework.running)
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
        public FunctionForAnimation CalculationFunction = Animation.Nothing;
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
        public FunctionForAnimation CalculationFunction = Animation.Nothing;

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

            internal static SDL.SDL_Point position = new();

            public static int X => position.x;

            public static int Y => position.y;

            static bool cursorhide = false;

            public static bool HideCursor
            {
                get => false;
                set
                {
                    SDL.SDL_ShowCursor((cursorhide = value) ? 0 : 1);
                }
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

    

    public static class Animation
    {
        internal class LinkedListForAnimation : LinkedList<InformationClass.AnimationInfomation>
        {
            public void Add(InformationClass.AnimationInfomation info)
            {
                if (info.EndTime <= Framework.RunningTime)
                {
                    info.Done();
                    return;
                }
                this.AddLast(info);
            }

            Queue<InformationClass.AnimationInfomation> RemoveTarget = new();

            public new void Remove(InformationClass.AnimationInfomation info)
            {
                RemoveTarget.Enqueue(info);
            }

            public void Update()
            {
                if (Framework.MultiCoreProcess)
                {
                    Parallel.ForEach(AnimationQueue, (a) => { a.Update(); });
                    return;
                }
                foreach (var a in AnimationQueue) a.Update();

                while(RemoveTarget.Count!=0) base.Remove(RemoveTarget.Dequeue());
            }
        }

        internal static LinkedListForAnimation AnimationQueue = new();


        public static InformationClass.Movement Move(ZeneretyObject Target,int X,int Y,double StartTime,double EndTime,Action? WhenFinish,FunctionForAnimation? Motion = null)
        {
            InformationClass.Movement result = new(X, Y, Target, StartTime, EndTime, WhenFinish, Motion);
            AnimationQueue.Add(result);
            return result;
        }

        public static class InformationClass
        {
            public abstract class AnimationInfomation
            {
                public AnimationInfomation(ZeneretyObject zo,double st,double et,Action? fff = null,FunctionForAnimation? ffa = null)
                {
                    Target = zo;
                    StartTime = st;
                    EndTime = et;
                    AnimationTime = EndTime - StartTime;
                    FunctionForFinish = fff;
                    if (ffa is not null) AnimationCalculator = ffa;
                }

                public ZeneretyObject Target { get; internal set; } = null!;
                public double StartTime { get; internal set; }
                public double EndTime { get; internal set; }
                public double AnimationTime { get; internal set; }
                public bool Finished { get; internal set; } = false;
                public Action? FunctionForFinish { get; internal set; } = null;
                public FunctionForAnimation AnimationCalculator { get; internal set; } = Animation.Nothing;

                internal double Progress = 0d;

                internal bool CheckTime()
                {
                    Progress = Framework.RunningTime;
                    if (Progress <= StartTime) return true;
                    if (Progress >= EndTime)
                    {
                        Done();
                        return true;
                    }
                    Progress = AnimationCalculator((Progress - StartTime) / AnimationTime);
                    return false;
                }

                internal abstract void Update();

                public virtual void Done()
                {
                    this.Finished = true;
                    if (FunctionForFinish is not null) FunctionForFinish();
                    AnimationQueue.Remove(this);
                }

                public virtual void Undo()
                {
                    this.Finished = false;
                }
            }

            public class Movement : AnimationInfomation
            {
                public Movement(int x,int y,ZeneretyObject zo, double st, double et, Action? fff = null, FunctionForAnimation? ffa = null) : base(zo,st,et,fff,ffa) {
                    BX = zo.X;
                    BY = zo.Y;
                    AX = x;
                    AY = y;
                    LX = AX - BX;
                    LY = AY - BY;
                }

                int BX, BY, AX, AY, LX, LY;
                internal override void Update()
                {
                    if (CheckTime()) return;
                    Target.X = BX + (int)(LX * Progress);
                    Target.Y = BY + (int)(LY * Progress);
                }

                public override void Done()
                {
                    Target.X = AX;
                    Target.Y = AY;
                    base.Done();
                }

                public override void Undo()
                {
                    Target.X = BX;
                    Target.Y = BY;
                    base.Done();
                }


            }



        }

        internal static double Nothing(double x) => x;

        internal static double EaseInSine(double x)
        {
            return 1d - Math.Cos((x * Math.PI) * 0.5d);
        }

        internal static double EaseOutSine(double x)
        {
            return Math.Sin((x * Math.PI) * 0.5d);
        }

        internal static double EaseInOutSine(double x)
        {
            return -(Math.Cos(Math.PI * x) - 1d) * 0.5d;
        }

        internal static double EaseInQuad(double x)
        {
            return x * x;
        }

        internal static double EaseOutQuad(double x)
        {
            x = 1d - x;
            return 1 - x * x;
        }

        internal static double EaseInOutQuad(double x)
        {
            return x < 0.5d ? 2d * x * x : 1d - Math.Pow(-2d * x + 2d, 2d) * 0.5d;
        }

        public static FunctionForAnimation GetAnimation(AnimationType type)
        {
            switch (type)
            {
                case AnimationType.Normal:
                    return Nothing;
                case AnimationType.Easing:
                    return EaseInOutSine;
                case AnimationType.Ease_In:
                    return EaseInSine;
                case AnimationType.Ease_Out:
                    return EaseOutSine;
                case AnimationType.EaseInQuad:
                    return EaseInQuad;
                case AnimationType.EaseOutQuad:
                    return EaseOutQuad;
                case AnimationType.EaseInOutQuad:
                    return EaseInOutQuad;

            }
            throw new JyunrcaeaFrameworkException("존재하지 않는 애니메이션 타입");
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
            base.Ready();
        }

        internal override void Free()
        {
            key.Free();
            this.texture = IntPtr.Zero;
            this.src.w = this.src.h = 0;
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

    public class TextureFromText : DrawableTexture
    {
        public string Fontfile,Text;
        public int Size;
        public Color Color;
        public Color? BackgroundColor;
        public bool Blended;

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
            Ready();
        }

        internal override void Ready()
        {
            IntPtr fontsource = SDL_ttf.TTF_OpenFont(Fontfile, Size);
            if (fontsource == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"불러올수 없는 글꼴 파일 SDL Error: {SDL.SDL_GetError()}"); 
            IntPtr surface = (BackgroundColor == null) ?
                (Blended ? SDL_ttf.TTF_RenderUNICODE_Blended(fontsource, Text, Color.colorbase) : SDL_ttf.TTF_RenderUNICODE_Solid(fontsource, Text, Color.colorbase)) : SDL_ttf.TTF_RenderText_Shaded(fontsource,Text, Color.colorbase, BackgroundColor.colorbase);
            if (surface == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"텍스트를 렌더링 하는데 실패하였습니다. SDL Error: {SDL.SDL_GetError()}");
            this.texture = SDL.SDL_CreateTextureFromSurface(Framework.renderer,surface);
            SDL.SDL_FreeSurface(surface);
            if (this.texture == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"렌더링 된 텍스트를 텍스쳐로 변환하는데 실패하였습니다. {SDL.SDL_GetError()}");
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
            this.texture = IntPtr.Zero;
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
        internal IntPtr fontsource = IntPtr.Zero;

        internal int sz = 1;

        public int Size { get => sz; set {
                if (SDL_ttf.TTF_SetFontSize(this.fontsource, this.sz = value) == -1) throw new JyunrcaeaFrameworkException($"폰트 로드에 실패했습니다. SDL_TTF Error: {SDL_ttf.TTF_GetError()}");
            }
        }

        public Font(string filename,int size)
        {
            this.fontsource = SDL_ttf.TTF_OpenFont(filename, this.sz = size);
        }

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