using SDL2;

namespace JyunrcaeaFramework;

public class PaintOnMemory : IDisposable
{
    IntPtr surface;
    SDL.SDL_Surface sur;
    SDL.SDL_PixelFormat format;

    internal IntPtr Address => surface;

    public PaintOnMemory(int width = 0,int height = 0)
    {
        surface = SDL.SDL_CreateRGBSurfaceWithFormat(0, width, height, 32, SDL.SDL_PIXELFORMAT_ARGB8888);
        sur = System.Runtime.InteropServices.Marshal.PtrToStructure<SDL.SDL_Surface>(surface);
        format = System.Runtime.InteropServices.Marshal.PtrToStructure<SDL.SDL_PixelFormat>(sur.format);
        SDL.SDL_SetSurfaceBlendMode(surface, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
    }

    public unsafe void Point(int x,int y,Color color)
    {
        Point(x, y, color.colorbase.r, color.colorbase.g, color.colorbase.b, color.colorbase.a);
    }

    public unsafe void Point(int x, int y, byte r,byte g,byte b,byte a)
    {
        SDL.SDL_LockSurface(surface);
        byte* pixel_arr = (byte*)sur.pixels;
        pixel_arr[y * sur.pitch + x * format.BytesPerPixel + 0] = b;
        pixel_arr[y * sur.pitch + x * format.BytesPerPixel + 1] = g;
        pixel_arr[y * sur.pitch + x * format.BytesPerPixel + 2] = r;
        pixel_arr[y * sur.pitch + x * format.BytesPerPixel + 3] = a;
        SDL.SDL_UnlockSurface(surface);
    }

    public unsafe Color GetPixel(int x,int y)
    {
        uint key = *(UInt32*)((byte*)sur.pixels + y * sur.pitch + x * format.BytesPerPixel);
        Color color = new();
        SDL.SDL_GetRGBA(key, sur.format, out color.colorbase.r, out color.colorbase.g, out color.colorbase.b, out color.colorbase.a);
        return color;
    }

    public Texture GetTexture()
    {
        return new(this.surface);
    }

    public void Rectanlge(int x,int y,int w,int h)
    {
        
    }

    public ImageOnMemory GetImage()
    {
        return new(SDL.SDL_DuplicateSurface(this.surface));
    }

    public void Dispose()
    {
        SDL.SDL_FreeSurface(surface);
        surface = IntPtr.Zero;
    }
}