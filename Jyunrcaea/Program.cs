using JyunrcaeaFramework;


namespace Jyunrcaea
{
    public static class Store
    {
        public const string Version = "0.2.2 Development Version";
    }

    class GoodBackground : Canvas
    {
        TextureFromFile t;
        RectSize s = new();

        public GoodBackground()
        {
            t = new("test.png");
            t.Opacity = 100;
            this.AddUsingTexture(t);
        }

        public override void Start()
        {
            Resize();
            base.Start();
        }

        public override void Render()
        {
            Renderer.BlendMode(Renderer.BlendType.Mul);
            Renderer.Texture(t,s);
        }

        public override void Resize()
        {
            s.Width = Window.Width;
            s.Height = Window.Height;
        }
    }

    class JFIcon : Sprite
    {
        public JFIcon() : base(new TextureFromFile("Jyunrcaea!FrameworkIcon.png"))
        {
            
        }

        public override void Resize()
        {
            this.Size = Window.AppropriateSize * 0.5f;
            base.Resize();
        }
    }

    public class TestGruop : GroupObject
    {
        public TestGruop()
        {
            this.AddSprite(new JFIcon());
            
        }

        public override void Resize()
        {
            base.Resize();
        }
    }

    class Program
    {
        public static MusicList.MainScene musiclistscene = null!;

        public static WindowState ws = null!;

        public static Setting.SettingScene ss = null!;

        static void Main(string[] args)
        {
            Framework.SavingPerformance = false;
            Framework.Init("Jyunrcaea!", 1080, 720, null, null, new(true,  false, false, true));
            //Window.Resize((int)(Display.MonitorWidth * 0.7f), (int)(Display.MonitorHeight * 0.7f));
            //Window.Move(null, null);
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
            Display.FrameLateLimit = 240;

            Display.AddScene(new MainMenu.MainScene());
            Display.AddScene(musiclistscene = new MusicList.MainScene());
            //Display.AddScene(new GoodBackground());
            Display.AddScene(ss = new Setting.SettingScene());
            Display.AddScene(ws = new WindowState());
            //Display.AddScene(new TitleBar.Border());
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
            if (e == Input.Keycode.F1)
                Debug.ObjectDrawDebuging = !Debug.ObjectDrawDebuging;
            else if (e == Input.Keycode.F11)
            {
                fullscreenswicthed = true;
                Window.Fullscreen = !Window.Fullscreen;
            } else if (e == Input.Keycode.ESCAPE)
            {
                Framework.Stop();
            } else if (e== Input.Keycode.F2)
            {
                Window.Resize(Window.DefaultWidth, Window.DefaultHeight);
            } else if (e == Input.Keycode.F4)
            {
                Window.Move(null, null);
            } else if (e == Input.Keycode.F12)
            {
                while (Debug.LogCount != 0)
                    Console.WriteLine(Debug.GetLog);
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