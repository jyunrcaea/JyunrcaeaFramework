namespace JyunrcaeaFramework;

/// <summary>
/// 배율 조정 구조체입니다.
/// </summary>
public struct Scale2D
{
    public double X, Y;
    public Scale2D(double x,double y)
    {
        X = x;
        Y = y;
    }
    public Scale2D(double xy)
    {
        X = Y = xy;
    }
    public Scale2D()
    {
        X = 1d;
        Y = 1d;
    }
}