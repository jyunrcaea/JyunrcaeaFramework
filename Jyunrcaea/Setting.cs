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
            back = new(Window.Width, Window.Height, new(120,120,120,180));
            back.RelativeSize = false;

            front = new((int)(Window.Width * Data.zoom),(int)(Window.Height * Data.zoom));

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
            Animation.Add(af = new(front, 240, null, Data.opacitytime, 1, Animation.Type.EaseOutSine));
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
        SettingOption perf;

        public Options()
        {
            Title = new("Setting", 30,Color.White);
            Title.DrawY = VerticalPositionType.Top;

            Madeby = new("Made by Jyunni", 18,Color.White);
            Madeby.DrawY = VerticalPositionType.Bottom;

            perf = new SettingOption();

            this.Objects.Add(Title);
            this.Objects.Add(perf);
            this.Objects.Add(Madeby);
        }

        public override void Resize()
        {
            base.Resize();
            Madeby.Y = (int)(Data.height * 0.5);
            Title.Y = -Madeby.Y;
            perf.X = (int)(Data.width * -0.45);
            perf.Y = (int)(Data.height * -0.4);
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

    class CustomButton : Design.Button
    {
        public Action<CustomButton> OnClick;

        public CustomButton(string text,Action<CustomButton> onclick) : base(text,300,24,5)
        {
            this.Background = new Box(300, 24) { Color = new(50, 50, 50, 50) };
            this.OnClick = onclick;
        }

        public override void MouseClick()
        {
            base.MouseClick();
            this.OnClick(this);
        }
    }

    class SettingOption : Design.VerticalList
    {
        CustomButton performancesaving = new("최고 성능 모드: 꺼짐",(me) =>
        {
            if (Framework.SavingPerformance = !Framework.SavingPerformance)
            {
                me.Content = "최고 성능 모드: 꺼짐";
            }
            else
            {
                me.Content = "최고 성능 모드: 켜짐";
            }
        });
        static byte level = 2;
        CustomButton setframelimit = new("초당 프레임: " + Display.MonitorRefreshRate * 2, (me) =>
        {
            if ((level *= 2) > 8) level = 1;
            Display.FrameLateLimit = Display.MonitorRefreshRate * level;
            me.Content = "초당 프레임: " + Display.FrameLateLimit;
        });
        CustomButton eventmutilthreading = new("이벤트 멀티스레딩: 사용", (me) =>
        {
            Framework.EventMultiThreading = !Framework.EventMultiThreading;
            me.Content = "이벤트 멀티스레딩: " + (Framework.EventMultiThreading ? "사용" : "미사용");
        });

        public SettingOption() : base(2)
        {
            this.DrawX = HorizontalPositionType.Right;
            //Performance
            this.Objects.Add(new Text("성능:", 34));
            this.Objects.Add(this.performancesaving);
            this.Objects.Add(this.setframelimit);
            this.Objects.Add(this.eventmutilthreading);
        }

        public override void Prepare()
        {
            base.Prepare();
        }
    }

    class Menu
    {

    }
}
