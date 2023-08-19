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
    }
}

class MusicInfo : Group, Events.MouseKeyDown
{
    const int height = 100;

    string Title;

    RoundBox background;
    Text name;
    Music music;

    public MusicInfo(string path)
    {
        this.music = new(path);
        this.music.PlayReady = true;
        Title = this.music.Title;

        if (Title == "")
        {
            //Title = path.Remove(path.LastIndexOf(".mp3"));
            Title = path.Remove(path.Length - 4).Remove(0, 6);
        }

        background = new(Window.Width, height, 6, new(240,240,240));
        background.RelativeSize = false;

        name = new(Title, 20);

        this.Objects.AddRange(background, name);
    }

    public override void Resize()
    {
        background.Size.Width = Window.Width;
        background.Size.Height = (int)(height * Window.AppropriateSize);
        background.Radius = (short)(6 * Window.AppropriateSize);
        base.Resize();
    }

    public void MouseKeyDown(Input.Mouse.Key k)
    {
        if (k != Input.Mouse.Key.Left) return;
        if (Convenience.MouseOver(background))
        {
            musicname = this.Title;
            Music.Play(this.music);
        }
    }

    public static string? musicname = null;
}