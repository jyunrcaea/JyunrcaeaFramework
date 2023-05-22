using JyunrcaeaFramework;


class Program
{
    static void Main()
    {
        Framework.Init("Zenerety", 1280, 720);
        Framework.NewRenderingSolution = true;
        Window.BackgroundColor = new();
        Image Icon = new Image { Texture = new TextureFromFile("Icon.png") };
        Display.Target.ObjectList.Add(Icon);
        Framework.Function = new FF();
        Animation.Add(new Animation.Info.Rotation(Icon,360,1000,1000,0,null,Animation.GetAnimation(AnimationType.Easing)));
        Framework.Run();
    }

}

class FF : FrameworkFunction
{
    public override void KeyDown(Input.Keycode e)
    {
        base.KeyDown(e);
        if (e == Input.Keycode.F11)
        {
            Window.DesktopFullscreen = true;
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