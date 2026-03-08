namespace JyunrcaeaFramework.Objects;

/// <summary>
/// 모든 객체의 기본이 되는 핵심 객체 
/// </summary>
public class BaseObject
{
    public bool hide = false;

    public int X { get; set; }
    public int Y { get; set; }
    internal virtual int Rx => X + Cx;
    internal virtual int Ry => Y + Cy;

    public Group? Parent { get; internal set; } = null;

    public T TypedParent<T>() where T : Group
    {
        if (this.Parent is null) throw new JyunrcaeaFrameworkException("이 객체가 포함된 그룹이 없습니다.");
        return (T)this.Parent;
    }

    internal int Cx = 0, Cy = 0;

    internal double CxD = 0.5, CyD = 0.5;

    //public bool CxIsChanged = false;
    //public bool CyIsChanged = false;

    public double CenterX
    {
        get => CxD;
        //{
        //    // 이미 값이 변경되었고, 부모가 null이 아닐경우 계산
        //    if (CxIsChanged && Parent is not null)
        //    {
        //        CxIsChanged = false;
        //        Cx = (int)(Parent.RealWidth * CxD);
        //    }
        //    return CxD;
        //}
        set
        {
            CxD = value;
            //if (CxIsChanged && Parent is not null) Cx = (int)(Parent.RealWidth * value);
        }
    }

    public double CenterY
    {
        get => CyD;
        set
        {
            CyD = value;
            //if (Parent is not null) Cy = (int)(Parent.RealWidth * value);
        }
    }


    internal bool MoveAnimation { get; set; } = false;

    internal virtual void Render(IntPtr renderer) { }

    internal virtual void UpdatePosition(int parentX, int parentY) { }
}
