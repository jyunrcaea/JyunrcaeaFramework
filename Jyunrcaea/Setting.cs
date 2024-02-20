using JyunrcaeaFramework;

namespace Jyunrcaea.Setting
{
    internal class Scene : Group, Events.KeyDown
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

        public void KeyDown(Input.Keycode key)
        {
            //if (key is Input.Keycode.o) Input.KeyBoard.Reset();
            //if (this.Hide && Input.KeyBoard.IsPress(Input.Keycode.LCTRL)) Disappear = false;
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

        public static Tools.StatusScene fps;
    }

    class Background : Group
    {
        class CloseButton : Design.TextButton
        {
            public CloseButton() : base("×", 26, 26, 0)
            {
                this.HoverColor = Color.Silver;
                this.RelativeSize = true;
            }

            public override void MouseClick()
            {
                base.MouseClick();
                Sounds.dropdown_close.Play();
                Scene.Disappear = true;
            }

            public override void MouseOver()
            {
                Sounds.button_hover.Play();
                base.MouseOver();
            }
        }

        Box back;
        Box front;
        CloseButton close;

        public Background()
        {
            back = new(Window.Width, Window.Height, new(120,120,120,180));
            back.RelativeSize = false;

            front = new((int)(Window.Width * Data.zoom),(int)(Window.Height * Data.zoom));

            close = new();

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
            //ae = new(close, 0, null, 0);
            ab.Done();
            af.Done();
            //ae.Done();
        }

        Animation.Info.Opacity ab=null!,af=null!,ae=null!;

        public void Appear()
        {
            close.Hide = false;
            if (!ab.Finished)
            {
                ab.Stop(true);
                af.Stop();
                //ae.Stop();
            }
            this.Parent!.Hide = false;
            Animation.Add(ab = new(back, 100, null, Data.opacitytime, 1, Animation.Type.EaseOutSine));
            Animation.Add(af = new(front, 240, null, Data.opacitytime, 1, Animation.Type.EaseOutSine,(e) =>
            {
                CustomButton.GetEvent = true;
            }));
        }

        public void Disappear()
        {
            close.Hide = true;
            if (!ab.Finished)
            {
                ab.Stop();
                af.Stop();
                //ae.Stop();
            }
            Animation.Add(ab = new(back, 0, null, Data.opacitytime, 1, Animation.Type.EaseOutSine, (i) => i.Parent!.Parent!.Hide = true));
            Animation.Add(af = new(front, 0, null, Data.opacitytime, 1, Animation.Type.EaseOutSine));
            //Animation.Add(ae = new(close, 0, null, Data.opacitytime, 1, Animation.Type.EaseOutSine));

            CustomButton.GetEvent = false;
        }

        public override void Resize()
        {
            back.Size.Width = Window.Width;
            back.Size.Height = Window.Height;
            base.Resize();
            close.X = (int)((this.close.DisplayedWidth - this.front.DisplayedWidth) * 0.5f);
            close.Y = (int)((this.close.DisplayedHeight - this.front.DisplayedHeight) * 0.5f);
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

            Madeby = new($"Jyunrcaea! Framework Version: {Framework.Version.ToString()}", 18,Color.White);
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
            //ag.ApplySubGroup = true;
            ag.Done();
        }

        Animation.InfoForGroup.Opacity ag = null!;

        public void Appear()
        {
            this.perf.Hide = false;
            if (!ag.Finished)
            {
                ag.Stop();
            }
            ag.TargetOpacity = 255;
            Animation.Add(ag);
        }

        public void Disappear()
        {
            this.perf.Hide = true;
            if (!ag.Finished)
            {
                ag.Stop();
            }
            ag.TargetOpacity = 0;
            Animation.Add(ag);
        }
    }

    class CustomButton : Design.TextButton
    {
        public static bool GetEvent = false;

        public Action<CustomButton> OnClick;

        public ShowDes? showDes;

        public CustomButton(string text,Action<CustomButton> onclick,string? des = null) : base(text,300,24,5)
        {
            this.Background = new Box(300, 24 + this.Margin * 2) { Color = new(100, 100, 100, 50) };
            this.OnClick = onclick;
            if (des is not null)
            {
                showDes = new(des);
                this.Objects.Add(showDes);
            }
        }

        public override void MouseClick()
        {
            if (!GetEvent) return;
            Sounds.osd_change.Play();
            base.MouseClick();
            this.OnClick(this);
        }

        public override void MouseOver()
        {
            if (!GetEvent) return;
            base.MouseOver();
            Sounds.default_hover.Play();
            if (showDes is null) return;
            showDes.Appear();
        }

        public override void MouseOut()
        {
            base.MouseOut();
            if (showDes is null) return;
            showDes.Disappaer();
        }

        public override void Resize()
        {
            if (showDes is not null)
            {
                this.showDes.X = this.DisplayedWidth;
            }
            base.Resize();
        }
    }

    class ShowDes : Group
    {
        public Box background;
        public Text des;
        
