using SDL2;

namespace JyunrcaeaFramework.Graphics;

/// <summary>
/// 메모리에 직접 그리기 작업을 수행하는 클래스입니다.
/// SDL2 라이브러리를 사용하여 픽셀 단위의 저수준 그래픽 작업을 처리합니다.
/// 메모리상의 서페이스에 점을 그리거나 픽셀 정보를 조회할 수 있으며,
/// 최종적으로 텍스처나 이미지로 변환하여 사용할 수 있습니다.
/// </summary>
public class PaintOnMemory : IDisposable
{
    IntPtr surface;
    SDL.SDL_Surface sur;
    SDL.SDL_PixelFormat format;

    internal IntPtr Address => surface;

    /// <summary>
    /// 지정된 크기의 ARGB8888 포맷 서페이스를 생성합니다.
    /// 투명도를 지원하는 32비트 컬러 포맷으로 초기화되며, 블렌드 모드가 활성화됩니다.
    /// </summary>
    /// <param name="width">생성할 서페이스의 너비 (기본값: 0)</param>
    /// <param name="height">생성할 서페이스의 높이 (기본값: 0)</param>
    public PaintOnMemory(int width = 0,int height = 0)
    {
        surface = SDL.SDL_CreateRGBSurfaceWithFormat(0, width, height, 32, SDL.SDL_PIXELFORMAT_ARGB8888);
        sur = System.Runtime.InteropServices.Marshal.PtrToStructure<SDL.SDL_Surface>(surface);
        format = System.Runtime.InteropServices.Marshal.PtrToStructure<SDL.SDL_PixelFormat>(sur.format);
        _ = SDL.SDL_SetSurfaceBlendMode(surface, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
    }

    /// <summary>
    /// 지정된 좌표에 Color 객체를 사용하여 픽셀을 그립니다.
    /// </summary>
    /// <param name="x">픽셀의 X 좌표</param>
    /// <param name="y">픽셀의 Y 좌표</param>
    /// <param name="color">칠할 색상 정보</param>
    public unsafe void Point(int x,int y,Color color)
    {
        Point(x, y, color.colorbase.r, color.colorbase.g, color.colorbase.b, color.colorbase.a);
    }

    /// <summary>
    /// 지정된 좌표에 RGBA 색상 값을 사용하여 픽셀을 그립니다.
    /// 서페이스를 잠금/해제하여 안전하게 픽셀 데이터에 접근합니다.
    /// </summary>
    /// <param name="x">픽셀의 X 좌표</param>
    /// <param name="y">픽셀의 Y 좌표</param>
    /// <param name="r">빨간색 성분 (0-255)</param>
    /// <param name="g">초록색 성분 (0-255)</param>
    /// <param name="b">파란색 성분 (0-255)</param>
    /// <param name="a">알파(투명도) 성분 (0-255)</param>
    public unsafe void Point(int x, int y, byte r,byte g,byte b,byte a)
    {
        _ = SDL.SDL_LockSurface(surface);
        byte* pixel_arr = (byte*)sur.pixels;
        pixel_arr[y * sur.pitch + x * format.BytesPerPixel + 0] = b;
        pixel_arr[y * sur.pitch + x * format.BytesPerPixel + 1] = g;
        pixel_arr[y * sur.pitch + x * format.BytesPerPixel + 2] = r;
        pixel_arr[y * sur.pitch + x * format.BytesPerPixel + 3] = a;
        SDL.SDL_UnlockSurface(surface);
    }

    /// <summary>
    /// 지정된 좌표의 픽셀 색상을 조회합니다.
    /// </summary>
    /// <param name="x">픽셀의 X 좌표</param>
    /// <param name="y">픽셀의 Y 좌표</param>
    /// <returns>해당 좌표의 색상 정보를 담은 Color 객체</returns>
    public unsafe Color GetPixel(int x,int y)
    {
        uint key = *(UInt32*)((byte*)sur.pixels + y * sur.pitch + x * format.BytesPerPixel);
        Color color = new();
        SDL.SDL_GetRGBA(key, sur.format, out color.colorbase.r, out color.colorbase.g, out color.colorbase.b, out color.colorbase.a);
        return color;
    }

    /// <summary>
    /// 메모리상의 서페이스를 기반으로 텍스처를 생성합니다.
    /// </summary>
    /// <returns>이 서페이스로부터 생성된 Texture 객체</returns>
    public Texture GetTexture()
    {
        return new(this.surface);
    }

    /// <summary>
    /// 사각형을 그립니다.
    /// </summary>
    /// <param name="x">사각형의 X 좌표</param>
    /// <param name="y">사각형의 Y 좌표</param>
    /// <param name="w">사각형의 너비</param>
    /// <param name="h">사각형의 높이</param>
    [Obsolete("(현재 구현되지 않음)")]
    public void Rectanlge(int x,int y,int w,int h)
    {

    }

    /// <summary>
    /// 메모리상의 서페이스를 기반으로 이미지를 생성합니다.
    /// 서페이스를 복제하여 새로운 ImageOnMemory 객체를 반환합니다.
    /// </summary>
    /// <returns>이 서페이스의 복제본을 기반으로 생성된 ImageOnMemory 객체</returns>
    public ImageOnMemory GetImage()
    {
        return new(SDL.SDL_DuplicateSurface(this.surface));
    }

    /// <summary>
    /// 할당된 서페이스 리소스를 해제합니다.
    /// 이 객체를 더 이상 사용하지 않을 때 호출해야 합니다.
    /// </summary>
    public void Dispose()
    {
        SDL.SDL_FreeSurface(surface);
        surface = IntPtr.Zero;
    }
}
