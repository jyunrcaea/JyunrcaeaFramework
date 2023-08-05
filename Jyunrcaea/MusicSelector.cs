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
            this.Hide = true;

            this.Objects.Add(new Close());
        }

        public override void Prepare()
        {
            base.Prepare();
            Control.Scene = this;
        }
    }

    public class Data
    {
        public static TextureFromFile Texture = null!;
    }



    public class Close : Text, Events.MouseKeyDown, Events.MouseKeyUp
    {
        public Close() : base(" ×",26)
        {
            this.CenterX = 0;
            this.CenterY = 0;
            this.DrawX = HorizontalPositionType.Right;
            this.DrawY = VerticalPositionType.Bottom;
        }

        bool hovered = false;

        public void MouseKeyDown(Input.Mouse.Key k)
        {
            if (k != Input.Mouse.Key.Left) return;
            if (hovered = Convenience.MouseOver(this))
            {
                this.TextColor = Color.Gray;
            }
        }

        public void MouseKeyUp(Input.Mouse.Key k)
        {
            if (k != Input.Mouse.Key.Left) return;
            if (hovered)
            {
                Control.Show = false;
                this.TextColor = Color.Black;
            }
        } 
    }
}
