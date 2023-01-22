using JyunrcaeaFramework;


namespace Main
{


    class Program
    {
        public const string ProgramName = "Project Jyunni";

        static void Main(string[] args)
        {
            Framework.Init("Project Jyunni", 1080, 720, null, null, new(true, false, false, false, true));
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
            Window.Icon("cache/icon.png");
            Display.AddScene(new InstallerScene());
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
    }

    class InstallerScene : Scene
    {
        public InstallerScene()
        {
            this.AddSprite(title = new());
            this.AddSprite(subtitle = new());
            title.Opacity(0, 0f);
            subtitle.Opacity(0, 0f);
            title.Opacity(255, 300f, 500f);
            title.OpacityAnimationState.CompleteFunction = () =>
            {
                title.Move(0, (int)(Window.AppropriateSize * -60), 300f, 500f);
                title.OpacityAnimationState.CompleteFunction = null;
                title.MoveAnimationState.CompleteFunction = () =>
                {
                    subtitle.Opacity(255, 300f);
                };
            };
        }

        internal Title title;
        internal SubTitle subtitle;
    }

    class Title : TextboxForAnimation
    {
        public Title() : base("cache/font.ttf",38,Program.ProgramName +" Installer")
        {

        }

        public override void Resize()
        {
            this.Size = (int)(Window.AppropriateSize * 38);
            if (this.MoveAnimationState.Complete) this.Move(0, (int)(Window.AppropriateSize * -60));
            else this.MoveAnimationState.ModifyArrivalPoint(0, (int)(Window.AppropriateSize * -60));
            base.Resize();
        }
    }

    class SubTitle :TextboxForAnimation
    {
        public SubTitle() : base("cache/font.ttf",28,$"'{Program.ProgramName}' 설치를 시작합니다.")
        {

        }

        public override void Resize()
        {
            this.Size = (int)(Window.AppropriateSize * 28);
            base.Resize();
        }
    }
}