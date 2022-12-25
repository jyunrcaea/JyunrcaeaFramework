#define WINDOWS
using SDL2;
using System.Diagnostics.SymbolStore;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Transactions;

namespace JyunrcaeaFramework
{
    
    /// <summary>
    /// 프레임워크에 대한 명령어가 모여있습니다.
    /// 초기화, 시작, 종료 등이 있습니다.
    /// </summary>
    public static class Framework
    {
#if DEBUG
        public static bool ObjectDrawDebuging = false;

        public static Color ObjectDrawDebugingLineColor = new(255, 50, 50);

        public static Color SceneDrawDebugingLineColor = new(50, 255, 50);
#endif
        /// <summary>
        /// 이벤트(Update, Quit 등)를 멀티 코어(또는 스레드)로 처리할지에 대한 여부입니다.
        /// true 로 하게 될경우 모든 장면속 이벤트 함수가 동시에 실행됩니다!
        /// 장면 갯수가 적은 경우 사용하지 않는걸 권장합니다.
        /// </summary>
        public static bool MultiCoreProcess = false;

        public static Color BackgroundColor = new(31, 30, 51);

        public static readonly System.Version Version = new(0, 2, 1);

        public static FrameworkFunction function = new();
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
        public static void Init(string title, uint width, uint height, int? x, int? y, WindowOption option, RenderOption render_option = new())
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) != 0)
            {
                throw new JyunrcaeaFrameworkException($"SDL2 라이브러리 초기화에 실패하였습니다. SDL Error: {SDL.SDL_GetError()}");
            }
            window = SDL.SDL_CreateWindow(title, x == null ? SDL.SDL_WINDOWPOS_CENTERED : (int)x, y == null ? SDL.SDL_WINDOWPOS_CENTERED : (int)y, (int)width, (int)height, option.option);
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
#if WINDOWS
            SDL.SDL_SetEventFilter((_, eventPtr) =>
            {
                // ReSharper disable once PossibleNullReferenceException
                var e = (SDL.SDL_Event)System.Runtime.InteropServices.Marshal.PtrToStructure(eventPtr, typeof(SDL.SDL_Event))!;
                if (e.key.repeat != 0) return 0;
                if (e.type != SDL.SDL_EventType.SDL_WINDOWEVENT) return 1;
                switch (e.window.windowEvent)
                {
                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                        Framework.function.Resize();
                        break;
                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MOVED:
                        Framework.function.WindowMove();
                        break;
                    default:
                        return 1;
                }
                Framework.function.Draw();
                return 1;
            }, IntPtr.Zero);
