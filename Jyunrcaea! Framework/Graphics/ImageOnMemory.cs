using JyunrcaeaFramework.Core;
using SDL2;

namespace JyunrcaeaFramework.Graphics;

/// <summary>
/// SDL_Surface를 메모리에서 관리하고 픽셀 조작, 크기 조정, 이미지 효과 등을 제공하는 클래스입니다.
/// </summary>
public class ImageOnMemory : IDisposable
{
    SDL.SDL_Surface surface;
    IntPtr surface_ptr;

    /// <summary>
    /// SDL_Surface의 주소를 통해 ImageOnMemory 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="address">로드된 이미지의 SDL_Surface 주소입니다.</param>
    /// <exception cref="JyunrcaeaFrameworkException">주소가 유효하지 않으면 예외를 발생시킵니다.</exception>
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
            pp = bpp switch
            {
                1 => Process.One,
                2 => Process.Two,
                3 => Process.Three,
                4 => Process.Four,
                _ => Process.Default,
            };
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

    /// <summary>
    /// 파일 경로로부터 이미지를 로드하여 ImageOnMemory 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="path">로드할 이미지 파일의 경로입니다.</param>
    public ImageOnMemory(string path) : this(SDL_image.IMG_Load(path))
    {

    }

    /// <summary>
    /// 이미지의 가로 크기를 나타냅니다.
    /// </summary>
    public int Width => surface.w;

    /// <summary>
    /// 이미지의 세로 크기를 나타냅니다.
    /// </summary>
    public int Height => surface.h;

    Process.PixelProcesser pp = null!;

    SDL.SDL_PixelFormat format;
    int bpp;

    /// <summary>
    /// 지정된 좌표의 픽셀 값을 반환합니다.
    /// </summary>
    /// <param name="x">픽셀의 x 좌표입니다.</param>
    /// <param name="y">픽셀의 y 좌표입니다.</param>
    /// <returns>픽셀의 값을 UInt32로 반환합니다.</returns>
    public unsafe UInt32 GetPixel(int x,int y)
    {
        return pp((byte*)surface.pixels + y * surface.pitch + x * bpp);
    }

    /// <summary>
    /// 지정된 좌표의 픽셀을 RGBA 값으로 분해하여 반환합니다.
    /// </summary>
    /// <param name="x">픽셀의 x 좌표입니다.</param>
    /// <param name="y">픽셀의 y 좌표입니다.</param>
    /// <param name="r">빨간색(Red) 값이 전달됩니다.</param>
    /// <param name="g">초록색(Green) 값이 전달됩니다.</param>
    /// <param name="b">파란색(Blue) 값이 전달됩니다.</param>
    /// <param name="a">알파(Alpha/투명도) 값이 전달됩니다.</param>
    public void GetRGBA(int x,int y,out byte r,out byte g,out byte b,out byte a)
    {
        uint p = GetPixel(x, y);
        SDL.SDL_GetRGBA(p, surface.format, out r, out g, out b, out a);
    }

    /// <summary>
    /// 지정된 크기로 조정된 새로운 이미지를 생성하여 반환합니다. 현재 이미지는 변경되지 않습니다.
    /// </summary>
    /// <param name="width">새 이미지의 가로 크기입니다.</param>
    /// <param name="height">새 이미지의 세로 크기입니다.</param>
    /// <returns>크기가 조정된 새로운 ImageOnMemory 인스턴스를 반환합니다.</returns>
    /// <exception cref="JyunrcaeaFrameworkException">크기 조정에 실패하면 예외를 발생시킵니다.</exception>
    public ImageOnMemory GetResizedImage(int width,int height)
    {
        IntPtr result = SDL.SDL_CreateRGBSurfaceWithFormat(0, width, height, 32, SDL.SDL_PIXELFORMAT_ARGB8888);
        SDL.SDL_Rect origin = new() { x = 0, y = 0, w = surface.w, h = surface.h };
        SDL.SDL_Rect targetsize = new() { x = 0, y = 0, w = width, h = height };
        SDL.SDL_LowerBlitScaled(surface_ptr, ref origin, result,ref targetsize);
        if (result == IntPtr.Zero) throw new JyunrcaeaFrameworkException("크기 조정에 실패하였습니다.");
        return new(result);
    }

