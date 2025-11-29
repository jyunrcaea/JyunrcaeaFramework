using SDL2;

namespace JyunrcaeaFramework;

/// <summary>
/// 수학 공식을 까먹은 당신을 위해... 편리한 기능을 제공하는 함수들이 모여있습니다.
/// </summary>
public static class Convenience
{
    /// <summary>
    /// 두 객체가 서로 겹치는 부분이 있는지 (닿았는지) 판단합니다. (직사각형 기준)
    /// </summary>
    /// <param name="sp1">첫번째 객체</param>
    /// <param name="sp2">두번째 객체</param>
    /// <returns>겹친 부분이 있을경우 True 를 반환합니다.</returns>
    public static bool Overlap(DrawableObject sp1,DrawableObject sp2)
    {
        return SDL.SDL_IntersectRect(ref sp1.renderPosition, ref sp2.renderPosition , out _) == SDL.SDL_bool.SDL_TRUE;
    }

    public static bool MouseOver(this DrawableObject target)
    {
        if (target is Circle)
        {
            return Math.Sqrt(Math.Pow((target.Rx - Input.Mouse.X), 2) + Math.Pow((target.Ry - Input.Mouse.Y), 2)) <= ((Circle)target).Radius;
        }
        return SDL.SDL_PointInRect(ref Input.Mouse.position, ref target.renderPosition) == SDL.SDL_bool.SDL_TRUE;
    }

    /// <summary>
    /// 두 객체의 거리를 구합니다.
    /// </summary>
    /// <param name="sp1">첫번째 객체</param>
    /// <param name="sp2">두번째 객체</param>
    /// <returns>거리</returns>
    public static double Distance(DrawableObject sp1, DrawableObject sp2)
    {
        int x = sp1.renderPosition.x - sp2.renderPosition.x, y = sp1.renderPosition.y - sp2.renderPosition.y;
        return Math.Sqrt(x * x + y * y);
    }
}