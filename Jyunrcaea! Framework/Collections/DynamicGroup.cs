using SDL2;

namespace JyunrcaeaFramework.Collections;

/// <summary>
/// ??? ???? ?? ??? ???? ?? ??? ?????.
/// </summary>
public class DynamicGroup : Group, DetailOfObject.Size
{
    public int DisplayedWidth => contentrange.w;
    public int DisplayedHeight => contentrange.h;

    internal SDL.SDL_Rect contentrange = new();

    public override void Update(float ms)
    {
        base.Update(ms);
        int i = 0;
        while (this.Objects[i] is not DrawableObject || this.Objects[i] is not DynamicGroup)
        {
            // ??? ?? ??? ????
            if (i++ >= this.Objects.Count)
            {
                contentrange.x = -1;
                contentrange.y = -1;
                contentrange.w = 0;
                contentrange.h = 0;
                return;
            }
        }
        int left = ((DrawableObject)this.Objects[i]).renderPosition.x, right = left + ((DrawableObject)this.Objects[i]).renderPosition.w;
        int top = ((DrawableObject)this.Objects[i]).renderPosition.y, bottom = top + ((DrawableObject)this.Objects[i]).renderPosition.h;
        int ww, hh;
        for (;i<this.Objects.Count;i++)
        {
            if (this.Objects[i] is DrawableObject)
            {
                // ??
                ww = ((DrawableObject)this.Objects[i]).renderPosition.x;
                if (ww < left) left = ww;
                // ???
                ww += ((DrawableObject)this.Objects[i]).renderPosition.w;
                if (ww > right) right = ww;
                //?
                hh = ((DrawableObject)this.Objects[i]).renderPosition.y;
                if (hh < top) top = hh;
                //??
                hh += ((DrawableObject)this.Objects[i]).renderPosition.h;
                if (hh > bottom) bottom = hh;
            }
            if (this.Objects[i] is DynamicGroup)
            {
                // ??
                ww = ((DynamicGroup)this.Objects[i]).contentrange.x;
                if (ww < left) left = ww;
                // ???
                ww += ((DynamicGroup)this.Objects[i]).contentrange.w;
                if (ww > right) right = ww;
                //?
                hh = ((DynamicGroup)this.Objects[i]).contentrange.y;
                if (hh < top) top = hh;
                //??
                hh += ((DynamicGroup)this.Objects[i]).contentrange.h;
                if (hh > bottom) bottom = hh;
            }
        }

        this.contentrange.x = left;
        this.contentrange.y = top;
        this.contentrange.w = right - left;
        this.contentrange.h = bottom - top;
    }


}
