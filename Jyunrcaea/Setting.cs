using JyunrcaeaFramework;

namespace Jyunrcaea.Setting
{
    internal class Scene : Group
    {
        static Scene me = null!;
        static bool hidden = true;
        public static bool Disappear { get => hidden; set {
                if (hidden == value) return;
                hidden = value;
                if (value)
                {
                    me.b.Disappear();
                    me.o.Disappear();
                } else
                {
                    me.b.Appear();
                    me.o.Appear();
                }
            }
        }

        public Scene()
        {
            this.Hide = true;

            this.Objects.Add(b = new Background());
            this.Objects.Add(o = new Options());
        }

        public Background b;
        public Options o;

        public override void Prepare()
        {
            Data.defaultw = (int)(Window.Width * Data.zoom);
            Data.defaulth = (int)(Window.Height * Data.zoom);
            base.Prepare();
            me = this;
        }

        public override void Resize()
        {
            Data.width = (int)(Data.defaultw * Window.AppropriateSize);
            Data.height = (int)(Data.defaulth * Window.AppropriateSize);
            base.Resize();
        }
    }

    public static class Data
    {
        public const double opacitytime = 200;
        public const double zoom = 0.7;
        public static int width = 0;
        public static int height = 0;
        public static int defaultw = 0;
        public static int defaulth = 0;
    }

    class Background : Group, Events.MouseKeyDown, Events.MouseKeyUp
    {
        Box back;
        Box front;
        Text close;

        public Background()
        {
            back = new(Window.Width, Window.Height, new(150,150,150,100));
            back.RelativeSize = false;

            front = new((int)(Window.Width * Data.zoom),(int)(Window.Height * Data.zoom));
            front.Color.Alpha = 200;

            close = new(" ×",26);
            close.DrawX = HorizontalPositionType.Right;
            close.DrawY = VerticalPositionType.Bottom;

            this.Objects.AddRange(
                back,
                front,
                close
            );
        }

        public override void Prepare()
        {
            base.Prepare();
            ab = new(back, 0, null, 0);
            af = new(front, 0, null, 0);
            ae = new(close, 0, null, 0);
            ab.Done();
            af.Done();
            ae.Done();
        }

        Animation.Info.Opacity ab=null!,af=null!,ae=null!;

        public void Appear()
        {
            if (!ab.Finished)
            {
                ab.Stop(true);
                af.Stop();
                ae.Stop();
            }
            this.Parent!.Hide = false;
            Animation.Add(ab = new(back, 100, null, Data.opacitytime, 1, Animation.Type.EaseOutSine));
            Animation.Add(af = new(front, 200, null, Data.opacitytime, 1, Animation.Type.EaseOutSine));
            Animation.Add(ae = new(close, 255, null, Data.opacitytime, 1, Animation.Type.EaseOutSine));
        }

        public void Disappear()
        {
            if (!ab.Finished)
            {
                ab.Stop();
                af.Stop();
                ae.Stop();
            }
            Animation.Add(ab = new(back, 0, null, Data.opacitytime, 1, Animation.Type.EaseOutSine, (i) => i.Parent!.Parent!.Hide = true));
            Animation.Add(af = new(front, 0, null, Data.opacitytime, 1, Animation.Type.EaseOutSine));
            Animation.Add(ae = new(close, 0, null, Data.opacitytime, 1, Animation.Type.EaseOutSine));
        }

        public override void Resize()
        {
            back.Size.Width = Window.Width;
            back.Size.Height = Window.Height;
            base.Resize();
            close.X = (int)(this.front.Size.Width * -0.5 * Window.AppropriateSize);
            close.Y = (int)(this.front.Size.Height * -0.5 * Window.AppropriateSize);
        }

        bool hoverclose = false;

        public void MouseKeyDown(Input.Mouse.Key k)
        {
            if (k != Input.Mouse.Key.Left) return;
            hoverclose = Convenience.MouseOver(close);
            if(hoverclose) close.TextColor = Color.Gray;
        }

        public void MouseKeyUp(Input.Mouse.Key k)
        {
            if (hoverclose && k == Input.Mouse.Key.Left) Scene.Disappear = true;
            close.TextColor = Color.Black;
        }
    }

    class Options : Group
    {
        Text Title;
        Text Madeby;

        public Options()
        {
            Title = new("Setting", 30);
            Title.DrawY = VerticalPositionType.Bottom;

            Madeby = new("Made by Jyunni", 18);
            Madeby.DrawY = VerticalPositionType.Top;

            this.Objects.Add(Title);
            this.Objects.Add(Madeby);
        }

        public override void Resize()
        {
            base.Resize();
            Madeby.Y = (int)(Data.height * 0.5);
            Title.Y = -Madeby.Y;
        }

        public override void Prepare()
        {
            base.Prepare();
            ag = new(this, 0, null, Data.opacitytime);
            ag.Done();
        }

        Animation.InfoForGroup.Opacity ag = null!;

        public void Appear()
        {
            if (!ag.Finished)
            {
                ag.Stop();
            }
            ag.TargetOpacity = 255;
            Animation.Add(ag);
        }

        public void Disappear()
        {
            if (!ag.Finished)
            {
                ag.Stop();
            }
            ag.TargetOpacity = 0;
            Animation.Add(ag);
        }
    }

    class Menu
    {

    }
}