    /// <summary>
    /// 블러 효과의 진행 상태를 나타냅니다. -1은 미설정 상태입니다.
    /// </summary>
    public int BlurProgress { get; internal set; } = -1;

    /// <summary>
    /// 현재 이미지의 크기를 지정된 값으로 조정합니다. 기존 이미지는 해제되고 새로운 크기로 변경됩니다.
    /// </summary>
    /// <param name="width">변경할 가로 크기입니다.</param>
    /// <param name="height">변경할 세로 크기입니다.</param>
    public void Resize(int width,int height)
    {
        var result = GetResizedImage(width, height);
        this.Release();
        this.Init(result);
    }

    /// <summary>
    /// 현재 이미지에 블러 효과를 적용합니다. 기존 이미지는 해제되고 블러가 적용된 이미지로 변경됩니다.
    /// </summary>
    public void Blur()
    {
        var result = EffectForImage.Bluring(this);
        this.Release();
        this.Init(result.GetImage());
        result.Dispose();
    }

    /// <summary>
    /// 현재 이미지를 렌더링 가능한 Texture 객체로 변환하여 반환합니다.
    /// </summary>
    /// <returns>현재 이미지를 기반으로 한 새로운 Texture 인스턴스를 반환합니다.</returns>
    /// <exception cref="JyunrcaeaFrameworkException">이미지가 로드되지 않았거나 이미 해제되었으면 예외를 발생시킵니다.</exception>
    public Texture GetTexture()
    {
        if (surface_ptr == IntPtr.Zero) throw new JyunrcaeaFrameworkException("실패. 불러오지 않은 이미지, 또는 이미 해제된 이미지를 텍스쳐로 변환할려고 했습니다.");
        return new Texture(surface_ptr);
    }

    /// <summary>
    /// 이미지 리소스를 해제합니다. IDisposable 인터페이스의 구현입니다.
    /// </summary>
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

    /// <summary>
    /// 픽셀 포맷에 따라 픽셀 데이터를 처리하는 내부 클래스입니다.
    /// </summary>
    private static unsafe class Process {
        /// <summary>
        /// 바이트 포인터를 받아 픽셀 값을 UInt32로 반환하는 대리자입니다.
        /// </summary>
        /// <param name="p">픽셀 데이터의 메모리 주소입니다.</param>
        /// <returns>읽은 픽셀 값을 UInt32로 반환합니다.</returns>
        internal delegate UInt32 PixelProcesser(byte* p);

        /// <summary>
        /// 1바이트(8비트) 픽셀 포맷의 데이터를 처리합니다.
        /// </summary>
        internal static UInt32 One(byte* p)
        {
            return *p;
        }

        /// <summary>
        /// 2바이트(16비트) 픽셀 포맷의 데이터를 처리합니다.
        /// </summary>
        internal static UInt32 Two(byte* p)
        {
            return *(UInt16*)p;
        }

        /// <summary>
        /// 3바이트(24비트) 픽셀 포맷의 데이터를 처리합니다.
        /// </summary>
        internal static UInt32 Three(byte* p)
        {
            return (UInt32)(p[0] | p[1] << 8 | p[2] << 16);
        }

        /// <summary>
        /// 4바이트(32비트) 픽셀 포맷의 데이터를 처리합니다.
        /// </summary>
        internal static UInt32 Four(byte* p)
        {
            return *(UInt32*)p;
        }

        /// <summary>
        /// 지원하지 않는 픽셀 포맷인 경우 예외를 발생시킵니다.
        /// </summary>
        internal unsafe static UInt32 Default(byte* p)
        {
            throw new JyunrcaeaFrameworkException("can't read pixel");
        }
    }
}
