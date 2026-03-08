namespace JyunrcaeaFramework.Objects;

/// <summary>
/// 그리기도 가능하고 회전, 뒤집기 등이 가능한 확장된 객체
/// </summary>
public abstract class ExtendDrawableObject : DrawableObject
{
    /// <summary>
    /// 회전값
    /// </summary>
    public double Rotation { get; set; } = 0;

    public int AbsoluteWidth => this.RealWidth;
    public int AbsoluteHeight => this.RealHeight;
}
