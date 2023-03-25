using JyunrcaeaFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Jyunrcaea.Setting
{
    internal class SettingScene : ListingScene
    {
        Background bb;

        internal FrameLate fl;

        public SettingScene() {
            this.AddSpriteAtBack(new Blind());
            this.AddSpriteAtBack(bb = new Background());
            this.AddSprites(
                    new DetailTitle(),
                    new Version(),
                    new RunningTime(),
                    new SettingTitle(),
                    fl = new FrameLate(),
                    new FrameLateBackground(),
                    new FrameLateText()
                );
            this.RenderRangeOfListedObjects = new();
        }

        public override void Start()
        {
            base.Start();
            Resize();
        }

        public override void Resize()
        {
            base.Resize();
            this.RenderRangeOfListedObjects.Width = (int)(760 * Window.AppropriateSize);
            this.RenderRangeOfListedObjects.Height = (int)(560 * Window.AppropriateSize);
            this.RenderRangeOfListedObjects.X = (int)(Window.Width * 0.5f - this.RenderRangeOfListedObjects.Width * 0.5f + 20 * Window.AppropriateSize);
            this.RenderRangeOfListedObjects.Y = (int)(Window.Height * 0.5f - this.RenderRangeOfListedObjects.Height * 0.5f + 20 * Window.AppropriateSize);
        }
    }

    class Blind : Rectangle
    {
        public Blind() : base(1080, 720)
        {
            this.Color = new(128, 128, 128, 100);
        }

        public override void Resize()
        {
            this.Width = Window.Width;
            this.Height = Window.Height;
            base.Resize();
        }
    }

    class Background : Rectangle
    {
        public Background() : base(800, 600)
        {
            this.Radius = 8;
            this.Color.Alpha = 255;
        }

        public override void Resize()
        {
            this.Width = (int)(800 * Window.AppropriateSize);
            this.Height = (int)(600 * Window.AppropriateSize);
            base.Resize();
        }
    }

    class DetailTitle : DefaultTextbox
    {
        public DetailTitle() : base("Program Information",46) { }

        public override void Resize()
        {
            this.Size = (int)(46 * Window.AppropriateSize);
            base.Resize();
        }
    }

    class SettingTitle : DefaultTextbox, ListingOptionInterface
    {
        public SettingTitle() : base("Settting",46) { }

        public int LastMargin { get; } = 20;
        public int NextMargin { get; } = 0;
        public ListingLineOption ListingLineOption { get; } = ListingLineOption.NextLine;

        public override void Resize()
        {
            this.Size = (int)(46 * Window.AppropriateSize);
            //this.X = (int)(-300 * Window.AppropriateSize);
            //this.Y = (int)(-300 * Window.AppropriateSize) + (int)(this.Size * 1.1);
            base.Resize();
        }
    }

    class DefaultTextbox : TextBox
    {
        public DefaultTextbox(string text, int size =0) : base("cache/font.ttf", size, text)
        {
            this.FontColor = new(0, 0, 0);
            this.OriginX = HorizontalPositionType.Left;
            this.OriginY = VerticalPositionType.Top;
            this.DrawX = HorizontalPositionType.Right;
            this.DrawY = VerticalPositionType.Bottom;
        }
    }

    class Version : DefaultTextbox
    {
        public Version() : base($"Jyunrcaea! Version : {Jyunrcaea.Store.Version}") { }

        public override void Resize()
        {
            this.Size = (int)(24 * Window.AppropriateSize);
            base.Resize();
        }
    }

    class RunningTime : DefaultTextbox, UpdateEventInterface
    {
        public RunningTime() : base("Loading Time...") { }

        public override void Resize()
        {
            this.Size = (int)(24 * Window.AppropriateSize);
            base.Resize();
        }

        public void Update(float ms)
        {
            this.Text = "Running time : " + (Framework.RunningTime * 0.001f ).ToString("0.0") +"s";
        }
    }

    class FrameLate : DefaultTextbox, ListingOptionInterface
    {
        public FrameLate() : base("Frame Late : ") { }

        public int LastMargin { get; } = 0;
        public int NextMargin { get; } = 0;
        public ListingLineOption ListingLineOption { get; } = ListingLineOption.Collocate;

        public override void Start()
        {
            //this.Text = "Frame Late : " + Display.FrameLateLimit.ToString();
            //if (Display.FrameLateLimit == Display.MonitorRefreshRate)
            //{
            //    this.Text += " (= Monitor Refresh Rate)";
            //}
            base.Start();
        }

        public override void Resize()
        {
            this.Size = (int)(24 * Window.AppropriateSize);
            //this.X = (int)(-380 * Window.AppropriateSize);
            //this.Y = (int)(-150 * Window.AppropriateSize);
            base.Resize();
        }
    }

    class FrameLateBackground : RectangleForAnimation, ListingOptionInterface, MouseMoveEventInterface
    {
        public FrameLateBackground()
        {
            this.Color = new(150, 150, 150,0);
            this.OriginX = HorizontalPositionType.Left;
            this.OriginY = VerticalPositionType.Top;
            this.DrawX = HorizontalPositionType.Right;
            this.DrawY = VerticalPositionType.Bottom;
            this.Opacity(0);
        }

        public int LastMargin { get; } = 0;
        public int NextMargin { get; } = 0;
        public ListingLineOption ListingLineOption { get; } = ListingLineOption.StayStill;

        public override void Resize()
        {
            this.Height = ((SettingScene)this.InheritedObject).fl.Height;
            this.Width = (int)(48 * Window.AppropriateSize);
            base.Resize();
        }

        bool hovered = false;

        public void MouseMove()
        {
            if (hovered)
            {
                if (Convenience.MouseOver(this)) return;
                this.Opacity(0, 250f);
                hovered = false;
            } else
            {
                if (!Convenience.MouseOver(this)) return;
                this.Opacity(128, 250f);
                hovered = true;
            }
        }
    }

    class FrameLateText : DefaultTextbox
    {
        public FrameLateText() : base(Display.FrameLateLimit.ToString()) {
            this.FontColor = new();
        }

        public override void Resize()
        {
            this.Size = (int)(24 * Window.AppropriateSize);
            this.X = 4;
            base.Resize();
        }
    }
}

//namespace Jyunrcaea
//{
//    internal class SettingScene : Scene
//    {
//        public SettingScene() {
//            //this.Hide = true;
//            this.AddSprite(new Blind());
//            this.AddSprite(new Background());
//            this.AddSprite(new Title());
//            this.AddSprite(new FrameLate());
//        }

//        public override void Start()
//        {
//            base.Start();
//            this.Resize();
//        }
//    }


//}
