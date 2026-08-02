using System;
using SDL3;

namespace JyunrcaeaFramework
{
    public static class GFX
    {
        public static int roundedBoxRGBA(IntPtr renderer, float x1, float y1, float x2, float y2, float rad, byte r, byte g, byte b, byte a)
        {
            SDL.SetRenderDrawColor(renderer, r, g, b, a);
            var rect = new SDL.FRect { X = x1, Y = y1, W = x2 - x1, H = y2 - y1 };
            SDL.RenderFillRect(renderer, ref rect);
            return 0;
        }

        public static int roundedRectangleRGBA(IntPtr renderer, float x1, float y1, float x2, float y2, float rad, byte r, byte g, byte b, byte a)
        {
            SDL.SetRenderDrawColor(renderer, r, g, b, a);
            var rect = new SDL.FRect { X = x1, Y = y1, W = x2 - x1, H = y2 - y1 };
            SDL.RenderRect(renderer, ref rect);
            return 0;
        }

        public static int filledCircleRGBA(IntPtr renderer, float x, float y, float rad, byte r, byte g, byte b, byte a)
        {
            SDL.SetRenderDrawColor(renderer, r, g, b, a);
            var rect = new SDL.FRect { X = x - rad, Y = y - rad, W = rad * 2, H = rad * 2 };
            SDL.RenderFillRect(renderer, ref rect);
            return 0;
        }

        public static int circleRGBA(IntPtr renderer, float x, float y, float rad, byte r, byte g, byte b, byte a)
        {
            SDL.SetRenderDrawColor(renderer, r, g, b, a);
            var rect = new SDL.FRect { X = x - rad, Y = y - rad, W = rad * 2, H = rad * 2 };
            SDL.RenderRect(renderer, ref rect);
            return 0;
        }

        public static int filledTrigonRGBA(IntPtr renderer, float x1, float y1, float x2, float y2, float x3, float y3, byte r, byte g, byte b, byte a)
        {
            return 0;
        }
    }
}
