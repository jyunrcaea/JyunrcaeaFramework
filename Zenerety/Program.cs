using JyunrcaeaFramework;

class Program
{
    static void Main(string[] args)
    {
        Framework.Init("Jyunrcaea!", 1280, 720, null, null);

        Framework.NewRenderingSolution = true;

        Framework.SavingPerformance = false;

        Display.Target.ObjectList.Add(new JF());

        Framework.Run();
    }

}

class JF : Group
{
    Box background = new() { Color = new() };
    Image icon = new("Icon.png");

    public JF()
    {
        this.ObjectList.Add(background);
        this.ObjectList.Add(icon);
    }

    public override void Prepare()
    {
        base.Prepare();
        Animation.Add(new Animation.Info.Rotation(icon, 360, null, 2000, 0, null, Animation.Type.EaseInOutSine));
    }
}