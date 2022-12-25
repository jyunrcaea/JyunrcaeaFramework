using JyunrcaeaFramework;

public class Program
{

    static void Main(string[] args)
    {
        Framework.Init("Jyunrcaea! Framework Test", 720, 480, null, null, new(true, false, false, false, true));
        Framework.function = new CustomFrameworkFunction();
        Display.AddScene(new MainScene());
        Window.Icon("Jyunrcaea!FrameworkIcon.png");
        Framework.Run();
    }
}

class CustomFrameworkFunction : FrameworkFunction
{
    public override void Start()
    {
        base.Start();
        Window.Show = true;
    }

    public override void WindowQuit()
    {
        base.WindowQuit();
        Window.Show = false;
        Framework.Stop();
    }
}

class MainScene : Scene
{
    public MainScene()
    {
        this.AddSprite(new TimeText());
    }
}

class TimeText : TextBox, UpdateEventInterface
{
    public TimeText() : base("font.ttf",(int)(Window.AppropriateSize * 25))
    {

    }

    public override void Resize()
    {
        this.Size = (int)(Window.AppropriateSize * 25);
        base.Resize();
    }

    public void Update(float ms)
    {
        this.Text = $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}";
    }
}