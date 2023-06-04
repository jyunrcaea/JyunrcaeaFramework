using JyunrcaeaFramework;


class Program
{
    const bool IsZenerety = true;

    static void Main()
    {
        if (IsZenerety)
        {
            Framework.Init("Zenerety", 1280, 720);
            Framework.NewRenderingSolution = true;
            Window.BackgroundColor = new();
            Image Icon = new Image("Icon.png");
            Display.Target.ObjectList.Add(Icon);
            Framework.Function = new FF();
            Display.FrameLateLimit = 120;
            Animation.Add(new Animation.Info.Rotation(Icon,360,1000,1000,0,null,Animation.GetAnimation(AnimationType.Easing)));
            Framework.Run();
        } else
        {
            Framework.Init("NotZenerety", 1280, 720);
            Framework.NewRenderingSolution = false;
            Window.BackgroundColor = new();
            Scene scene = new();
            scene.AddSprite(new JFICON());
            Display.AddScene(scene);
            Framework.Function = new FF();
            Display.FrameLateLimit = 120;
            Framework.Run();
        }

    }

    class JFICON : Sprite, Events.Update
    {
        public JFICON() : base("Icon.png") { }

        public void Update(float ms)
        {
            this.Rotation += ms * 0.5;
            if (this.Rotation >= 360)
                this.Rotation -= 360;
        }
    }

} 

class FF : FrameworkFunction
{
    int fpscount = 0;
    double endtime = 0;
    public override void Update(float ms)
    {
        if (endtime <= Framework.RunningTime)
        {
            endtime += 1000;
            if (endtime <= Framework.RunningTime)
            {
                Console.WriteLine("FPS: {0}\nFPS: 0, StunTime: {1}ms",fpscount,Framework.RunningTime - endtime);
                endtime = Framework.RunningTime + 1000;
                fpscount = 0;
            }
            Console.WriteLine("FPS: {0}", fpscount);
            fpscount = 0;
        }
        fpscount++;
        base.Update(ms);
    }

    public override void KeyDown(Input.Keycode e)
    {
        base.KeyDown(e);
        if (e == Input.Keycode.F11)
        {
            Window.Fullscreen = !Window.Fullscreen;
        } else if (e == Input.Keycode.F10)
        {
            Window.DesktopFullscreen = false;
        } else if (e == Input.Keycode.F9)
        {
            Window.Fullscreen = true;
        } else if (e== Input.Keycode.F8)
        {
            Window.Fullscreen = false;
        } else if (e == Input.Keycode.F3)
        {
            Debug.ObjectDrawDebuging = !Debug.ObjectDrawDebuging;
        }


    }
}