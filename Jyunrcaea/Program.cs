using JyunrcaeaFramework;

namespace Jyunrcaea
{
    class Program
    {
        public static readonly Version version = new(1, 0, 1);
        public const string Name = "Jyunrcaea!";
        
        static void Main()
        {
            Framework.Init(Name,1280,720,KeepRenderingWhenResize:true);
            Framework.Function = new FrameworkFunctionCustom();
            Font.DefaultPath = "font.ttf";
            Display.Target.Objects.AddRange(
                new MainMenu.BackgroundImage(),
                new MainMenu.Scene(),
                new Setting.Scene(),
                new MusicSelector.Scene(),
                Setting.Data.fps = new Tools.StatusScene()
                );
            try
            {
                Framework.Run(true);
            }
            catch (Exception e)
            {
                Framework.Stop(true);
                Display.Target.Objects.Clear();
                Framework.Init(Name+" Error",960,540,KeepRenderingWhenResize: false);
                Display.Target.Objects.Add(new ErrorScene(e.Message));
                Framework.Run(true);
            }
        }

    }

    class FrameworkFunctionCustom : FrameworkFunction
    {
        public FrameworkFunctionCustom()
        {
            Framework.NewRenderingSolution = true;
            Window.BackgroundColor = Color.White;
            Font.DefaultPath = "font.ttf";
        }

        public override void Start()
        {
            Sounds.Init();
            Display.FrameLateLimit = 0;
            Display.FrameLateLimit = 2 * Display.FrameLateLimit;
            Music.RepeatPlay = true;
            base.Start();
        }

        public override void KeyDown(Input.Keycode e)
        {
            base.KeyDown(e);
            if (e == Input.Keycode.F3)
            {
#if DEBUG
                Debug.ObjectDrawDebuging = !Debug.ObjectDrawDebuging;
#endif
            } else if (e == Input.Keycode.F11)
            {
                Window.Fullscreen = !Window.Fullscreen;
            } 
        }
    }
}

