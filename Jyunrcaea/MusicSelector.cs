using Jyunrcaea.MainMenu;
using JyunrcaeaFramework;

namespace Jyunrcaea.MusicSelector
{
    public static class Control
    {
        public static Scene Scene = null!;

        static bool show = false;

        public static bool Show
        {
            get => show;
            set
            {
                if (show == value) return;
                if (show = value)
                {
                    Appear();
                    MainMenu.Control.Disappear();
                } else
                {
                    Disappear();
                    MainMenu.Control.Appear();
                }
            }
        }

        public static void Appear()
        {
            Scene.Resize();
            Scene.Hide = false;
        }

        public static void Disappear()
        {
            Scene.Hide = true;
        }
    }

    public class Thumbnail : Image, Events.Resize
    {
        double len;

        public Thumbnail() : base("dae_sang_huck.png")
        {
            this.CenterX = 0.25;
            self = this;
        }

        public void Resize()
        {
            this.Scale.Y =  this.Scale.X = 500.0 / this.Texture.Width;
        }

        public static Thumbnail self;
    }

    public class Scene : Group
    {
        BitmapList list;
        Thumbnail thumbnail;

        public Scene()
        {
            this.Hide = true;

            this.Objects.Add(new FullBox(64,64,64,100));
            this.Objects.Add(new Close());
            this.Objects.Add(list = new BitmapList());
            this.Objects.Add(thumbnail = new());
        }

        public override void Prepare()
        {
            base.Prepare();
            Control.Scene = this;
            var dires = Directory.GetDirectories("music");
            foreach (var dire in dires)
            {
                if (!Directory.Exists(dire))
                {
                    continue;
                }
                if (!File.Exists(dire + "\\music.mp3"))
                {
                    continue;
                }
                if (!File.Exists(dire + "\\setup.txt"))
                {
                    continue;
                }
                Texter texter = new(dire + "\\setup.txt");
                if (!texter.IsLoad) continue;
                string name = texter.Get("name");
                string artist = texter.Get("artist");
                string mapper = texter.Get("mapper");
                BitmapInfo info = new(name,artist,mapper, dire + "\\music.mp3",0,0,dire);
                this.list.Objects.Add(new BitmapBar(info));
            }

            this.list.Resize();
        }
    }

    public class Data
    {
        public static BitmapInfo select;
        public static bool loading = false;
    }

    public class BitmapList : Design.VerticalList
    {
        public BitmapList() : base(2)
        {
            this.DrawX = HorizontalPositionType.Left;

            //this.Objects.Add(new BitmapBar("Feeling", "KiRist"));
            //this.Objects.Add(new BitmapBar("Test", "Jyunni"));
        }

        public override void Resize()
        {
            base.Resize();
            this.X = (int)(Window.Width * 0.5f);
        }
    }

    public class BitmapInfo
    {
        public string name;
        public string artist;
        public string mapper;
        public string musicfile, dir;
        public double start,end;
        public string? shortpath, bgpath;

        public BitmapInfo(string name, string artist, string mapper, string music_path,double start,double end, string dir)
        {
            this.name = name;
            this.artist = artist;
            this.mapper = mapper;
            this.musicfile = music_path;
            this.start = start;
            this.end = end;
            this.dir = dir;
            shortpath = dir + "\\short.png";
            if (!File.Exists(shortpath))
            {
                shortpath = dir + "\\short.jpg";
                if (!File.Exists(shortpath))
                {
                    shortpath = null;
                }
            }
            bgpath = dir + "\\bg.png";
            if (!File.Exists(bgpath))
            {
                bgpath = dir + "\\bg.jpg";
                if (!File.Exists(bgpath))
                {
                    bgpath = null;
                }
            }
        }
    }

    public class BitmapBar : Design.ButtonBox
    {
        Box BG;
        Text Name;
        Text Artist;
        BitmapInfo info = null!;
        TextureFromFile thumbnail, bg;

        public BitmapBar(BitmapInfo info) : base(614,80,0)
        {
            this.info = info;

            this.BG = new(614, 80,new(240,240,240,220));
            this.Background = this.BG;

            this.Name = new(info.name, 28);
            this.Name.RelativeSize = true;
            this.Name.DrawX = HorizontalPositionType.Right;
            this.Name.DrawY = VerticalPositionType.Bottom;

            this.Artist = new(info.artist, 18,Color.DarkGray);
            this.Artist.RelativeSize = true;
            this.Artist.DrawX = HorizontalPositionType.Right;

            this.RelativeSize = true;

            this.Objects.Add(this.Name); this.Objects.Add(this.Artist);
        }

        public override void Prepare()
        {
            base.Prepare();
            if (info.shortpath is not null) thumbnail = new(info.shortpath);
            if (info.bgpath is not null) bg = new(info.bgpath);
        }

        public override void MouseClick()
        {
            base.MouseClick();
            Data.select = this.info;
            if (info.shortpath is not null)
            {
                Thumbnail.self.Texture = thumbnail;
                Thumbnail.self.Resize();
            }
            if (info.bgpath is not null)
            {
                BackgroundImage.self.Texture = bg;
                BackgroundImage.self.Resize();
            }
            Music music = new(this.info.musicfile);
            Music.Play(music);
        }

        public override void Resize()
        {
            base.Resize();
            this.Name.X = this.Artist.X = (int)(this.DisplayedWidth * -0.5 + Window.AppropriateSize * 2);
            this.Name.Y = (int)(this.DisplayedHeight * -0.5);
        }

        public override void MouseOver()
        {
            Sounds.button_hover.Play();
            base.MouseOver();
        }
    }

    public class Close : Design.TextButton
    {
        public Close() : base("×",30,26,2)
        {
            this.Background = new Box(30, 30, new(240,240,240,230));

            this.RelativeSize = true;
        }   

        public override void Resize()
        {
            base.Resize();
            this.X = (int)((this.DisplayedWidth - Window.Width) * 0.5);
            this.Y = (int)((this.DisplayedHeight - Window.Height) * 0.5);
        }

        public override void MouseKeyDown(Input.Mouse.Key k)
        {
            base.MouseKeyDown(k);
            if (!Control.Show) return;
            if (k == Input.Mouse.Key.Prev) Control.Show = false;
        }

        public override void MouseClick()
        {
            base.MouseClick();
            if (!Control.Show) return;
            Control.Show = false;
        }
    }
}
