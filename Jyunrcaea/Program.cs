using JyunrcaeaFramework;

namespace Jyunrcaea
{
    class Program
    {
        public static readonly Version version = new(0, 1, 2);

        static void Main()
        {
            Framework.Init("Jyunrcaea!",1280,720);
            Framework.Function = new FrameworkFunctionCustom();
            

            Font.DefaultPath = "font.ttf";

            //Display.Target.Objects.Add(
            //    new Intro.Scene()
            //    );

            Display.Target.Objects.AddRange(
                new MainMenu.BackgroundImage(),
                new MainMenu.Scene(),
                new Setting.Scene(),
                new MusicSelector.Scene(),
                new Tools.StatusScene()
                );


            Framework.Run(true);
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
            Display.FrameLateLimit = 0;
            Display.FrameLateLimit = 2 * Display.FrameLateLimit;
            base.Start();
        }

        public override void KeyDown(Input.Keycode e)
        {
            base.KeyDown(e);
            if (e == Input.Keycode.F3)
            {
                Debug.ObjectDrawDebuging = !Debug.ObjectDrawDebuging;
            } else if (e == Input.Keycode.F11)
            {
                Window.Fullscreen = !Window.Fullscreen;
            }
        }
    }
}

