using JyunrcaeaFramework;

class Program
{
    static void Main(string[] args)
    {
        Framework.Init("Jyunrcaea!", 1280, 720, null, null);
        Framework.NewRenderingSolution = true;
        Framework.SavingPerformance = false;

        Window.BackgroundColor = new();

        Display.Target.ObjectList.Add(new JF());

        Framework.Run();
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