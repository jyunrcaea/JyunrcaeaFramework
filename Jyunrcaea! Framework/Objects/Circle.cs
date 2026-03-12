using JyunrcaeaFramework.Core;
using JyunrcaeaFramework.EventSystem;
using JyunrcaeaFramework.Graphics;
using SDL2;

namespace JyunrcaeaFramework.Objects;

/// <summary>
/// 원을 그리는 객체입니다.
/// </summary>
public class Circle : DrawableObject, Animation.Available.Opacity
{
    public Circle(short radius = 0, Color? color = null)
    {
        this.Radius = radius;
        this.Color = color is null ? Color.White : color;
    }

    public short Radius;

    public Color Color;

    public override byte Opacity { get => Color.Alpha; set => Color.Alpha = value; }

    internal override int RealWidth => (int)(Radius * 2 * scale.X * (RelativeSize ? Window.AppropriateSize : 1));
    internal override int RealHeight => (int)(Radius * 2 * scale.Y * (RelativeSize ? Window.AppropriateSize : 1));

    internal override void Render(IntPtr renderer)
    {
        short drawRadius = (short)Math.Max(0, Math.Min(this.renderPosition.w, this.renderPosition.h) / 2);
        short centerX = (short)(this.renderPosition.x + this.renderPosition.w / 2);
        short centerY = (short)(this.renderPosition.y + this.renderPosition.h / 2);
        _ = SDL_gfx.filledCircleRGBA(renderer, centerX, centerY, drawRadius, this.Color.colorbase.r, this.Color.colorbase.g, this.Color.colorbase.b, this.Color.colorbase.a);
    }
}
