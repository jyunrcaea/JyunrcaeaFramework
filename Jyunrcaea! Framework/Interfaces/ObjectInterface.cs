namespace JyunrcaeaFramework.Interfaces;

/// <summary>
/// 객체의 기본이 되는 객체 인터페이스입니다. (사실 추상 클래스이긴 하지만...)
/// </summary>
public abstract class ObjectInterface
{
    public abstract void Resize();
    public abstract void Stop();
    internal abstract void Draw();
    public abstract void Start();
    public bool Hide = false;

    //public abstract int X { get; set; }
    //public abstract int Y { get; set; }
}