        public void Appear()
        {
            if (!opacity.Finished) opacity.Stop();
            this.Hide = false;
            opacity.FinishedAction = null;
            opacity.TargetOpacity = 255;
            opacity.AnimationTime = 200;
            opacity.StartTime = null;
            Animation.Add(opacity);
        }

        public void Disappaer()
        {
            if (!opacity.Finished) opacity.Stop();
            opacity.FinishedAction = group =>
            {
                group.Hide = true;
            };
            opacity.TargetOpacity = 0;
            opacity.AnimationTime = 200;
            opacity.StartTime = null;
            Animation.Add(opacity);
        }

        private Animation.InfoForGroup.Opacity opacity;

        public ShowDes(string des)
        {
            this.Hide = true;
            
            this.background = new(280, 30, new(230, 230, 230));
            this.des = new(des, 18, Color.Black);
            this.background.RelativeSize = false;
            this.des.RelativeSize = true;
             
            this.Objects.AddRange(
                this.background,
                this.des
                );

            this.opacity = new(this, 0, null, 200);
            this.opacity.TimeCalculator = Animation.Type.EaseOutSine;
            this.opacity.Done();
        }

        public override void Resize()
        {
            base.Resize();
            this.background.Scale.X = Window.AppropriateSize;
            this.des.WrapWidth = (uint)(this.background.DisplayedWidth * 0.9);
            this.Update(0);
            if (this.background.Opacity == 0)
            {
                this.Hide = true;
                this.des.Opacity = 0;
            }
            this.background.Size.Height = (int)(this.des.DisplayedHeight * 1.25);
        }
    }

    class SettingOption : Design.VerticalList
    {
        public static GhostBox EmptySpace => new(1, 15);
        public static Text Title(string text) => new Text(text,34);

        CustomButton performancesaving = new("성능 절약: 켜짐",(me) =>
        {
            if (Framework.SavingPerformance = !Framework.SavingPerformance)
            {
                me.Content = "성능 절약: 켜짐";
            }
            else
            {
                me.Content = "성능 절약: 꺼짐";
            }
        },"입력을 새로고침하는 빈도를 줄여 cpu 사용량을 줄입니다.\n대신 게임 내에서 입력이 약간 지연될수 있습니다.");
        static byte level = 2;

        private CustomButton setframelimit = new("초당 프레임: " + Display.MonitorRefreshRate * 2, (me) =>
        {
            if ((level *= 2) > 8) level = 1;
            if (level == 8) Display.FrameLateLimit = 1000;
            else Display.FrameLateLimit = Display.MonitorRefreshRate * level;
            me.Content = "초당 프레임: " + Display.FrameLateLimit;
        });
        CustomButton eventmutilthreading = new("입력 멀티스레딩: 미사용", (me) =>
        {
            Framework.EventMultiThreading = !Framework.EventMultiThreading;
            me.Content = "입력 멀티스레딩: " + (Framework.EventMultiThreading ? "사용" : "미사용");
        },"입력을 병렬로 처리합니다.\n(아직 불안정한 기능입니다.)");
        CustomButton fullscreen = new("전체화면 전환", (_) =>
        {
            Window.Fullscreen = !Window.Fullscreen;
        },"전체화면으로 전환합니다.\n('F11' 키로 전환할수도 있습니다.)");

        private CustomButton showfps = new("초당 프레임 표시: 꺼짐", (me) =>
        {
            if (Data.fps.Hide) {
                Data.fps.Enable();
                me.Content = "초당 프레임 표시: 켜짐";
            }
            else
            {
                Data.fps.Disable();
                me.Content = "초당 프레임 표시: 꺼짐";
            }
        });

        public SettingOption() : base(2)
        {
            this.DrawX = HorizontalPositionType.Right;
            //Performance
            this.Objects.Add(Title("성능"));
            this.Objects.Add(this.performancesaving);
            //this.Objects.Add(this.eventmutilthreading);

            this.Objects.Add(EmptySpace);
            //Graphics
            this.Objects.Add(Title("그래픽"));
            this.Objects.Add(this.fullscreen);
            this.Objects.Add(this.showfps);
            this.Objects.Add(this.setframelimit);


            //this.Objects.Add(EmptySpace);
            ////Debug
            //this.Objects.Add(Title("디버그"));
            //this.Objects.Add(new CustomButton("객체 오버레이 표시: 꺼짐", me =>
            //{
            //    if (Debug.ObjectDrawDebuging)
            //    {
            //        Debug.ObjectDrawDebuging = false;
            //        me.Content = "객체 오버레이 표시: 꺼짐";
            //    } else
            //    {
            //        Debug.ObjectDrawDebuging = true;
            //        me.Content = "객체 오버레이 표시: 켜짐";
            //    }
            //},"현재 불러온 모든 객체들의 테두리 영역을 표시합니다.\n(개발자용)"));
        }
    }

    class Menu
    {

    }
}
