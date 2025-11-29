namespace JyunrcaeaFramework;

/// <summary>
/// 객체에 대한 자세한 정보를 얻어냅니다.
/// 객체는 렌더링 때 좌표나 크기 등 값들이 새로고침 되므로, 업데이트 도중에 변경사항이 있어도 그 사항이 적용된 값을 제공하지 않는다는점 주의해주세요.
/// </summary>
public static class DetailOfObject
{
    public interface Size
    {
        public int DisplayedWidth { get; }
        public int DisplayedHeight { get; }
    }

    public static int DrawWidth(DrawableObject obj)
    {
        return obj.RealWidth;
    }

    public static int DrawHeight(DrawableObject obj)
    {
        return obj.RealHeight;
    }

    /// <summary>
    /// 실제 렌더링 위치를 알아냅니다. (객체의 왼쪽 위 모서리의 좌표)
    /// </summary>
    /// <param name="obj">객체</param>
    /// <param name="x">X 좌표</param>
    /// <param name="y">Y 좌표</param>
    public static void RealPosition(DrawableObject obj,out int x,out int y)
    {
        x = obj.Rx;
        y = obj.Ry;
    }

    /// <summary>
    /// 하위 객체의 위치를 다시 맞춥니다.
    /// </summary>
    [Obsolete("정확하지 않음")]
    public static void ResetPosition(Group target)
    {
        if (target.Parent is not null)
        {
            Stack<Group> top = new();
            top.Push(target.Parent);
            Group? g;
            while ((g = top.First().Parent) is not TopGroup)
            {
                if (g is null) break;
                top.Push(g);
            }
            int x=0, y=0;
            while (top.Count != 0)
            {
                g = top.Pop();
                x += g.Rx;
                y += g.Ry;
            }
            Framework.DrawPos.x = x;
            Framework.DrawPos.y = y;
        }
        Framework.Positioning(target);
    }
}