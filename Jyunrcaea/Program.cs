using JyunrcaeaFramework;


namespace Jyunrcaea
{
    public static class Store
    {
        public const string Version = "0.1";
    }

    class Program
    {
        static void Main(string[] args)
        {
            Framework.SavingPerformance = false;
            Framework.Init("Jyunrcaea!", 1080, 720, null, null, new(true, false, false, false, true));
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

        public override void KeyDown(Keycode e)
        {
            if (e == Keycode.F1)
                Framework.ObjectDrawDebuging = !Framework.ObjectDrawDebuging;
            base.KeyDown(e);
        }
    }

    
}