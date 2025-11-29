namespace JyunrcaeaFramework;

/// <summary>
/// 직사각형을 그리는 객체입니다.
/// </summary>
public class Box : DrawableObject, Animation.Available.Opacity, Animation.Available.Size
{
    public Box(int width = 0,int height = 0,Color? color = null)
    {
        this.Size = new(width, height);
        this.Color = color is null ? Color.White : color;
    }

    /// <summary>
    /// 직사각형의 너비와 높이
    /// </summary>
    public Size2D Size { get; set; }

    /// <summary>
    /// 출력할 색상
    /// </summary>
    public Color Color;

    public override byte Opacity { get => Color.Alpha; set => Color.Alpha = value; }

    internal override int RealWidth =>  (int)(Size.Width * scale.X * (this.RelativeSize ? Window.AppropriateSize : 1));
    internal override int RealHeight => (int)(Size.Height * scale.Y * (this.RelativeSize ? Window.AppropriateSize : 1));
}