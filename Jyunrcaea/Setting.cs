using Jyunrcaea.MainMenu;
using JyunrcaeaFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
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
        Blind bd;
        HoverButton hb;

        public SettingScene()
        {
            this.Hide = true;
            this.AddSprites(
                    bd = new Blind(),
                    bb = new Background(),
                    hb = new HoverButton("< Exit")
                );
            this.AddSprites(settingList.ToArray());
        }

        public override void Start()
        {
            base.Start();
            Hidden();
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

        public void Shown()
        {
            bd.OpacityAnimationState.CompleteFunction = null;
            this.Hide = false;
            bd.Opacity(100, 300f);
            bb.Opacity(255, 200f);
            //hb.Shown();
            ((SupportOpacity)hb).Shown();
            settingList.ForEach((a) =>
            {
                if (a is not SupportOpacity) a.Hide = false;
                else ((SupportOpacity)a).Shown();
            });
        }

        public void Hidden()
        {
            bd.OpacityAnimationState.CompleteFunction = () => { this.Hide = true; };
            settingList.ForEach((a) =>
            {
                if (a is not SupportOpacity) a.Hide = true;
                else ((SupportOpacity)a).Hidden();
            });
            hb.Hidden();
            bd.Opacity(0, 300f);
            bb.Opacity(0, 200f);

        }

        bool ctrl = false;

        public override void KeyDown(Input.Keycode e)
        {
            base.KeyDown(e);
            if (e == Input.Keycode.LCTRL)
            {
                ctrl = true;
            } else if (e == Input.Keycode.o && ctrl)
            {
                if (!bd.MoveAnimationState.Complete) return;
                if (this.Hide)
                {
                    this.Shown();
                } else
                {
                    this.Hidden();
                }
            } 
        }

        public override void KeyUp(Input.Keycode e)
        {
            base.KeyUp(e);
            if (e == Input.Keycode.LCTRL)
            {
                ctrl = false;
            }
        }
    }

    class Blind : RectangleForAnimation
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

    interface SupportOpacity
    {
        public void Shown();
        public void Hidden();
    }

    class TextboxForSetting : GroupObject, SupportOpacity
    {
        public TextboxForAnimation Title = new("cache/font.ttf", 0) { FontColor = new(0,0,0), OriginX = HorizontalPositionType.Left, DrawX = HorizontalPositionType.Right, OriginY = VerticalPositionType.Top, DrawY = VerticalPositionType.Bottom };

        public TextboxForAnimation Text = new("cache/font.ttf",0) { FontColor = new(0, 0, 0), OriginX = HorizontalPositionType.Left, DrawX = HorizontalPositionType.Right, OriginY = VerticalPositionType.Top, DrawY = VerticalPositionType.Bottom };

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

        public void Shown()
        {
            //Title.Hide = Text.Hide = false;
            Title.Opacity(255, 200f,100f);
            Text.Opacity(255, 200f,100f);
        }

        public void Hidden()
        {
            Title.Opacity(0, 200f,100f);
            Text.Opacity(0, 200f,100f);
            //Title.Hide = Text.Hide = true;
        }
    }

    class TitleForSetting : TextboxForAnimation, SupportOpacity
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

        public void Shown()
        {
            //this.Hide =  false;
            this.Opacity(255, 200f, 100f);
        }

        public void Hidden()
        {
            this.Opacity(0, 200f, 100f);
            //this.Hide = true;
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

    class HoverButton : GroupObject, SupportOpacity, Events.MouseMove, Events.MouseKeyDown
    {
        RectangleForAnimation rt = new() { Color = new(alpha: 0) };

        TextboxForAnimation ta = new("cache/font.ttf", 0) { FontColor = new(0,0,0)};

        int fontsize;

        public HoverButton(string text,int size = 24)
        {
            this.AddSprite(rt);
            this.AddSprite(ta);
            ta.Size =  fontsize = size;
            ta.Text = text;
            rt.OriginX = ta.OriginX = HorizontalPositionType.Left;
            rt.OriginY = ta.OriginY = VerticalPositionType.Top;
            rt.DrawX = ta.DrawX = HorizontalPositionType.Right;
            rt.DrawY = ta.DrawY = VerticalPositionType.Bottom;
        }

        public override void Start()
        {
            base.Start();
            rt.Opacity(0);
            rt.Width = ta.Width;
            rt.Height = ta.Height;
            Console.WriteLine(((GroupObject)rt.InheritedObject).InheritedScene.RenderRange == null);
        }

        public override void Resize()
        {
            base.Resize();
            ta.Size = (int)(fontsize * Window.AppropriateSize);
            rt.X = ta.X = (int)(5f * Window.AppropriateSize);
            rt.Y = ta.Y = (int)(5f * Window.AppropriateSize);
            rt.Width = ta.Width;
            rt.Height = ta.Height;
        }

        public void Shown()
        {
            ((RectangleForAnimation)rt).Opacity(0);
            ta.Opacity(255, 200f);
        }

        public void Hidden()
        {
            rt.Opacity(0,200f);
            ta.Opacity(0,200f);
        }

        bool hoverd = false;

        public void MouseMove()
        {
            bool hov = Convenience.MouseOver(rt);
            if (hoverd && !hov)
            {
                hoverd = false;
                ((RectangleForAnimation)rt).Opacity(0, 100f);
            } else if (!hoverd && hov)
            {
                hoverd = true;
                ((RectangleForAnimation)rt).Opacity(100, 100f);
            }
        }

        public void MouseKeyDown(Input.Mouse.Key e)
        {
            if (e != Input.Mouse.Key.Left) return;
            if (hoverd) ((SettingScene)this.InheritedObject).Hidden();
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
