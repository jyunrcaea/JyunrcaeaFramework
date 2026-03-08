using SDL2;

namespace JyunrcaeaFramework.Structs;

public class RectSize
{
    internal SDL.SDL_Rect size;
    public int X { get => size.x; set => size.x = value; }
    public int Y { get => size.y; set => size.y = value; }
    public int Width { get => size.w; set => size.w = value; }
    public int Height { get => size.h; set => size.h = value; }
    public RectSize(int x = 0,int y = 0, int w = 0, int h = 0)
    {
        size = new() { x = x, y = y, w = w, h = h };
    }
}
