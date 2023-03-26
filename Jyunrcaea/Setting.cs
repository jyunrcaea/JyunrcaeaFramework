using JyunrcaeaFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Jyunrcaea.Setting
{
    internal class SettingScene : Scene
    {
        List<TextboxForSetting> settingList = new()
        {
            new("FrameLate: ",Display.FrameLateLimit.ToString()),
        };

        Background bb;

        public SettingScene()
        {
            this.AddSprites(
                    new Blind(),
                    bb = new Background()
                );
            this.AddSprites(settingList.ToArray());
        }

        public override void Start()
        {
            base.Start();
            Resize();
        }

        public override void Resize()
        {
            base.Resize();
            int x = (int)(Window.Width * 0.5f -  400f * Window.AppropriateSize);
            int y = (int)(Window.Height * 0.5f - 300f * Window.AppropriateSize);
            x += (int)(20f * Window.AppropriateSize);
            y += (int)(20f * Window.AppropriateSize);
            int hei = (int)(Window.AppropriateSize * 24);
            for (int i =0; i < settingList.Count; i++) { settingList[i].X = x; settingList[i].Y = y; y += hei; }

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

    class Background : RectangleForAnimation
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

    class TextboxForSetting : GroupObject
    {
        public TextBox Title = new("cache/font.ttf", 0) { FontColor = new(0,0,0), OriginX = HorizontalPositionType.Left, DrawX = HorizontalPositionType.Right, OriginY = VerticalPositionType.Top, DrawY = VerticalPositionType.Bottom };

        public TextBox Text = new("cache/font.ttf",0) { FontColor = new(0, 0, 0), OriginX = HorizontalPositionType.Left, DrawX = HorizontalPositionType.Right, OriginY = VerticalPositionType.Top, DrawY = VerticalPositionType.Bottom };

        public TextboxForSetting(string text1,string text2)
        {
            Title.Text = text1;
            Text.Text = text2;
            this.AddSprite(Title);
            this.AddSprite(Text);
        }

        public override void Resize()
        {
            this.Text.Size = this.Title.Size = (int)(Window.AppropriateSize * 24);
            this.Text.X = this.Title.Width;
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
