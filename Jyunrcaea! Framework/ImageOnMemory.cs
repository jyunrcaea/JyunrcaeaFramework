using SDL2;

namespace JyunrcaeaFramework;

public class ImageOnMemory : IDisposable
{
    SDL.SDL_Surface surface;
    IntPtr surface_ptr;

    public ImageOnMemory(IntPtr address)
    {
        Init(address);
    }

    private void Init(IntPtr address)
    {
        surface_ptr = address;
        if (surface_ptr == IntPtr.Zero) throw new JyunrcaeaFrameworkException("이미지를 불러오는데 실패하였습니다.");
        surface = System.Runtime.InteropServices.Marshal.PtrToStructure<SDL.SDL_Surface>(surface_ptr);
        format = System.Runtime.InteropServices.Marshal.PtrToStructure<SDL.SDL_PixelFormat>(surface.format);
        bpp = format.BytesPerPixel;
        unsafe
        {
            switch (bpp)
            {
                case 1:
                    pp = Process.One;
                    break;
                case 2:
                    pp = Process.Two;
                    break;
                case 3:
                    pp = Process.Three;
                    break;
                case 4:
                    pp = Process.Four;
                    break;
                default:
                    pp = Process.Default;
                    break;
            }
        }
    }

    private void Init(ImageOnMemory me)
    {
        this.surface = me.surface;
        this.surface_ptr = me.surface_ptr;
        this.pp = me.pp;
        this.format = me.format;
        this.bpp = me.bpp;
    }

    public ImageOnMemory(string path) : this(SDL_image.IMG_Load(path))
    {

    }

    public int Width => surface.w; public int Height => surface.h;

    Process.PixelProcesser pp = null!;

    SDL.SDL_PixelFormat format;
    int bpp;

    public unsafe UInt32 GetPixel(int x,int y)
    {
        return pp((byte*)surface.pixels + y * surface.pitch + x * bpp);
    }

    public void GetRGBA(int x,int y,out byte r,out byte g,out byte b,out byte a)
    {
        uint p = GetPixel(x, y);
        SDL.SDL_GetRGBA(p, surface.format, out r, out g, out b, out a);
    }

    public ImageOnMemory GetResizedImage(int width,int height)
    {
        IntPtr result = SDL.SDL_CreateRGBSurfaceWithFormat(0, width, height, 32, SDL.SDL_PIXELFORMAT_ARGB8888);
        SDL.SDL_Rect origin = new() { x = 0, y = 0, w = surface.w, h = surface.h };
        SDL.SDL_Rect targetsize = new() { x = 0, y = 0, w = width, h = height };
        SDL.SDL_LowerBlitScaled(surface_ptr, ref origin, result,ref targetsize);
        if (result == IntPtr.Zero) throw new JyunrcaeaFrameworkException("크기 조정에 실패하였습니다.");
        return new(result);
    }

    public int BlurProgress { get; internal set; } = -1;

    public void Resize(int width,int height)
    {
        var result = GetResizedImage(width, height);
        this.Release();
        this.Init(result);
    }

    public void Blur()
    {
        var result = EffectForImage.Bluring(this);
        this.Release();
        this.Init(result.GetImage());
        result.Dispose();
    }

    public Texture GetTexture()
    {
        if (surface_ptr == IntPtr.Zero) throw new JyunrcaeaFrameworkException("실패. 불러오지 않은 이미지, 또는 이미 해제된 이미지를 텍스쳐로 변환할려고 했습니다.");
        return new Texture(surface_ptr);
    }

    public void Dispose()
    {
        Release();
    }

    private void Release()
    {
        if (surface_ptr == IntPtr.Zero) return;
        SDL.SDL_FreeSurface(surface_ptr);
        surface_ptr = IntPtr.Zero;
    }

    private static unsafe class Process {
        internal delegate UInt32 PixelProcesser(byte* p);

        internal static UInt32 One(byte* p)
        {
            return *p;
        }

        internal static UInt32 Two(byte* p)
        {
            return *(UInt16*)p;
        }

        internal static UInt32 Three(byte* p)
        {
            return (UInt32)(p[0] | p[1] << 8 | p[2] << 16);
        }

        internal static UInt32 Four(byte* p)
        {
            return *(UInt32*)p;
        }

        internal unsafe static UInt32 Default(byte* p)
        {
            throw new JyunrcaeaFrameworkException("can't read pixel");
        }
    }
}