#endif
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
            SDL.SDL_SetHint(SDL.SDL_HINT_IME_SHOW_UI, "1");
        }
        /// <summary>
        /// 실행
        /// </summary>
        internal static bool running = false;
        /// <summary>
        /// SDL Event
        /// </summary>
        internal static SDL.SDL_Event sdle;
        /// <summary>
        /// For limit Framelate
        /// </summary>
        internal static System.Diagnostics.Stopwatch frametimer = new();

        public static float RunningTime => frametimer.ElapsedTicks * 0.0001f;

        /// <summary>
        /// Framework.Stop(); 을 호출할때까지 창을 띄웁니다. (또는 오류가 날때까지...)
        /// </summary>
        /// <exception cref="JyunrcaeaFrameworkException">실행중에 호출할경우</exception>
        public static void Run()
        {
            if (running) throw new JyunrcaeaFrameworkException("이 함수는 이미 실행중인 함수입니다. (함수가 종료될때까지 호출할수 없습니다.)");
            running = true;
            Framework.function.Start();
            frametimer.Start();
            FrameworkFunction.endtime = (FrameworkFunction.updatetime = frametimer.ElapsedTicks) + Display.framelatelimit;
            while (running)
            {
                while (SDL.SDL_PollEvent(out sdle) != 0)
                {
                    switch (sdle.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            Framework.function.WindowQuit();
                            break;
                        case SDL.SDL_EventType.SDL_WINDOWEVENT:
                            switch (sdle.window.windowEvent)
                            {
                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                                    function.Resized();
                                    break;
                                
                                //case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_NONE:
                                //    Console.WriteLine("none");
                                //    break;
                                //case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                                //    Console.WriteLine("close");
                                //    break;
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
                            Framework.function.FileDropped(SDL.UTF8_ToManaged(sdle.drop.file, true));
                            break;
                        case SDL.SDL_EventType.SDL_DROPTEXT:
                            break;
                        case SDL.SDL_EventType.SDL_DROPCOMPLETE:
                            break;
                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            //Console.WriteLine(sdle.key.keysym.sym.ToString());
                            Framework.function.KeyDown((Keycode)sdle.key.keysym.sym);
                            break;
                        case SDL.SDL_EventType.SDL_MOUSEMOTION:
                            Framework.function.MouseMove();
                            break;
                        case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                            Framework.function.MouseButtonDown((Mousecode)sdle.button.button);
                            break;
                        case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                            Framework.function.MouseButtonUp((Mousecode)sdle.button.button);
                            break;
                        case SDL.SDL_EventType.SDL_KEYUP:
                            Framework.function.KeyUp((Keycode)sdle.key.keysym.sym);
                            break;
                    }
                    
                }
                Framework.function.Draw();
            }
            Framework.function.Stop();
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL_image.IMG_Quit();
            SDL_ttf.TTF_Quit();
            SDL.SDL_Quit();
        }

        public static void Stop()
        {
            running = false;
        }
    }

    public static class Display
    {
        internal static SDL.SDL_DisplayMode dm;

        internal static List<SceneInterface> scenes = new();

        internal static List<Thread> threads = new();

        public static int AddScene(SceneInterface scene)
        {
            scenes.Add(scene);
            threads.Add(null!);
            if (Framework.running) scene.Start();
            return scenes.Count - 1;
        }

        public static void RemoveScene(int index)
        {
            scenes[index].Stop();
            scenes.RemoveAt(index);
            threads.RemoveAt(index);
        }

        public static void RemoveScene(SceneInterface scene)
        {
            scene.Stop();
            scenes.Remove(scene);
            threads.RemoveAt(0);
        }

        static float fps = 60;

        public static int MonitorWidth => dm.w;

        public static int MonitorHeight => dm.h;

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
                framelatelimit = (long)(1f / fps * 10000000);
            }
        }
    }

    public static class Window {
        internal static SDL.SDL_Rect size = new();

        internal static SDL.SDL_Point position = new();

        internal static SDL.SDL_Point default_size = new();

        public static int DefaultWidth => default_size.x;

        public static int DefaultHeight => default_size.y;

        public static float AppropriateSize { get; internal set; } = 1;

        //internal static uint h = 0;
        internal static float wh = 0, hh = 0;
        //internal static int Y = 0;

        public static int X => position.x;
        public static int Y => position.y;

        public static uint UWidth => (uint)size.w;
        public static uint UHeight => (uint)size.h;

        public static int Width => size.w;
        public static int Height => size.h;

        public static bool Show { set { if (value) SDL.SDL_ShowWindow(Framework.window); else SDL.SDL_HideWindow(Framework.window); } }

        public static void Icon(string filename)
        {
            IntPtr surface = SDL_image.IMG_Load(filename);
            if (surface == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"파일을 불러올수 없습니다. (SDL image Error: {SDL_image.IMG_GetError()})");
            SDL.SDL_SetWindowIcon(Framework.window, surface);
            SDL.SDL_FreeSurface(surface);
        }

        //static bool fulls = false;

        //static uint beforew=0,beforeh=0;

        //public static bool FullScreen
        //{
        //    set
        //    {
        //        fulls = value;  
        //        if (value == true)
        //        {
        //            beforew = w; beforeh = h;
        //            SDL.SDL_SetWindowDisplayMode(Framework.window,ref Display.dm);
        //            SDL.SDL_SetWindowFullscreen(Framework.window, (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN);


        //        }else
        //        {
        //            //SDL.SDL_SetWindowSize(Framework.window, (int)beforew, (int)beforeh);
        //            SDL.SDL_SetWindowFullscreen(Framework.window, 0);
        //        }
        //    }
        //    get => fulls;
        //}
    }

    public interface AllEventInterface:
        ResizeEndEventInterface,
        WindowMoveEventInterface,
        DropFileEventInterface,
        UpdateEventInterface,
        KeyDownEventInterface,
        MouseMoveEventInterface,
        WindowQuitEventInterface,
        MouseButtonDownEventInterface,
        MouseButtonUpEventInterface,
        KeyUpEventInterface
    {

    }

    public class FrameworkFunction : ObjectInterface, AllEventInterface
    {
        public override void Start()
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

        public override void Stop()
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

        private int iu,id,ir,iwm,ird,ist,ifd;

        private float iratio;

        public override void Resize()
        {
            SDL.SDL_GetWindowSize(Framework.window, out Window.size.w, out Window.size.h);
            Window.wh = Window.size.w * 0.5f;
            Window.hh = Window.size.h * 0.5f;
            iratio = (float)Window.size.w / (float)Window.default_size.x;
            if ( iratio * Window.default_size.y > Window.size.h )
            {
                iratio = (float)Window.size.h / (float)Window.default_size.y;
            }
            Window.AppropriateSize = iratio;
            for (ir = 0; ir < Display.scenes.Count; ir++)
            {
                Display.scenes[ir].Resize();
            }
            //for (ir = 0; ir < Display.scenes.Count; ir++)
            //{
            //    (Display.threads[ir] = new(Display.scenes[ir].Resize)).Start();
            //}

        }

        internal static long endtime=0;

        internal override void Draw()
        {
            if (endtime > Framework.frametimer.ElapsedTicks) {
                if (endtime > Framework.frametimer.ElapsedTicks + 1100) Thread.Sleep(1);
                return;
            }
            Update(((updatems = Framework.frametimer.ElapsedTicks) - updatetime) * 0.0001f);
            //for (id = 0; id < Display.scenes.Count; id++)
            //{
            //    (Display.threads[id] = new(Display.scenes[id].Draw)).Start();
            //}
            //while(id-->0)
            //{
            //    Display.threads[id].Join();
            //}
            for (id = 0; id < Display.scenes.Count; id++)
            {
                Display.scenes[id].Draw();
            }
#if DEBUG
            if (Framework.ObjectDrawDebuging)
            {
                ODD();
            }
#endif
            SDL.SDL_RenderPresent(Framework.renderer);
            SDL.SDL_SetRenderDrawColor(Framework.renderer, Framework.BackgroundColor.Red, Framework.BackgroundColor.Green, Framework.BackgroundColor.Blue, Framework.BackgroundColor.Alpha);
            SDL.SDL_RenderClear(Framework.renderer);
            if (endtime <= Framework.frametimer.ElapsedTicks - Display.framelatelimit)
                endtime = Framework.frametimer.ElapsedTicks + Display.framelatelimit;
            else endtime += Display.framelatelimit;
        }

        internal static long updatetime = 0, updatems = 0;

        public virtual void Update(float ms)
        {
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => Display.scenes[i].Update(ms));
            }
            else {
                for (iu = 0; iu < Display.scenes.Count; iu++)
                {
                    Display.scenes[iu].Update(ms);
                }         
            }

            updatetime = updatems;
        }

        public virtual void Resized()
        {
            Resize();
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => Display.scenes[i].Resized());
            }
            else {
                 for (ird =0;ird<Display.scenes.Count;ird++)
                {
                    Display.scenes[ird].Resized();
                }           
            }

        }

        public virtual void WindowMove()
        {
            SDL.SDL_GetWindowPosition(Framework.window, out var x, out var y);
            Window.position.x = x;
            Window.position.y = y;
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => Display.scenes[i].WindowMove());
            }
            else
            {
                for (iwm = 0; iwm < Display.scenes.Count; iwm++)
                {
                    Display.scenes[iwm].WindowMove();
                }
            }

        }

        static int iwq;

        public virtual void WindowQuit()
        {
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => Display.scenes[i].WindowQuit());
            }
            else
            {
                for (iwq = 0; iwq < Display.scenes.Count; iwq++)
                    Display.scenes[iwq].WindowQuit();
            }

        }

        public virtual void FileDropped(string filename)
        {
            for (ifd = 0; ifd < Display.scenes.Count; ifd++)
                Display.scenes[ifd].FileDropped(filename);
        }

        static int ikd;

        public virtual void KeyDown(Keycode e)
        {
            for (ikd = 0; ikd < Display.scenes.Count; ikd++)
                Display.scenes[ikd].KeyDown(e);
        }

        static int imm;

        public virtual void MouseMove()
        {
            SDL.SDL_GetMouseState(out Mouse.position.x, out Mouse.position.y);
            for (imm = 0; imm < Display.scenes.Count; imm++)
            {
                Display.scenes[imm].MouseMove();
            }
        }

        static int imd, imu, iku;

        public virtual void MouseButtonDown(Mousecode key)
        {
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => Display.scenes[i].MouseButtonDown(key));
            }
            else
            {
                for(imd = 0;imd < Display.scenes.Count; imd++)
                {
                    Display.scenes[imd].MouseButtonDown(key);
                }
            }
        }

        public virtual void MouseButtonUp(Mousecode key)
        {
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => Display.scenes[i].MouseButtonUp(key));
            }
            else
            {
                for (imu = 0; imu < Display.scenes.Count; imu++)
                {
                    Display.scenes[imu].MouseButtonUp(key);
                }
            }
        }

        public virtual void KeyUp(Keycode key)
        {
            if (Framework.MultiCoreProcess)
            {
                Parallel.For(0, Display.scenes.Count, (i, _) => Display.scenes[i].KeyUp(key));
            }
            else
            {
                for (iku = 0; iku < Display.scenes.Count; iku++)
                {
                    Display.scenes[iku].KeyUp(key);
                }
            }
        }

