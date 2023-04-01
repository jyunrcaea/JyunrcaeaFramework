using JyunrcaeaFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Jyunrcaea.TitleBar
{
    public class Border : Scene
    {
        public Border()
        {
            this.AddSprite(new TitleBar());
            this.AddSprite(new ExitButton());
        }
    }

    class TitleBar : RectangleForAnimation, MouseMoveEventInterface, MouseButtonDownEventInterface
    {
        public TitleBar() : base(400, 30)
        {
            this.Opacity(0);
            this.OriginY = VerticalPositionType.Top;
            this.DrawY = VerticalPositionType.Bottom;
            this.X = -10;
            this.Y = 10;
            this.Radius = 15;
        }

        bool hovered = false;

        public void MouseMove()
        {
            if (Convenience.MouseOver(this))
            {
                if (!hovered) { this.hovered = true; this.Opacity(128, 200f); }
            }
            else
            {
                if (hovered) { this.hovered = false; this.Opacity(0, 200f); }
            }

        }

        public void MouseButtonDown(Input.Mouse.Key key)
        {
            if (key != Input.Mouse.Key.Left) return;
            if (Convenience.MouseOver(this)) Framework.Stop();
        }
    }

    class ExitButton : RectangleForAnimation, MouseMoveEventInterface, MouseButtonDownEventInterface
    {
        public ExitButton() : base(30,30)
        {
            this.Color = new(200, 120, 120, 255);
            this.Opacity(0);

            this.OriginX = HorizontalPositionType.Right;
            this.DrawX = HorizontalPositionType.Left;
            this.OriginY = VerticalPositionType.Top;
            this.DrawY = VerticalPositionType.Bottom;
            this.X = -10;
            this.Y = 10;
            this.Radius = 10;
        }

        bool hovered = false;

        public void MouseMove()
        {
            if (Convenience.MouseOver(this))
            {
                if (!hovered) { this.hovered = true; this.Opacity(128, 200f); }
            } else
            {
                if (hovered) { this.hovered = false; this.Opacity(0, 200f); }
            }

        }

        public void MouseButtonDown(Input.Mouse.Key key)
        {
            if (key != Input.Mouse.Key.Left) return;
            if (Convenience.MouseOver(this)) Framework.Stop();
        }
    }
}
