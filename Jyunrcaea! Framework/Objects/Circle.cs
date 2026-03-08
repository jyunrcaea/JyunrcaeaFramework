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

    internal override int RealWidth => (int)(Radius * 2 * (RelativeSize ? Window.AppropriateSize : 1));
    internal override int RealHeight => (int)(Radius * 2 * (RelativeSize ? Window.AppropriateSize : 1));

    internal override void Render(IntPtr renderer)
    {
        SDL_gfx.filledCircleRGBA(renderer, (short)this.renderPosition.x, (short)this.renderPosition.y, this.Radius, this.Color.colorbase.r, this.Color.colorbase.g, this.Color.colorbase.b, this.Color.colorbase.a);
    }
}
