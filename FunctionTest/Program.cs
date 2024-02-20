using JyunrcaeaFramework;

class Program
{
    static void Main()
    {
        Framework.Init("Test", 1280, 720);
        Window.BackgroundColor = Color.Gray;
        Display.Target.Objects.Add(new Scene1());
        Framework.Run();
    }
}

class Scene1 : Group
{
    Box box;

    public Scene1()
    {
        box = new(Window.Width>>1,Window.Height>>1);

        this.Objects.Add(box);
    }

    public override void Prepare()
    {
        base.Prepare();
        Display.Target.Objects.Add(new Scene2());
    }
}

class Scene2 : Group
{
    Text text;

    public Scene2()
    {
        text = new Text("hello world!",28,null,"font.ttf");

        this.Objects.Add(text);
    }
}