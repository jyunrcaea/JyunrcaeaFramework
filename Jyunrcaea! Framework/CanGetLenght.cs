namespace JyunrcaeaFramework;

/// <summary>
/// 너비와 높이를 얻을수 있는 객체에게만 상속되는 인터페이스입니다.
/// </summary>
public interface CanGetLenght
{
    public int Width { get; }
    public int Height { get; }
}