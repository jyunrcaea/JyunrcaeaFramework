namespace JyunrcaeaFramework;

/// <summary>
/// 그려지지 않는 가짜 객체입니다.
/// </summary>
public class GhostBox : DrawableObject
{
    public GhostBox(int width=0,int height=0)
    {
        this.Size = new(width, height);
    }
    public Size2D Size;

    public override byte Opacity { get; set; } = 0;

    internal override int RealWidth => (int)(Size.Width * scale.X * (this.RelativeSize ? Window.AppropriateSize : 1));
    internal override int RealHeight => (int)(Size.Height * scale.Y * (this.RelativeSize ? Window.AppropriateSize : 1));
}