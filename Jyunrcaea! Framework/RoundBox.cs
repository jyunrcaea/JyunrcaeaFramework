namespace JyunrcaeaFramework;

/// <summary>
/// 모서리가 둥근 직사각형을 그리는 객체입니다.
/// </summary>
public class RoundBox : Box
{
    public RoundBox(int Width = 0,int Height = 0,short Radius = 0,Color? color = null) : base(Width,Height,color)
    {
        this.Radius = Radius;
    }

    /// <summary>
    /// 모서리의 둥글기 정도 (픽셀 기준)
    /// </summary>
    public short Radius;
}