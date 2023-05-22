using JyunrcaeaFramework;


namespace Jyunrcaea
{
    public static class Store
    {
        public const string Version = "0.2.2 Development Version";
    }

    class JFIcon : Sprite
    {
        public JFIcon() : base(new TextureFromFile("Jyunrcaea!FrameworkIcon.png"))
        {
            
        }

        public override void Resize()
        {
            this.Width = 300;
            this.Height = 300;
            base.Resize();
        }
    }

    public class TestScene : Scene
    {
        public static bool Test = false;

        public TestScene()
        {
            this.AddSprite(new JFIcon());
        }
    }

    class Program
    {
        public static MusicList.MainScene musiclistscene = null!;

        public static WindowState ws = null!;

        public static Setting.SettingScene ss = null!;

        public static Framerate fr = null!;

        static void Main(string[] args)
        {
            //Framework.SavingPerformance = false;
            Framework.Init("Jyunrcaea!", 1080, 720, null, null, new(true,  false, false, true));
            Window.BackgroundColor = new(225,223,252);
            if (!Directory.Exists("cache"))
            {
                var df = Directory.CreateDirectory("cache");
                df.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            if (!File.Exists("cache/font.ttf"))
            {
                File.WriteAllBytes("cache/font.ttf", Jyunrcaea.Properties.Resources.font);
            }
            if (!File.Exists("cache/icon.png"))
            {
                File.WriteAllBytes("cache/icon.png", Jyunrcaea.Properties.Resources.Icon);
            }
            if (!File.Exists("cache/background.png"))
            {
                File.WriteAllBytes("cache/background.png", Jyunrcaea.Properties.Resources.background);
            }
            Window.Icon("cache/icon.png");
            Display.FrameLateLimit = 120;
            //Framework.EventMultiThreading = true;
            Display.AddScene(new MainMenu.MainScene());
            Display.AddScene(musiclistscene = new MusicList.MainScene());
            Display.AddScene(ss = new Setting.SettingScene());
            Display.AddScene(ws = new WindowState());
            Display.AddScene(fr = new Framerate());
            if(TestScene.Test) Display.AddScene(new TestScene());
            Framework.Function = new CustomFrameworkFunction();
            Framework.Run();
        }


    }

    class CustomFrameworkFunction : FrameworkFunction
    {
        public CustomFrameworkFunction() { }

        public override void Start()
        {
            base.Start();
            Window.Show = true;
        }

        public override void WindowQuit()
        {
            base.WindowQuit();
            Framework.Stop();
        }

        public override void WindowMove()
        {
            base.WindowMove();
        }

        public static bool fullscreenswicthed = false;

        public override void KeyDown(Input.Keycode e)
        {
            base.KeyDown(e);
            switch (e)
            {
#if DEBUG
                case Input.Keycode.F1:
                    Debug.ObjectDrawDebuging = !Debug.ObjectDrawDebuging;
                    break;
#endif
                case Input.Keycode.F2:
                    Window.Resize(Window.DefaultWidth, Window.DefaultHeight);
                    break;
                case Input.Keycode.F3:
                    break;
                case Input.Keycode.F4:
                    Program.fr.Hide = !Program.fr.Hide;
                    break;
#if DEBUG
                case Input.Keycode.F5:
                    while (Debug.LogCount != 0)
                        Console.WriteLine(Debug.GetLog);
                    break;
#endif
                case Input.Keycode.F6:
                    Window.Move(null, null);
                    break;
#if DEBUG
                case Input.Keycode.F7:
                    Console.WriteLine(Debug.TextRenderCount);
                    break;
#endif
                case Input.Keycode.F8:
                    Framework.SavingPerformance = !Framework.SavingPerformance;
                    break;
                case Input.Keycode.F9:
                    //Framework.SavingPerformanceLevel = 255;
                    break;
                case Input.Keycode.F10:
                    Window.Raise();
                    break;
                case Input.Keycode.F11:
                    fullscreenswicthed = true;
                    Window.Fullscreen = !Window.Fullscreen;
                    break;
                case Input.Keycode.F12:
                    break;
                case Input.Keycode.ESCAPE:
                    Framework.Stop();
                    break;
            }
        }
    }

    class WindowState : Scene
    {
        StateText t;
        StateBackground b;

        public WindowState()
        {
            this.AddSprite(b = new());
            this.AddSprite(t = new());

            t.Opacity(0); b.Opacity(0);
        }

        public override void Start()
        {
            base.Start();
            t.Resize();
            b.Resize();
        }

        public override void Resized()
        {
            t.Y = b.Y = (int)(Window.Height * 0.2f);
            t.Size = (int)(20 * Window.AppropriateSize);
            b.Width = (int)(400 * Window.AppropriateSize);
            b.Height = t.Size * 2;
            if (CustomFrameworkFunction.fullscreenswicthed) {
                CustomFrameworkFunction.fullscreenswicthed = false;
                Show($"창 모드 변경됨: {(Window.Fullscreen ? "전체화면" : "창화면" )} ({Window.Width} × {Window.Height})");
            }
            else Show($"창 크기 조절됨: {Window.Width} × {Window.Height}");
        }

        public void Show(string text)
        {
            t.Text = text;
            t.Opacity(200, 200f);
            b.Opacity(200, 200f);
        }
    }

    class StateText : TextboxForAnimation
    {
        public  StateText() : base("cache\\font.ttf",0) {
            this.FontColor = new(0, 0, 0);
            this.OpacityAnimationState.CompleteFunction = () =>
            {
                this.Opacity(0, 200f, 500f);
            };
        }

    }

    class StateBackground : RectangleForAnimation
    {
        public StateBackground()
        {
            this.Radius = 8;
            this.OpacityAnimationState.CompleteFunction = () =>
            {
                this.Opacity(0, 200f, 500f);
            };
        }
    }
}