using JyunrcaeaFramework;

class Program
{
    static void Main()
    {
        Init();
        Display.Target.Objects.AddRange(
            new MusicList(),
            new PlayerBar()
            );
        Framework.Run(true);
    }

    static void Init()
    {
        Framework.Init("Jyunni Music Player", 1280, 720);
        Window.BackgroundColor = Color.White;
        Framework.NewRenderingSolution = true;
        Font.DefaultPath = "font.ttf";
        Window.SetMinimizeSize(480, 320);
    }
}

class PlayerBar : Group
{
    const int bar_height = 150;
    const float zoom = 0.8f;

    Box background;
    RoundBox progress;
    Text title;

    public PlayerBar()
    {
        background = new(Window.Width, bar_height,new(210,210,210));
        background.CenterY = 1;
        background.DrawY = VerticalPositionType.Top;
        background.RelativeSize = false;

        progress = new RoundBox((int)(Window.Width * zoom),8,4,new(150,150,150));
        progress.CenterY = 1;
        progress.DrawY = VerticalPositionType.Top;
        progress.RelativeSize = false;

        title = new("(재생중인 음악 없음)",20,Color.Gray);
        title.CenterY = 1;
        title.DrawY = VerticalPositionType.Top;

        this.Objects.AddRange(
            background,
            progress,
            title
            );
    }

    public override void Resize()
    {
        background.Size.Width = Window.Width;
        background.Size.Height = (int)(bar_height * Window.AppropriateSize);

        progress.Size.Width = (int)(Window.Width * zoom);
        progress.Size.Height = (int)(8 * Window.AppropriateSize);
        progress.Radius = (short)(progress.Size.Height * 0.5f + 1);
        progress.Y = (int)(background.Size.Height * -0.3f);

        title.Y = (int)(background.Size.Height * -0.7f);
        base.Resize();
    }

    public override void Update(float ms)
    {
        base.Update(ms);
        this.title.Content = MusicInfo.musicname ?? "(재생중인 음악 없음)";
        if (MusicInfo.musicname is not null) {
            var t = TimeSpan.FromSeconds(Music.NowTime);
            this.title.Content += $" - ({t.Minutes}:{t.Seconds})";
        }
    }
}

class MusicList : Group
{
    bool refresh = true;

    public MusicList()
    {

    }

    public override void Prepare()
    {
        base.Prepare();
        var files = Directory.GetFiles("music");
        foreach ( var file in files )
        {
            if (!file.EndsWith(".mp3")) continue;
            this.Objects.Add(new MusicInfo(file));
        }
        refresh = true;
    }

    public virtual void Arrange()
    {
        int h = (int)(MusicInfo.height * Window.AppropriateSize);
        int pos = Window.Height / 2;
        for ( int i = 0; i < this.Objects.Count; i++ )
        {
            this.Objects[i].Y = h * (i+1) - pos;
        }
    }

    public override void Resize()
    {
        refresh = true;
        base.Resize();
    }

    public override void Update(float ms)
    {
        if (refresh)
        {
            refresh = false;
            Arrange();
        }
        base.Update(ms);
    }
}

class MusicInfo : Button
{
    public const int height = 50;
    public const int width = 200;
    public const int text = 28;

    string Title;

    //RoundBox background = new(width,text,5,Color.LightPeriwinkle);
    Text name;
    Music music;

    public MusicInfo(string path) : base("",width,text,null, new RoundBox(width, text, 5, Color.LightPeriwinkle))
    {
        this.music = new(path);
        this.music.PlayReady = true;
        Title = this.music.Title;

        if (Title == "")
        {
            //Title = path.Remove(path.LastIndexOf(".mp3"));
            Title = path.Remove(path.Length - 4).Remove(0, 6);
        }

        this.Content = Title;
        //this.Background = background;

        //background = new(Window.Width, height, 6, new(240,240,240));
        //background.RelativeSize = false;

        //name = new(Title, 20);

        //background.CenterY = name.CenterY = 0;
        //background.DrawY = name.DrawY = VerticalPositionType.Bottom;

        //this.Objects.AddRange(background, name);
    }

    //public override void Resize()
    //{
    //    background.Size.Width = Window.Width;
    //    background.Size.Height = (int)(height * Window.AppropriateSize);
    //    background.Radius = (short)(6 * Window.AppropriateSize);
    //    name.Y =(int)Math.Round(background.Size.Height * 0.5);
    //    base.Resize();
    //}

    public override void MouseClick()
    {
        musicname = this.Title;
        Music.Play(this.music);
        base.MouseClick();
    }

    public static string? musicname = null;
}

/// <summary>
/// 호버링 기능이 존재하는 평범한 버튼
/// </summary>
public class Button : Group, Events.MouseMove, Events.MouseKeyDown, Events.MouseKeyUp
{
    public ZeneretyDrawableObject? Background { get; internal set; }
    Text title;

    Box hoverbox;

    public Button(string content, int width = 0, int height = 0, Color? textcolor = null, ZeneretyDrawableObject? background = null)
    {
        this.Background = background;
        //this.background = new Box(width,height,color);
        this.hoverbox = new Box(width, height, Color.White);
        this.title = new Text(content, height, textcolor);

        this.AnimationInfo = new(this.hoverbox, 0, null, this.AnimationTime, 1, Animation.Type.EaseInSine);

        if (background is not null) this.Objects.Add(background); this.Objects.Add(hoverbox); this.Objects.Add(title);
    }

    public Animation.Info.Opacity AnimationInfo { get; internal set; }
    public double AnimationTime = 100;
    public byte OpacityOnHover = 100;

    bool nowhover = false;
    public bool IsHover { get; internal set; } = false;

    public override void Prepare()
    {
        base.Prepare();
        this.AnimationInfo.Done();
    }

    /// <summary>
    /// 마우스가 버튼을 눌렀을때 호출되는 함수.
    /// </summary>
    public virtual void MouseClick()
    {

    }

    /// <summary>
    /// 마우스 포인터가 버튼에 닿았을때 호출되는 함수.
    /// </summary>
    public virtual void MouseOver()
    {
        HoverAnimating(this.OpacityOnHover);
    }

    internal virtual void HoverAnimating(byte opacity)
    {
        if (!this.AnimationInfo.Finished) this.AnimationInfo.Stop();
        this.AnimationInfo.Modify(opacity);
        this.AnimationInfo.ResetStartTime = null;
        Animation.Add(this.AnimationInfo);
    }

    /// <summary>
    /// 마우스 포인터가 버튼 밖으로 나갔을때 호출되는 함수.
    /// </summary>
    public virtual void MouseOut()
    {
        HoverAnimating(0);
    }

    public virtual void MouseMove()
    {
        IsHover = Convenience.MouseOver(this.hoverbox);
        if (nowhover != IsHover)
        {
            if (nowhover = IsHover)
            {
                this.MouseOver();
            }
            else
            {
                this.MouseOut();
            }
        }
    }

    public string Content { get => title.Content; set => title.Content = value; }   

    bool ispress = false;

    public virtual void MouseKeyDown(Input.Mouse.Key k)
    {
        if (k != Input.Mouse.Key.Left) return;
        if (this.nowhover) this.ispress = true;
    }

    public virtual void MouseKeyUp(Input.Mouse.Key k)
    {
        if (k != Input.Mouse.Key.Left) return;
        if (this.ispress)
        {
            this.ispress = false;
            if (nowhover)
            {
                this.MouseClick();
            }
        }
    }
}