#if DEBUG
        internal override void ODD()
        {
            SDL.SDL_SetRenderDrawColor(Framework.renderer, Framework.ObjectDrawDebugingLineColor.Red, Framework.ObjectDrawDebugingLineColor.Green, Framework.ObjectDrawDebugingLineColor.Blue, Framework.ObjectDrawDebugingLineColor.Alpha);
            for (int i = 0; i < Display.scenes.Count; i++)
            {
                if (!Display.scenes[i].Hide) Display.scenes[i].ODD();
            }
        }
#endif
    }

    public struct WindowOption
    {
        internal SDL.SDL_WindowFlags option = new();

        public WindowOption(bool resize,bool borderless,bool fullscreen, bool fullscreen_desktop,bool hide)
        {
            if (resize) option |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
            if (borderless) option |= SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
            if (fullscreen) option |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
            if (fullscreen_desktop) option |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
            if (hide) option |= SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN;
        }
    }

    public struct RenderOption
    {
        internal SDL.SDL_RendererFlags option = new();
        public byte anti_level = 0;

        public RenderOption(bool sccelerated = true,bool software = false,bool vsync = false,bool anti_aliasing = true)
        {
            if (sccelerated) option |= SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED;
            if (software) option |= SDL.SDL_RendererFlags.SDL_RENDERER_SOFTWARE;
            if (vsync) option |= SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC;
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

    public abstract class ObjectInterface
#if  DEBUG
        : ODDInterface
#endif
    {
        public abstract void Resize();
        public abstract void Stop();
        internal abstract void Draw();
        public abstract void Start();
        public bool Hide = false;
    }

    public abstract class SceneInterface : ObjectInterface , AllEventInterface
    {
        public RectSize? RenderRange = null;

        //internal RenderPositionForScene pos = new();

        //internal bool resetpos = false;

        //public int X
        //{
        //    get => pos.X; set
        //    {
        //        pos.X = value;
        //        resetpos = true;
        //    }
        //}

        //public int Y
        //{
        //    get => pos.Y; set
        //    {
        //        pos.Y = value;
        //        resetpos = true;
        //    }
        //}

        public abstract void FileDropped(string filename);

        public abstract void Resized();

        public abstract void Update(float millisecond);

        public abstract void WindowMove();

        public abstract void KeyDown(Keycode e);

        public abstract void MouseMove();

        public abstract void WindowQuit();

        public abstract void KeyUp(Keycode e);

        public abstract void MouseButtonDown(Mousecode e);

        public abstract void MouseButtonUp(Mousecode e);
    }

    public abstract class DrawableObject : ObjectInterface
    {
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

    public interface DropFileEventInterface {
        public void FileDropped(string filename);
    }

    public interface ResizeEndEventInterface
    {
        public void Resized();
    }

    public interface UpdateEventInterface
    {
        public void Update(float millisecond);
    }

    public interface WindowMoveEventInterface
    {
        public void WindowMove();
    }

    public interface KeyDownEventInterface
    {
        public void KeyDown(Keycode key);
    }

    public interface MouseMoveEventInterface
    {
        public void MouseMove();
    }

    public interface WindowQuitEventInterface
    {
        public void WindowQuit();
    }

    public interface MouseButtonDownEventInterface
    {
        public void MouseButtonDown(Mousecode key);
    }

    public interface MouseButtonUpEventInterface
    {
        public void MouseButtonUp(Mousecode key);
    }

    public interface KeyUpEventInterface
    {
        public void KeyUp(Keycode key);
    }

#if DEBUG
    public abstract class ODDInterface
    {
        internal abstract void ODD();
    }
#endif

    public class Scene : SceneInterface
    {
        //public int X = 0, Y = 0;

        //List<object> objectlist = new();
        List<DrawableObject> sprites = new();
        List<DropFileEventInterface> drops = new();
        List<ResizeEndEventInterface> resizes = new();
        List<UpdateEventInterface> updates = new();
        List<WindowMoveEventInterface> windowMovedInterfaces = new();
        List<KeyDownEventInterface> keyDownEvents = new();
        List<MouseMoveEventInterface> mouseMoves = new();
        List<WindowQuitEventInterface> windowQuits = new();
        List<KeyUpEventInterface> keyUpEvents = new();
        List<MouseButtonDownEventInterface> mouseButtonDownEvents = new();
        List<MouseButtonUpEventInterface> mouseButtonUpEvents = new();

        //public void AddSprite(DrawableObject sp)
        //{
        //    sprites.Add(sp);
        //    if (Framework.running)
        //        sp.Start();
        //}
        /// <summary>
        /// 장면 위에 그릴수 있는 객체를 원하는 범위에 추가합니다. 
        /// </summary>
        /// <param name="sp">그릴수 있는 객체(sprite, textbox, rectangle 등)</param>
        /// <param name="index">추가할 범위 (음수일경우 맨 마지막에서 횟수만큼 앞으로 가서 추가합니다, 즉 -2 일경우 마지막(-1)에서 앞으로 한칸입니다.)</param>
        /// <returns>성공시 true를 반환, 실패서 false를 반환</returns>
        public bool AddSprite(object sp,int index)
        {
            if (sp is DrawableObject)
            {
                //objectlist.Add(sp);
                //((DrawableObject)sp).scenepos = this.pos;
                if (sp is DropFileEventInterface) drops.Add((DropFileEventInterface)sp);
                if (sp is ResizeEndEventInterface) resizes.Add((ResizeEndEventInterface)sp);
                if (sp is UpdateEventInterface) updates.Add((UpdateEventInterface)sp);
                if (sp is WindowMoveEventInterface) windowMovedInterfaces.Add((WindowMoveEventInterface)sp); 
                if (sp is KeyDownEventInterface) keyDownEvents.Add((KeyDownEventInterface)sp);
                if (sp is MouseMoveEventInterface) mouseMoves.Add((MouseMoveEventInterface)sp);
                if (sp is WindowQuitEventInterface) windowQuits.Add((WindowQuitEventInterface)sp);
                if (sp is KeyUpEventInterface) keyUpEvents.Add((KeyUpEventInterface)sp);
                if (sp is MouseButtonDownEventInterface) mouseButtonDownEvents.Add((MouseButtonDownEventInterface)sp);
                if (sp is MouseButtonUpEventInterface) mouseButtonUpEvents.Add((MouseButtonUpEventInterface)sp);
                if (index < 0)
                {
                    if (index == -1) {
                        sprites.Add((DrawableObject)sp);
                        //objectlist.Add(sp);
                    }
                    else
                    {
                        int i = sprites.Count + index;
                        if (i < 0) throw new JyunrcaeaFrameworkException($"존재하지 않는 범위입니다. (입력한 범위: {index}, 선택된 범위: {i}, 리스트 갯수: {sprites.Count})");
                        sprites.Insert(i, (DrawableObject)sp);
                        //objectlist.Insert(i, sp);
                    }
                }
                else {
                    sprites.Insert(index, (DrawableObject)sp);
                    //objectlist.Insert(index, sp);
                }
                if (Framework.running)
                    sprites.Last().Start();
                return true;
            }
            return false;
        }

        public bool RemoveSprite(object sp)
        {
            if (sp is not DrawableObject) return false;
            
            //if (objectlist.IndexOf(sp) == -1) return false;
            //objectlist.Remove(sp);
            
            if (!sprites.Remove((DrawableObject)sp)) return false;
           // ((DrawableObject)sp).scenepos = null!;
            if (sp is DropFileEventInterface) drops.Remove((DropFileEventInterface)sp);
            if (sp is ResizeEndEventInterface) resizes.Remove((ResizeEndEventInterface)sp);
            if (sp is UpdateEventInterface) updates.Remove((UpdateEventInterface)sp);
            if (sp is WindowMoveEventInterface) windowMovedInterfaces.Remove((WindowMoveEventInterface)sp);
            if (sp is KeyDownEventInterface) keyDownEvents.Remove((KeyDownEventInterface)sp);
            if (sp is MouseMoveEventInterface) mouseMoves.Remove((MouseMoveEventInterface)sp);
            if (sp is WindowQuitEventInterface) windowQuits.Remove((WindowQuitEventInterface)sp);
            if (sp is KeyUpEventInterface) keyUpEvents.Remove((KeyUpEventInterface)sp);
            if (sp is MouseButtonDownEventInterface) mouseButtonDownEvents.Remove((MouseButtonDownEventInterface)sp);
            if (sp is MouseButtonUpEventInterface) mouseButtonUpEvents.Remove((MouseButtonUpEventInterface)sp);
            ((DrawableObject)sp).Stop();
            return true;
        }

        /// <summary>
        /// 장면 위에 그릴수 있는 객체를 (추가된 객체들 뒤에) 추가합니다.
        /// </summary>
        /// <param name="sp">그릴수 있는 객체(sprite, textbox, rectangle 등)</param>
        /// <returns>성공시 객체가 리스트에 저장된 위치를 반환하며, 실패시 '-1' 를 반환합니다.</returns>
        public int AddSprite(object sp)
        {
            if (AddSprite(sp, -1)) return sprites.Count - 1;
                return -1;
        }

        /// <summary>
        /// 장면 위에 그릴수 있는 객체 여러개를 추가합니다.
        /// </summary>
        /// <param name="sp">그릴수 있는 객체들(sprite, textbox, rectangle 등)</param>
        /// <returns>실패한 객체들의 위치들이 반환됩니다. 만약 반환된게 하나도 없다면 전부다 성공한겁니다.</returns>
        public int[] AddSprites(params object[] sp)
        {
            if (sp.Length == 0) return null!;
            List<int> not_success_list = new();
            for (int i = 0; i < sp.Length; i++)
            {
                if (AddSprite(sp[i]) == -1) not_success_list.Add(i);
            }
            return not_success_list.ToArray();
        }

#if DEBUG
        internal override void ODD()
        {
            SDL.SDL_SetRenderDrawColor(Framework.renderer, Framework.ObjectDrawDebugingLineColor.Red, Framework.ObjectDrawDebugingLineColor.Green, Framework.ObjectDrawDebugingLineColor.Blue, Framework.ObjectDrawDebugingLineColor.Alpha);
            for(int i =0; i < sprites.Count; i++)
            {
                if (sprites[i].Hide) continue;
                sprites[i].ODD();
            }
            SDL.SDL_SetRenderDrawColor(Framework.renderer, Framework.SceneDrawDebugingLineColor.Red, Framework.SceneDrawDebugingLineColor.Green, Framework.SceneDrawDebugingLineColor.Blue, Framework.SceneDrawDebugingLineColor.Alpha);
            if (this.RenderRange == null) SDL.SDL_RenderDrawRect(Framework.renderer, ref Window.size);
            else SDL.SDL_RenderDrawRect(Framework.renderer, ref this.RenderRange.size);
        }
#endif

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
            
        }

        public override void Update(float ms)
        {
            for(int i = 0; i < updates.Count; i++)
                updates[i].Update(ms);
        }

        internal override void Draw()
        {
            if (this.Hide) return;
            if (this.RenderRange == null) SDL.SDL_RenderSetViewport(Framework.renderer, ref Window.size);
            else SDL.SDL_RenderSetViewport(Framework.renderer, ref this.RenderRange.size);
            for (int i = 0; i < this.sprites.Count ; i++)
            {
                if (sprites[i].Hide) continue;
                //if (this.resetpos) sprites[i].needresetposition = true;
                sprites[i].Draw();
            }
            //if (this.resetpos) resetpos = false;
        }

        public override void FileDropped(string filename)
        {
            for (int i = 0; i < drops.Count; i++)
                drops[i].FileDropped(filename);
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

        public override void KeyDown(Keycode e)
        {
            for (int i=0; i < keyDownEvents.Count; i++)
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

        public override void KeyUp(Keycode e)
        {
            for (int i = 0; i < keyUpEvents.Count; i++)
                keyUpEvents[i].KeyUp(e);
        }

        public override void MouseButtonDown(Mousecode e)
        {
            for (int i = 0; i < mouseButtonDownEvents.Count; i++)
                mouseButtonDownEvents[i].MouseButtonDown(e);
        }

        public override void MouseButtonUp(Mousecode e)
        {
            for (int i = 0; i < mouseButtonUpEvents.Count; i++)
                mouseButtonUpEvents[i].MouseButtonUp(e);
        }
    }

    public class Rectangle : DrawableObject
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

        internal override void Draw()
        {
            if (needresetposition) this.ResetPosition();
            if (needresetsize) this.ResetSize();
            if (needresetdrawposition)
            {
                this.dst.x = this.px + this.originpos.x + this.mx;
                this.dst.y = this.py + this.originpos.y + this.my;
            }
            //Console.WriteLine("r: {0}, w: {1}, h: {2}, x: {3}, y: {4}",this.Color.Red,this.size.w,this.size.h,this.size.x,this.size.y);
            if (SDL.SDL_SetRenderDrawColor(Framework.renderer,this.Color.Red,this.Color.Green,this.Color.Blue,this.Color.Alpha) < 0) throw new JyunrcaeaFrameworkException($"색 변경에 실패하였습니다. (SDL Error: {SDL.SDL_GetError()})");

            if (SDL.SDL_RenderFillRect(Framework.renderer,ref dst) == -1) throw new JyunrcaeaFrameworkException($"직사각형 렌더링에 실패하였습니다. (SDL Error: {SDL.SDL_GetError()})");
        }

#if DEBUG
        internal override void ODD()
        {
            SDL.SDL_RenderDrawRect(Framework.renderer,ref dst);
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

    public class Sprite : DrawableObject , IDisposable
    {
        //string filename = string.Empty;

        //string[]? xpm = null;

        //IntPtr source = IntPtr.Zero;

        //SDL.SDL_Rect src = new();

        DrawableTexture Texture = null!;

        //SDL.SDL_Rect dst = new();

        int px = 0, py = 0;
        /// <summary>
        /// 수평 원점을 설정합니다.
        /// </summary>
        /// 
        public double Rotation = 0;

        //public int TextureWidth => src.w;

        //public int TextureHeight => src.h;

        public int Width => dst.w;

        public int Height => dst.h; 

        float sz = 1;
        /// <summary>
        /// 크기를 설정합니다.
        /// </summary>
        public float Size { get { return sz; } set {
                sz = value;
                //dst.w = (int)(src.w * value);
                //dst.h = (int)(src.h * value);
                dst.w = (int)(Texture.src.w * value);
                dst.h = (int)(Texture.src.h * value);
                needresetsize = true;
         }}

        bool fh = false, fv = false;

        public bool FlipHorizontal { get => fh; set => flip = ((fh = value) ? SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE) | (fv ? SDL.SDL_RendererFlip.SDL_FLIP_VERTICAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE); }

        public bool FlipVertical { get => fv; set => flip = (fh ? SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE) | ((fv = value) ? SDL.SDL_RendererFlip.SDL_FLIP_VERTICAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE); }

        SDL.SDL_RendererFlip flip = SDL.SDL_RendererFlip.SDL_FLIP_NONE;

        /// <summary>
        /// 이미지를 불러옵니다.
        /// 실패시 오류가 납니다.
        /// </summary>
        /// <exception cref="JyunrcaeaFrameworkException">이미지 불러오기 실패시</exception>
        public override void Start()
        {
            //if(xpm == null) this.source = SDL_image.IMG_LoadTexture(Framework.renderer,filename);
            //else
            //{
            //    IntPtr surface = SDL_image.IMG_ReadXPMFromArray(this.xpm);
            //    if (surface == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"XPM 형식 이미지 로드에 실패하였습니다. (SDL_image Error: {SDL_image.IMG_GetError()})");
            //    this.source = SDL.SDL_CreateTextureFromSurface(Framework.renderer,surface);
            //    SDL.SDL_FreeSurface(surface);
            //}
            //if (this.source == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"이미지 로드에 실패하였습니다. (SDL_image Error: {SDL_image.IMG_GetError()})");
            //SDL.SDL_QueryTexture(source, out _, out _, out src.w, out src.h);
            this.Texture.Ready();
            dst.w = (int)( Texture.src.w * this.sz);
            dst.h = (int)(Texture.src.h * this.sz);
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
            this.Texture.Free();
        }

        internal override void Draw()
        {
            if (this.Texture.needresettexture)
            {
                dst.w = (int)(Texture.src.w * this.sz);
                dst.h = (int)(Texture.src.h * this.sz);
                needresetposition=needresetsize=true;
                this.Texture.needresettexture = false;
            }
            if (needresetposition) ResetPosition();
            if (needresetsize) ResetSize();
            if (needresetdrawposition)
            {
                this.dst.x = this.px + this.originpos.x + this.mx;
                this.dst.y = this.py + this.originpos.y + this.my;
                this.needresetdrawposition = false;
            }
            SDL.SDL_RenderCopyEx(Framework.renderer, this.Texture.texture,ref this.Texture.src,ref this.dst, Rotation, IntPtr.Zero, flip);
        }

        //public void ImageLoad(string filename)
        //{
        //    this.filename = filename;
        //    this.xpm = null;
        //    if (this.source != IntPtr.Zero)
        //    {
        //        Stop();
        //        Start();
        //    }
        //}

        //public void ImageLoad(string[] xpm)
        //{
        //    this.xpm = xpm;
        //    this.filename = string.Empty;
        //    if (this.source != IntPtr.Zero)
        //    {
        //        Stop();
        //        Start();
        //    }
        //}

        public Sprite() { }

        //public Sprite(string filename)
        //{
        //    this.filename = filename;
        //}

        public Sprite(DrawableTexture texture)
        {
            this.Texture = texture;
        }

#if DEBUG
        internal override void ODD()
        {
            SDL.SDL_RenderDrawRect(Framework.renderer,ref dst);
        }
#endif
    }

    public class SpriteForAnimation : Sprite, UpdateEventInterface
    {
        public SpriteForAnimation()
        {

        }

        //public SpriteForAnimation(string filename) : base(filename) { }

        public SpriteForAnimation(DrawableTexture texture) : base(texture) { }

        long starttime = 0;
        float nowtime = 0;

        float xe = 0, ye = 0;

        public new int X => base.X;

        public new int Y => base.Y;

        public int TargetPositionX { get; internal set; } = 0;

        public int TargetPositionY { get; internal set; } = 0;

        int bpx = 0, bpy = 0;



        public float ArrivalTime { get; internal set; } = 0;

        public bool MoveComplete { get; internal set; } = true;

        public Action? CompleteFunction = null;

        /// <summary>
        /// 원하는 위치에 주어진 시간 내 적당한 속도로 이동합니다.
        /// </summary>
        /// <param name="x">좌표값 X</param>
        /// <param name="y">좌표값 Y</param>
        /// <param name="movetime">이동 시간(밀리초)</param>
        /// /// <param name="starttime">시작 시간(밀리초)</param>
        public void Move(int x,int y,float movetime,float starttime = 0f)
        {
            bpx = base.X;
            bpy = base.Y;
            TargetPositionX = x;
            TargetPositionY = y;
            this.ArrivalTime = movetime;
            this.starttime = Framework.frametimer.ElapsedTicks + (long)(starttime * 10000);
            if (movetime == 0f)
            {
                base.X = x; base.Y = y;
                return; 
            }

            xe = (float)(x - bpx) / movetime;
            ye = (float)(y - bpy) / movetime;

            MoveComplete = false;

        }

        public virtual void Update(float ms)
        {
            if (MoveComplete || Framework.frametimer.ElapsedTicks <= starttime) return;

             nowtime = (Framework.frametimer.ElapsedTicks - starttime) * 0.0001f;

            if (this.ArrivalTime <= nowtime)
            {
                base.X = TargetPositionX;
                base.Y = TargetPositionY;
                MoveComplete = true;
                if (CompleteFunction != null) CompleteFunction();
                return;
            }

            base.X = bpx + (int)(xe * nowtime);
            base.Y = bpy + (int)(ye * nowtime);
        }
    }

    public class TextBox : DrawableObject, IDisposable
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

        public bool Blended { get; set; } = true;

        string txt = string.Empty;

        bool rerender = false;

        public string Text { get => txt; set
            {
                if (this.txt == value) return;
                this.txt = value;
                this.rerender = true;
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
                if (SDL_ttf.TTF_SetFontSize(this.fontsource, fontsize = value) == -1) throw new JyunrcaeaFrameworkException("글꼴 크기 조정에 실패하였습니다.");
                rerender = true;
            }
        }

        Color fc = new();

        Color? bc = null;

        public Color FontColor { get =>this.fc; set {
                this.fc = value;
                this.rerender = true;
            }
        }

        public Color? BackgroundColor
        {
            get => this.bc;
            set
            {
                this.bc = value;
                this.rerender = true;
            }
        }

        public double Rotation = 0;

        bool fh = false, fv = false;

        public bool FlipHorizontal { get => fh; set => flip = ((fh = value) ? SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE) | (fv ? SDL.SDL_RendererFlip.SDL_FLIP_VERTICAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE); }

        public bool FlipVertical { get => fv; set => flip = (fh ? SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE) | ((fv = value) ? SDL.SDL_RendererFlip.SDL_FLIP_VERTICAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE); }

        SDL.SDL_RendererFlip flip = SDL.SDL_RendererFlip.SDL_FLIP_NONE;

        //Font? ft = null;

        IntPtr tt = IntPtr.Zero;

        //public Font? Font { get => font; set {
        //        if (ft == value)
        //        {
        //            this.tt = IntPtr.Zero;
        //            return;
        //        }
        //        ft = value;
        //        this.rerender = true;
        //    }
        //}

        public void Reset()
        {
            needresetsize = false;
            if (this.dx != HorizontalPositionType.Right) this.px = (int)(this.dst.w * (this.dx == HorizontalPositionType.Middle ? -0.5f : -1f));
            else this.px = 0;
            if (this.dy != VerticalPositionType.Bottom) this.py = (int)(this.dst.h * (this.dy == VerticalPositionType.Middle ? -0.5f : -1f));
            else this.py = 0;
            needresetdrawposition = true;
        }

        internal override void Draw()
        {
            if (rerender) TextRender();
            if (needresetposition) ResetPosition();
            if (needresetsize) Reset();
            if (needresetdrawposition)
            {
                this.dst.x = this.px + this.originpos.x + this.mx;
                this.dst.y = this.py + this.originpos.y + this.my;
                needresetdrawposition = false;
            }

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
#if WINDOWS
              (Blended ? SDL_ttf.TTF_RenderUNICODE_Blended(this.fontsource, this.txt, fc.colorbase) : SDL_ttf.TTF_RenderUNICODE_Solid(this.fontsource, this.txt, fc.colorbase)) : SDL_ttf.TTF_RenderText_Shaded(this.fontsource, this.txt, fc.colorbase, this.bc.colorbase);
#else
              (Blended ? SDL_ttf.TTF_RenderUTF8_Blended(this.fontsource, this.txt, fc.colorbase) : SDL_ttf.TTF_RenderUTF8_Solid(this.fontsource,this.txt,fc.colorbase)) : SDL_ttf.TTF_RenderUTF8_Shaded(this.fontsource, this.txt, fc.colorbase,this.bc.colorbase);
#endif
            //Console.WriteLine("{0}, {1}",this.bc == null,Blended);
            if (surface == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"텍스트 렌더링에 실패하였습니다. SDL ttf Error : {SDL_ttf.TTF_GetError()}");
            this.tt = SDL.SDL_CreateTextureFromSurface(Framework.renderer, surface);
            SDL.SDL_FreeSurface(surface);
            if (this.tt == IntPtr.Zero) throw new JyunrcaeaFrameworkException($"텍스쳐로 변환하는데 실패했습니다. SDL Error : {SDL.SDL_GetError()}");
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

        public byte Alpha { get => this.colorbase.b; set => this.colorbase.a = value; }

        internal SDL.SDL_Color colorbase = new();
    }

    public static class Mouse
    {
        internal static SDL.SDL_Point position = new();

        public static int X => position.x;

        public static int Y => position.y;
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

        public static bool MouseOver(DrawableObject sp)
        {
            return SDL.SDL_PointInRect(ref Mouse.position,ref sp.dst) == SDL.SDL_bool.SDL_TRUE;
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

    //public static class SharedTexture
    //{
    //    internal static List<TextureSourceForSharing> list = null!;

    //    internal static List<uint> using_count = new();

    //    internal static SDL.SDL_Rect GetRect(int index)
    //    {
    //        SDL.SDL_QueryTexture(list[index].source, out _, out _, out int w, out int h);
    //        return new() { w = w, h = h };
    //    }

    //    /// <summary>
    //    /// 리스트를 청소해줍니다.
    //    /// 텍스쳐를 매우 많이 로드하고 해체할경우
    //    /// 가끔씩 호출해주세요.
    //    /// </summary>
    //    public static async void GC()
    //    {
    //        await Task.Run(() =>
    //        {
    //            for (int i = list.Count - 1; i >= 0; i--)
    //            {
    //                if (list[i].filename == string.Empty)
    //                {
    //                    list.RemoveAt(i);
    //                    using_count.RemoveAt(i);
    //                }
    //                else break;
    //            }
    //            for (int i = 0; i < list.Count; i++)
    //            {
    //                if (using_count[i] == 0 && list[i].source != IntPtr.Zero)
    //                {
    //                    list[i].Free();
    //                }
    //            }
    //        });
    //    }

    //    public static uint? GetUsingCount(int index)
    //    {
    //        if (index >= list.Count) return null;
    //        return using_count[index];
    //    }

    //    public static int Load(string filename)
    //    {
    //        list.Add(new(filename));
    //        using_count.Add(0);
    //        return list.Count - 1;
    //    }

    //    public static void Remove(int index)
    //    {
    //        list[index].Free();
    //        if (index + 1 == list.Count)
    //        {
    //            list.RemoveAt(index);
    //            using_count.RemoveAt(index);
    //        }
    //        list[index].Dispose();
    //    }

    //    internal static void Ready(int index)
    //    {
    //        if (index >= list.Count) throw new JyunrcaeaFrameworkException($"범위를 벗어난 텍스쳐 값입니다. (저장된 택스쳐 수: {list.Count}, 입력받은 값:{index})");
    //        if (using_count[index] == 0)
    //        {
    //            //forcefree 때문에.
    //            using_count[index] = 0;
    //            list[index].Ready();
    //            if (list[index].source == IntPtr.Zero)
    //            {
    //                throw new JyunrcaeaFrameworkException($"텍스쳐를 불러오지 못했습니다. 텍스쳐 번호: {index}, SDL Error: {SDL.SDL_GetError()}");
    //            }
    //        }
    //        using_count[index]++;

    //    }

    //    internal static void Free(int index)
    //    {
    //        using_count[index]--;
    //        if (using_count[index] <= 0)
    //        {
    //            //forcefree 때문에
    //            using_count[index] = 0;
    //            list[index].Free();
    //        }
    //    }

    //    internal static void AllFocusFree()
    //    {
    //        for(int i=0;i < list.Count;i++)
    //        {
    //            list[i].Free();
    //            list[i].Dispose();
    //        }
    //        list = null!;
    //    }

    //    public static int Count => list.Count;
    //}

    //public class TextureSourceForSharing : IDisposable
    //{
    //    public string filename = string.Empty;
    //    public IntPtr source = IntPtr.Zero;
    //    public bool Ready()
    //    {
    //        if (source != IntPtr.Zero) return false;
    //        source = SDL_image.IMG_LoadTexture(Framework.renderer, filename);
    //        if (this.source == IntPtr.Zero)
    //        {
    //            if (Framework.renderer == IntPtr.Zero) throw new JyunrcaeaFrameworkException("텍스쳐 로드에 실패했습니다. (프레임워크 초기화를 해주세요.) ");
    //            throw new JyunrcaeaFrameworkException($"텍스쳐 로드에 실패했습니다. SDL Error: {SDL.SDL_GetError()}, SDL Image Error: {SDL_image.IMG_GetError()}");
    //        }
    //        return true;
    //    }
    //    public void Free()
    //    {
    //        SDL.SDL_DestroyTexture(this.source);
    //        this.source = IntPtr.Zero;
    //    }
    //    public TextureSourceForSharing()
    //    {

    //    }
    //    public TextureSourceForSharing(string filename)
    //    {
    //        this.filename = filename;
    //    }
    //    public void Dispose()
    //    {
    //        SDL.SDL_DestroyTexture(this.source);
    //        this.filename = string.Empty;
    //        this.source = IntPtr.Zero;
    //    }
    //}

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

    public abstract class DrawableTexture
    {
        public bool FixedRenderRange = false;

        internal bool needresettexture = false;

        internal IntPtr texture;

        internal SDL.SDL_Rect src = new();

        internal SDL.SDL_Point absolutesrc = new();

        public int Width => absolutesrc.x;

        public int Height => absolutesrc.y;

        public void SetRenderRange(int x,int y,int width,int height)
        {
            src.x = x; src.y = y; src.w = width; src.h = height;
            needresettexture = true;
        }

        public RectSize RenderRange
        {
            get => new(src.x, src.y, src.w, src.h);
            set
            {
                this.src = value.size;
                needresettexture = true;
            }
        }

        internal abstract void Ready();

        internal abstract void Free();
    }

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
        }

        internal override void Free()
        {
            key.Free();
            this.texture = IntPtr.Zero;
            this.src.w = this.src.h = 0;
        }
    }

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
        }

        internal override void Free()
        {
            SDL.SDL_DestroyTexture(this.texture);
            this.absolutesrc.x = this.absolutesrc.y = 0;
            this.needresettexture = true;
        }
    }

    [Obsolete("메모리 엑세스 오류가 발생함, 추후 삭제예정",true)]
    public class TextureFromByte : DrawableTexture
    {
        byte[] filedata;

        bool cs;

        /// <summary>
        /// byte[] 형식의 이미지 파일을 불러옵니다.
        /// </summary>
        /// <param name="bytes">파일 데이터</param>
        /// <param name="clearsource">이미지 로딩 후 파일 데이터 제거 여부</param>
        public TextureFromByte(byte[] bytes,bool clearsource = true)
        {
            this.filedata = bytes;
            this.cs = clearsource;
        }

        internal override void Ready() {
            if (filedata == null) throw new JyunrcaeaFrameworkException("null 값이 입력되었습니다.");
            GCHandle gCHandle = GCHandle.Alloc(filedata, GCHandleType.Pinned);
            IntPtr srw = SDL.SDL_RWFromConstMem(gCHandle.AddrOfPinnedObject(), filedata.Length * sizeof(byte));
            if (srw == IntPtr.Zero) { gCHandle.Free(); throw new JyunrcaeaFrameworkException("SDL Error: " + SDL.SDL_GetError()); }
            else SDL.SDL_FreeRW(srw);
            this.texture = SDL_image.IMG_LoadTexture_RW(Framework.renderer,srw, 1);
            gCHandle.Free();
            if (this.texture == IntPtr.Zero) throw new JyunrcaeaFrameworkException("SDL image Error: " + SDL_image.IMG_GetError());
            SDL.SDL_QueryTexture(this.texture, out _, out _, out this.absolutesrc.x, out this.absolutesrc.y);
            if (cs) filedata = null!;
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
            this.absolutesrc.x = this.absolutesrc.y = 0;
            this.needresettexture = true;
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

    public static class OS
    {
        public const bool WindowsOnly
        #if WINDOWS
        = true;
        #else
        = false;
        #endif

        public static OSPlatform Platform { get; internal set; }

        public static void CheckPlatform()
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                Platform = OSPlatform.Windows;
#if !WINDOWS
                Console.WriteLine("이 라이브러리는 윈도우 이외 플랫폼 전용입니다. 실행에 문제가 생길수 있습니다.");
#endif
                return;
            }
#if WINDOWS
            Console.WriteLine("이 라이브러리는 윈도우 전용입니다. 실행에 문제가 생길수 있습니다.");
#endif
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                Platform = OSPlatform.OSX;
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.FreeBSD))
                Platform = OSPlatform.FreeBSD;
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                Platform = OSPlatform.Linux;
            else Platform = OSPlatform.Unknown;
        }
    }

    public enum OSPlatform : byte
    {
        Windows = 0,
        Linux = 1,
        OSX = 2,
        FreeBSD = 3,
        Unknown = 4
    }

    public enum Keycode
    {
         UNKNOWN = 0,

         RETURN = '\r',
         ESCAPE = 27, // '\033'
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
        /*
        Skip uppercase letters
        */
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

        // NUMLOCKCLEAR = (int)SDL_Scancode.SDL_SCANCODE_NUMLOCKCLEAR |  SCANCODE_MASK,
        // KP_DIVIDE = (int)SDL_Scancode.SDL_SCANCODE_KP_DIVIDE |  SCANCODE_MASK,
        // KP_MULTIPLY = (int)SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY |  SCANCODE_MASK,
        // KP_MINUS = (int)SDL_Scancode.SDL_SCANCODE_KP_MINUS |  SCANCODE_MASK,
        // KP_PLUS = (int)SDL_Scancode.SDL_SCANCODE_KP_PLUS |  SCANCODE_MASK,
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
        // POWER = (int)SDL_Scancode.SDL_SCANCODE_POWER |  SCANCODE_MASK,
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

        // LCTRL = (int)SDL_Scancode.SDL_SCANCODE_LCTRL |  SCANCODE_MASK,
        // LSHIFT = (int)SDL_Scancode.SDL_SCANCODE_LSHIFT |  SCANCODE_MASK,
        // LALT = (int)SDL_Scancode.SDL_SCANCODE_LALT |  SCANCODE_MASK,
        // LGUI = (int)SDL_Scancode.SDL_SCANCODE_LGUI |  SCANCODE_MASK,
        // RCTRL = (int)SDL_Scancode.SDL_SCANCODE_RCTRL |  SCANCODE_MASK,
        // RSHIFT = (int)SDL_Scancode.SDL_SCANCODE_RSHIFT |  SCANCODE_MASK,
        // RALT = (int)SDL_Scancode.SDL_SCANCODE_RALT |  SCANCODE_MASK,
        // RGUI = (int)SDL_Scancode.SDL_SCANCODE_RGUI |  SCANCODE_MASK,

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

        // BRIGHTNESSDOWN =
        //(int)SDL_Scancode.SDL_SCANCODE_BRIGHTNESSDOWN |  SCANCODE_MASK,
        // BRIGHTNESSUP = (int)SDL_Scancode.SDL_SCANCODE_BRIGHTNESSUP |  SCANCODE_MASK,
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

    public enum Mousecode : byte
    {
        Left = 1,
        Middle = 2,
        Right = 3
    }

    public class JyunrcaeaFrameworkException : Exception
    {
        public JyunrcaeaFrameworkException() { }
        public JyunrcaeaFrameworkException(string message) : base(message) { }
    }
}