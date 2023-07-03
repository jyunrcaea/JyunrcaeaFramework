using JyunrcaeaFramework;

class Program
{
    static void Main(string[] args)
    {
        Framework.Init("Jyunrcaea! Framework", 1280, 720, null, null);
        Framework.NewRenderingSolution = true;

        Display.FrameLateLimit = 0;
        Display.FrameLateLimit *= 2;
        Font.DefaultPath = "font.ttf";

        Window.BackgroundColor = Color.Black;
        Display.Target.ObjectList.Add(new Box() { Color = new(), Size = new(Window.Width,Window.Height) });
        Display.Target.ObjectList.Add(new Hello());

        //Display.Target.ObjectList.Add(new PoweredBy());
        Framework.Run();
    }


}

class Hello :Text
{
    public Hello() : base("안녕하세요!", 50, Color.Black) { }

    public override void Resize()
    {
        base.Resize();
    }

    public override void Update(float ms)
    {
        base.Update(ms);
    }
}


class JF : Group
{
    Box background = new() { Color = new(255,240,240) };
    Image icon = new("Icon.png");

    public JF()
    {
        this.ObjectList.Add(background);
        this.ObjectList.Add(icon);
    }

    public override void Prepare()
    {
        base.Prepare();
        background.Size.Width = icon.AbsoluteWidth;
        background.Size.Height = icon.AbsoluteHeight;
        Animation.Add(new Animation.Info.Rotation(icon, 360, null, 2000, 0, null, Animation.Type.EaseInOutSine));
    }

}

class PoweredBy : Group
{
    Image icon = new("Icon.png");
    Text title = new("망했어욤", 30);

    public PoweredBy()
    {
        this.ObjectList.Add(icon);
        this.ObjectList.Add(title);
        title.CenterY = 1;
        title.DrawY = VerticalPositionType.Top;
        title.Background = new();
    }

    public override void Prepare()
    {
        base.Prepare();
        Animation.Add(new Animation.Info.Rotation(icon, 360, null, 2000, 0, null, Animation.Type.EaseInOutSine));
    }

    public override void Update(float ms)
    {
        base.Update(ms);
    }
}