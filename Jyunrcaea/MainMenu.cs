using JyunrcaeaFramework;

namespace Jyunrcaea.MainMenu
{
    public static class Control
    {
        public static Scene Scene = null!;

        public static void Appear()
        {
            Scene.Hide = false;
        }

        public static void Disappear()
        {
            Scene.Hide = true;
        }
    }

    public class Scene : Group
    {
        public Scene()
        {
            //this.Objects.Add(new BackgroundImage());
            this.Objects.Add(new FullBox(64, 64, 64, 64));
            this.Objects.Add(new Version());
            this.Objects.Add(new LeftBar());
            //this.Objects.Add(new Design.VerticalList(2,new Text("hello",20),new Text("world",30),new Design.Button("Hello world",200,20)));
        }

        public override void Prepare()
        {
            base.Prepare();
            Control.Scene = this;
        }
    }

    class Version : Text
    {
        public Version() : base($"{Program.Name} ({Program.version} ver)",20,Color.White)
        {
            this.CenterY = 1;
            this.DrawY = VerticalPositionType.Top;

            this.Y = 25;
            Animation.Add(new Animation.Info.Movement(this, null, 0, 614, 200));
        }

    }

    public class BackgroundImage : Image, Events.Resize, Events.MouseMove
    {
        public static BackgroundImage self;

        public BackgroundImage() : base("bg1.jpg")
        {
            this.RelativeSize = false;
            self = this;
        }
        double ratio;
        double zoom = 1.2;

        public void Resize()
        {
            ratio = (double)Window.Width * zoom / (double)this.Texture.Width;
            if (ratio * this.Texture.Height < Window.Height * zoom)
            {
                borderlength = (int)(Window.Height * 0.1);
                ratio = (double)Window.Height * zoom / (double)this.Texture.Height;
            }
            else borderlength = (int)(Window.Width * 0.1);
            this.Scale.X = ratio;
            this.Scale.Y = ratio;
        }

        int borderlength = 0;

        public void MouseMove()
        {
            this.X = (int)(((double)Input.Mouse.X / (double)Window.Width - 0.5) * borderlength);
            this.Y = (int)(((double)Input.Mouse.Y / (double)Window.Height - 0.5) * borderlength);
        }
    }

    class LeftBar : Group
    {
        Box background;
        public Text Title;
        Selector Selector = new();

        public LeftBar()
        {
            background = new Box(
                (int)(Window.DefaultWidth * 0.4),
                Window.Height,
                new(230, 230, 230, 140)
            )
            {
                RelativeSize = false,
            };
            background.CenterX = 0;
            background.DrawX = HorizontalPositionType.Right;

            Title = new Text(
                Program.Name,
                38
            );
            Title.CenterX = 0;
            Title.CenterY = 0.2;


            this.Objects.AddRange(
                background,
                Title,
                Selector
            );
        }

        public override void Prepare()
        {
            this.X = -background.Size.Width;
            base.Prepare();
            Animation.Add(new Animation.Info.Movement(this, 0, null, 614, 250, 1, Animation.Type.EaseOutQuad));
        }

        public override void Resize()
        {
            Title.X = (background.Size.Width = (int)(Window.DefaultWidth * 0.4 * Window.AppropriateSize)) / 2;
            background.Size.Height = Window.Height;
            base.Resize();
        }
    }

    class Selector : Group, Events.MouseMove, Events.MouseKeyDown, Events.MouseKeyUp
    {
        Button Start;
        Button Setting;
        Button Exit;

        internal Box Select;

        internal const int ButtonWidth = 300;
        internal const int ButtonHeight = 28;
        internal const byte ColorValue = 250;
        internal const byte OpacityValue = 150;

        public Selector()
        {
            Start = new("Start", -1);
            Setting = new("Setting",0);
            Exit = new("Exit", 1);

            Select = new(ButtonWidth,ButtonHeight,new(ColorValue, ColorValue, ColorValue, 0));
            Select.CenterX = 0;

            this.Objects.AddRange(
                Select,
                Start,
                Setting,
                Exit
            );

        }

        int before = 0;
        int option = 0;

        public void MouseMove()
        {
            if (Convenience.MouseOver(Start.box))
            {
                option = 1;
            } else if (Convenience.MouseOver(Setting.box))
            {
                option = 2;
            } else if (Convenience.MouseOver(Exit.box))
            {
                option = 3;
            } else
            {
                option = 0;
            }

            if (before != option)
            {
                if (option != 0)
                {
                    if(Jyunrcaea.Setting.Scene.Disappear) Sounds.default_hover.Play();
                    MoveAnimating();
                }
                if (before == 0 || option == 0)
                {
                    if (ao is not null) ao.Stop(true);
                    Animation.Add(new Animation.Info.Opacity(Select,(option == 0) ? byte.MinValue : OpacityValue, null, DefaultAnimationTime,TimeCalculator: Animation.Type.EaseInQuad,FunctionWhenFinished: (i) => ao = null));
                }
                before = option;
            }
        }

        Animation.Info.Opacity? ao = null;
        const double DefaultAnimationTime = 90;
        bool IsAnimating = false;
        Animation.Info.Movement am = null!;

        void MoveAnimating()
        {
            if (IsAnimating)
            {
                am.Stop();
            }
            IsAnimating = true;
            Animation.Add(am = new Animation.Info.Movement(Select,null,SelectProperSize,null,DefaultAnimationTime,TimeClaculator: Animation.Type.EaseOutQuad,FunctionWhenFinished: (i) => IsAnimating = false));
        }

        int SelectProperSize => (int)(Select.Size.Height * Window.AppropriateSize * (option - 2));

        public override void Resize()
        {
            base.Resize();
            Select.X = this.TypedParent<LeftBar>().Title.X;
            if (IsAnimating)
            {
                am.EditEndPoint(null, SelectProperSize);
            }
        }

        int hoveroption;

        public void MouseKeyDown(Input.Mouse.Key k)
        {
            if (k != Input.Mouse.Key.Left) return;
            hoveroption = option;
        }

        public void MouseKeyUp(Input.Mouse.Key k)
        {
            if (!Jyunrcaea.Setting.Scene.Disappear || hoveroption != option || k != Input.Mouse.Key.Left) return;
            //if (hoveroption != 0) Sounds.button_play_select.Play();
            switch (hoveroption)
            {
                case 0:
                    return;
                case 1:
                    Sounds.button_play_select.Play();
                    Jyunrcaea.MusicSelector.Control.Show = true;
                    break;
                case 2:
                    Sounds.dropdown_open.Play();
                    Jyunrcaea.Setting.Scene.Disappear = false;
                    break;
                case 3:
                    Framework.Stop();
                    break;
            }
        }
    }

    class Button : Group
    {
        public Text text;
        public GhostBox box;

        int pos;

        public Button(string text,int pos = 0,int width = Selector.ButtonWidth,int height = Selector.ButtonHeight)
        {
            this.text = new(text,height);
            this.text.CenterX = 0;
            box = new(width, height);
            box.CenterX = 0;

            this.pos = pos;

            this.Objects.AddRange(
                box,
                this.text
             );
        }

        public override void Prepare()
        {
            base.Prepare();
            text.Update(0);
            this.box.Size.Height = this.text.AbsoluteHeight;
            this.TypedParent<Selector>().Select.Size.Height = this.text.AbsoluteHeight;
        }

        public override void Resize()
        {
            base.Resize();
            this.box.X = this.text.X = this.Parent!.TypedParent<LeftBar>().Title.X;
            this.Y = (int)(this.box.Size.Height * Window.AppropriateSize * this.pos);
        }

        
    }
}
