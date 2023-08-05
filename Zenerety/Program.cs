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

        Window.BackgroundColor = Color.White;

        Display.Target.Objects.Add(new Windows());
        Display.Target.Objects.Add(new FrameAnalyze());

        Framework.Run(CallResize: true, ShowWindow: true);
    }
}


class Windows : Group, Events.KeyDown
{

    public Windows()
    {
        this.Objects.AddRange(
            new WindowForm("614project", 720, 480),
            new WindowForm("sus", 614, 360, 100, 100,new Info())
            );
    }

    public void KeyDown(Input.Keycode k)
    {
        switch (k)
        {
            case Input.Keycode.F3:
#if DEBUG
                Debug.ObjectDrawDebuging = !Debug.ObjectDrawDebuging;
#endif
                break;
        }
    }
}

class WindowForm : Group, Events.MouseMove, Events.MouseKeyDown, Events.MouseKeyUp
{
    Box background = new();
    Box TitleBar = new();
    Text title;
    Box Close = new(30, 30, new(255, 10, 10, 100)) { RelativeSize = false };

    Color Normal = new(150, 150, 150, 128);
    Color Hover = new(150, 150, 200, 150);

    GhostBox Horizon;
    GhostBox Vertical;

    Group content = null!;

    public WindowForm(string title, int width, int height, int x = 0, int y = 0, Group? content = null)
    {
        //Horizon
        Horizon = new(width, 6);
        Horizon.CenterY = 0;
        Horizon.Y = height;
        Horizon.CenterX = 0;
        Horizon.DrawX = HorizontalPositionType.Right;
        Horizon.RelativeSize = false;

        //Vertical
        Vertical = new(6, height);
        Vertical.CenterX = 0;
        Vertical.X = width;
        Vertical.CenterY = 0;
        Vertical.DrawY = VerticalPositionType.Bottom;
        Vertical.RelativeSize = false;

        //background
        background.RelativeSize = TitleBar.RelativeSize = false;
        background.Size.Width = TitleBar.Size.Width = width;
        background.Size.Height = height;
        background.Color = new(200, 200, 200, 128);

        //title
        this.title = new(title, 20) { RelativeSize = false, X = 2 };
        TitleBar.Color = this.Normal;

        //this
        this.X = x;
        this.Y = y;
        this.Objects.AddRange(
            background,
            TitleBar,
            Close,
            this.title,
            Horizon,
            Vertical
            );

        TitleBar.Size.Height = 30;
        TitleBar.CenterY = this.title.CenterY = this.background.CenterY = 0;
        TitleBar.DrawY = this.title.DrawY = this.background.DrawY = VerticalPositionType.Bottom;
        TitleBar.CenterX = this.title.CenterX = background.CenterX = 0;
        TitleBar.DrawX = this.title.DrawX = background.DrawX = HorizontalPositionType.Right;


        //Close
        Close.CenterY = 0;
        Close.CenterX = 0;
        Close.DrawX = HorizontalPositionType.Left;
        Close.DrawY = VerticalPositionType.Bottom;
        Close.X = width;

        if (content is not null)
        {
            this.Objects.Add(this.content = content);
            this.content.RenderRange = new(width,height);
        }
    }

    public int Width
    {
        get => background.Size.Width;
        set
        {
            background.Size.Width = value;
            TitleBar.Size.Width = value;
            if (content is not null) content.RenderRange!.Width = value;
            Close.X = value;
            this.Horizon.Size.Width = value;
            this.Vertical.X = value;
        }
    }

    public int Height
    {
        get => background.Size.Height;
        set
        {
            background.Size.Height = value;
            this.Vertical.Size.Height = value;
            if (content is not null) content.RenderRange!.Height = value - 30;
            this.Horizon.Y = value;
        }
    }

    public override void Update(float ms)
    {
        base.Update(ms);
    }

    public void MouseMove()
    {
        if (grab)
        {
            this.X = Input.Mouse.X + mousew;
            this.Y = Input.Mouse.Y + mouseh;
            if (content is null) return;
            content.X = this.X;
            content.Y = this.Y + 30;
            return;
        }
        if (resizing)
        {
            if (horizon)
            {
                this.Width = Input.Mouse.X - this.X;
            }
            else
            {
                this.Height = Input.Mouse.Y - this.Y;
            }
        }
        if (Convenience.MouseOver(TitleBar))
        {
            TitleBar.Color = this.Hover;
        }
        else
        {
            TitleBar.Color = this.Normal;
        }
        if (Convenience.MouseOver(Close))
        {
            Close.Color.Alpha = 255;
        }
        else
        {
            Close.Color.Alpha = 100;
        }
    }

    public override void Prepare()
    {
        base.Prepare();
    }

    bool grab = false;
    bool resizing = false;
    bool horizon = false;

    int mousew = 0;
    int mouseh = 0;

    public void MouseKeyDown(Input.Mouse.Key key)
    {
        if (key != Input.Mouse.Key.Left) return;
        if (Convenience.MouseOver(Close))
        {
            this.Parent!.Objects.Remove(this);
            return;
        }
        if (Convenience.MouseOver(TitleBar))
        {
            Input.Mouse.SetCursor(Input.Mouse.CursorType.Move);
            grab = true;
            mousew = this.X - Input.Mouse.X;
            mouseh = this.Y - Input.Mouse.Y;
            this.Parent!.Objects.Switch(this);
            return;
        }
        if (Convenience.MouseOver(Horizon))
        {
            Input.Mouse.SetCursor(Input.Mouse.CursorType.VericalSizing);
            resizing = true;
            horizon = false;
            return;
        }
        if (Convenience.MouseOver(Vertical))
        {
            Input.Mouse.SetCursor(Input.Mouse.CursorType.HorizonSizing);
            resizing = true;
            horizon = true;
            return;
        }
    }

    public void MouseKeyUp(Input.Mouse.Key key)
    {
        if (key != Input.Mouse.Key.Left) return;
        grab = false;
        resizing = false;
        Input.Mouse.SetCursor(Input.Mouse.CursorType.Arrow);
    }
}

class Info : Group
{
    Text title;

    public Info()
    {
        title = new("1 + 1 = 1",30);
        this.Objects.Add(title);
    }
}

class FrameAnalyze : Text
{
    public FrameAnalyze() : base("측정중...", 20)
    {
        this.CenterX = 0;
        this.CenterY = 1;
        this.DrawX = HorizontalPositionType.Right;
        this.DrawY = VerticalPositionType.Top;
    }

    int framecount = 0;

    uint endtime = 1000;

    public override void Update(float ms)
    {
        if (endtime <= Framework.RunningTime)
        {
            endtime += 1000;
            this.Content = "FPS: " + framecount;
            framecount = 0;
        }
        framecount++;
        base.Update(ms);
    }
}

class One : Circle
{
    public One()
    {
        this.Radius = 15;
        this.Color = new(200, 150, 150,150);
    }
}