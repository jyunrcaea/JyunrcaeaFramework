using JyunrcaeaFramework;

namespace Minigration.Home;

public class Scene : Group
{
    Text title, sub;
    Option opt;

    public Scene()
    {
        title = new("Minigration",36);
        title.CenterY = 0.2;

        sub = new("Made by Jyunni",16);
        sub.CenterY = 1;
        sub.DrawY = VerticalPositionType.Top;

        opt = new();

        this.Objects.AddRange(title, sub,opt);
    }
}

public class Option : Jyunrcaea.Design.VerticalList
{
    ActionButton host, client, exit;

    public Option()
    {
        host = new("Create Room", () =>
        {
            
        });
        client = new("Connect", () =>
        {

        });
        exit = new("Exit", () =>
        {
            Framework.Stop();
        });

        this.Objects.AddRange(host, client, exit);
    }
}

public class ActionButton : Jyunrcaea.Design.TextButton
{
    Action action;

    public ActionButton(string text, Action act) : base(text,240,32)
    {
        this.action = act;
    }

    public override void MouseClick()
    {
        action.Invoke();
        base.MouseClick();
    }
}

public class ConnectWindow : Group
{
    Box background;
    Text title;
    Text enterline;
    ActionButton back, start;

    public ConnectWindow()
    {
        background = new(Window.Width, Window.Height, new(100,100,100,100));
        background.RelativeSize = false;

        title = new("Create password for room.");
        enterline = new("");
    }

    public override void Resize()
    {
        background.Size = (Window.Width, Window.Height);
        base.Resize();
    }

    public override void Update(float ms)
    {
        enterline.Content = Input.Text.Content;
        base.Update(ms);
    }
}