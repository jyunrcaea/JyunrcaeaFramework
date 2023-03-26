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
        List<DrawableObject> settingList = new()
        {
            new TitleForSetting("Jyunrcaea! Information",32),
            new TextboxForSetting("Version: ",Store.Version),
            new TextboxForSetting("Framework Version: ",Framework.Version.ToString()),
            new EmptySpaceForSetting(30),
            new TitleForSetting("Display Option",32),
            new TextboxForSetting("Framelate Limit: ",$"{Display.FrameLateLimit.ToString()} frame/s"),
            new EmptySpaceForSetting(30),
            new TitleForSetting("Skin",32),
            new TextboxForSetting("Skin Folder Directory: ","Data/Skin/Jyunrcaea!"),
            new EmptySpaceForSetting(30),
            new TitleForSetting("Debug",32),
            new TitleForSetting("Reset Setting Data", 24) { BackgroundColor = new(200,200,200,128)},
            new TitleForSetting("Uninstall", 24) { BackgroundColor = new(200,200,200,128)},
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
            int hei = (int)(Window.AppropriateSize * 25.5f);
            for (int i =0; i < settingList.Count; i++) { settingList[i].X = x; settingList[i].Y = y; if (settingList[i] is TextBox) y += ((TextBox)settingList[i]).Height; else if (settingList[i] is GhostObject) y += ((GhostObject)settingList[i]).Height; else y += hei; }

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

    class TitleForSetting : TextBox
    {
        int defaultsize;

        public TitleForSetting(string text,int size) : base("cache/font.ttf",size,text) {
            defaultsize = size;
            this.FontColor = new(0, 0, 0);
            OriginX = HorizontalPositionType.Left;
            DrawX = HorizontalPositionType.Right;
            OriginY = VerticalPositionType.Top;
            DrawY = VerticalPositionType.Bottom;
        }

        public override void Resize()
        {
            this.Size = (int)(defaultsize * Window.AppropriateSize);
            base.Resize();
        }
    }

    class EmptySpaceForSetting : GhostObject
    {
        int defaultsize;

        public EmptySpaceForSetting(int size) : base(0,0,5, size) {
            OriginX = HorizontalPositionType.Left;
            DrawX = HorizontalPositionType.Right;
            OriginY = VerticalPositionType.Top;
            DrawY = VerticalPositionType.Bottom;
            defaultsize = size;
        }

        public override void Resize()
        {
            this.Height = (int)(defaultsize * Window.AppropriateSize);
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
