using JyunrcaeaFramework;

namespace Jyunrcaea
{
    class Program
    {
        public static readonly Version version = new(0, 1, 1);

        static void Main()
        {
            Framework.Init("Jyunrcaea!",1280,720);
            Framework.Function = new FrameworkFunctionCustom();

            Display.Target.Objects.AddRange(
                new MainMenu.Scene()
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
            base.Start();
        }

        public override void KeyDown(Input.Keycode e)
        {
            base.KeyDown(e);
            if (e == Input.Keycode.F3)
            {
                Debug.ObjectDrawDebuging = !Debug.ObjectDrawDebuging;
            }
        }
    }
}

