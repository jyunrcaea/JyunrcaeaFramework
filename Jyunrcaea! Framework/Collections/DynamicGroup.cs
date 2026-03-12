using JyunrcaeaFramework.Interfaces;
using JyunrcaeaFramework.Objects;
using SDL2;

namespace JyunrcaeaFramework.Collections;

/// <summary>
/// 자신의 내용을 기준으로 동적으로 표시 범위를 계산하는 그룹입니다.
/// </summary>
public class DynamicGroup : Group, DetailOfObject.Size
{
    public int DisplayedWidth => contentrange.w;
    public int DisplayedHeight => contentrange.h;

    internal SDL.SDL_Rect contentrange = new();

    public override void Update(float ms)
    {
        base.Update(ms);

        if (this.Objects.Count == 0)
        {
            contentrange.x = -1;
            contentrange.y = -1;
            contentrange.w = 0;
            contentrange.h = 0;
            return;
        }

        bool found = false;
        int left = 0;
        int right = 0;
        int top = 0;
        int bottom = 0;

        bool TryGetBounds(BaseObject target, out int l, out int t, out int r, out int b)
        {
            if (target is DrawableObject drawable)
            {
                l = drawable.renderPosition.x;
                t = drawable.renderPosition.y;
                r = l + drawable.renderPosition.w;
                b = t + drawable.renderPosition.h;
                return true;
            }

            if (target is DynamicGroup group && group.contentrange.x >= 0 && group.contentrange.y >= 0)
            {
                l = group.contentrange.x;
                t = group.contentrange.y;
                r = l + group.contentrange.w;
                b = t + group.contentrange.h;
                return true;
            }

            l = t = r = b = 0;
            return false;
        }

        for (int i = 0; i < this.Objects.Count; i++)
        {
            if (!TryGetBounds(this.Objects[i], out left, out top, out right, out bottom)) continue;

            found = true;
            for (i = i + 1; i < this.Objects.Count; i++)
            {
                if (!TryGetBounds(this.Objects[i], out int l, out int t, out int r, out int b)) continue;

                if (l < left) left = l;
                if (t < top) top = t;
                if (r > right) right = r;
                if (b > bottom) bottom = b;
            }
            break;
        }

        if (!found)
        {
            contentrange.x = -1;
            contentrange.y = -1;
            contentrange.w = 0;
            contentrange.h = 0;
            return;
        }

        this.contentrange.x = left;
        this.contentrange.y = top;
        this.contentrange.w = right - left;
        this.contentrange.h = bottom - top;
    }
}
