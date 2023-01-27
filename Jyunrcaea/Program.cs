using JyunrcaeaFramework;


namespace Main
{


    class Program
    {
        public const string ProgramName = "Project Jyunni";

        static void Main(string[] args)
        {
            Framework.SavingPerformance = false;
            Framework.Init("Animation Test", 1080, 720, null, null, new(true, false, false, false, true));
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
            Display.FrameLateLimit = 240;
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
            this.AddSprite(new FPSCounter());
            this.AddSprite(title = new());
            this.AddSprite(subtitle = new());
            this.AddSprite(jficon = new());
            title.Opacity(0, 0f);
            subtitle.Opacity(0, 0f);
            jficon.Opacity(0, 0f);
            title.Opacity(255, 300f, 500f);
            title.OpacityAnimationState.CompleteFunction = () =>
            {
                title.Move(0, (int)(Window.AppropriateSize * -160), 300f, 500f);
                title.OpacityAnimationState.CompleteFunction = null;
                title.MoveAnimationState.CompleteFunction = () =>
                {
                    subtitle.Opacity(255, 300f);
                    subtitle.OpacityAnimationState.CompleteFunction = () =>
                    {
                        subtitle.Move(0, (int)(Window.AppropriateSize * 160), 300f,500f);
                        subtitle.OpacityAnimationState.CompleteFunction = null;
                        subtitle.MoveAnimationState.CompleteFunction = () =>
                        {
                            jficon.Opacity(255, 300f, 500f);
                            jficon.OpacityAnimationState.CompleteFunction = () =>
                            {
                                jficon.Move((int)(Window.Width * 0.5f), 0, 250f,500f);
                                jficon.MoveAnimationState.CompleteFunction = jficon.Left;
                            };
                            subtitle.MoveAnimationState.CompleteFunction = null;
                        };
                    };
                };
            };
        }

        internal Title title;
        internal SubTitle subtitle;
        internal JFIcon jficon;
    }

    class FPSCounter : TextBox, UpdateEventInterface
    {
        public FPSCounter() : base("cache/font.ttf",25,"Loading...") {
            OriginX = HorizontalPositionType.Left;
            OriginY = VerticalPositionType.Top;
            DrawX = HorizontalPositionType.Right;
            DrawY = VerticalPositionType.Bottom;
        }

        float finishtime = 1000f;

        int count = 0;

        public void Update(float ms)
        {
           if (finishtime <= Framework.RunningTime)
            {
                this.Text = "FPS: " + count;
                count = 1;
                finishtime += 1000f;
                return;
            }
            count++;
        }
    }

    class Title : TextboxForAnimation
    {
        public Title() : base("cache/font.ttf",38,"Animation")
        {
            this.MoveAnimationState.CalculationFunction = Animation.GetAnimation(AnimationType.Ease_Out);
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
        public SubTitle() : base("cache/font.ttf",28,"아무튼 애니메이션임")
        {
            this.MoveAnimationState.CalculationFunction = Animation.GetAnimation(AnimationType.Ease_In);
        }

        public override void Resize()
        {
            this.Size = (int)(Window.AppropriateSize * 28);
            base.Resize();
        }
    }

    class JFIcon : SpriteForAnimation
    {
        public JFIcon() : base(new TextureFromFile("cache/icon.png"))
        {
            this.MoveAnimationState.CalculationFunction = Animation.GetAnimation(AnimationType.EaseInOutQuad);
            this.Size = 0.3f;
        }

        public override void Resize()
        {
            base.Resize();
        }

        public void Right()
        {
            this.Move((int)(Window.Width * 0.5f), 0, 500f,100f);
            this.MoveAnimationState.CompleteFunction = Left;
        }

        public void Left()
        {
            this.Move((int)(Window.Width * -0.5f), 0, 500f,100f);
            this.MoveAnimationState.CompleteFunction = Right;
        }
    }
}