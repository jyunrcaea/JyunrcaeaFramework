using JyunrcaeaFramework.Core;
using SDL2;

namespace JyunrcaeaFramework.Graphics;

/// <summary>
/// 글꼴 리소스입니다.
/// </summary>
public sealed class Font : IDisposable
{
    /// <summary>
    /// 기본 글꼴 경로입니다. 명시적인 글꼴 지정 없이 텍스트 객체를 생성하는 헬퍼에서 사용됩니다.
    /// </summary>
    public static string DefaultPath { get; set; } = string.Empty;

    internal IntPtr pointer = IntPtr.Zero;

    int sz = 1;

    /// <summary>
    /// 현재 기본으로 적용된 글자 크기입니다.
    /// </summary>
    public int Size
    {
        get => sz;
        set
        {
            if (pointer == IntPtr.Zero)
            {
                throw new ObjectDisposedException(nameof(Font));
            }

            if (SDL_ttf.TTF_SetFontSize(pointer, sz = value) == -1)
            {
                throw new JyunrcaeaFrameworkException($"SDL_ttf Error: {SDL_ttf.TTF_GetError()}");
            }
        }
    }

    /// <summary>
    /// 객체를 생성하는 즉시 주어진 인자대로 글꼴을 로드합니다.
    /// </summary>
    /// <remarks>지원하는 폰트는 ttf, oft, woff 등이 있습니다. 지정된 파일이 존재하고 접근 가능한지 확인하세요.
    /// 파일이 유효하지 않거나 지원되지 않는 형식이면 생성자가 실패합니다.</remarks>
    /// <param name="filename">불러올 글꼴 파일의 경로입니다.</param>
    /// <param name="size">불러올 글꼴의 크기입니다. 자연수여야  합니다.</param>
    /// <exception cref="JyunrcaeaFrameworkException">SDL_ttf에서 글꼴을 열거나 로드할 수 없을 때 발생합니다.</exception>
    public Font(string filename, int size)
    {
        pointer = SDL_ttf.TTF_OpenFont(filename, sz = size);
        if (pointer == IntPtr.Zero)
        {
            throw new JyunrcaeaFrameworkException($"Failed to open font. SDL_ttf Error: {SDL_ttf.TTF_GetError()}");
        }
    }

    /// <summary>
    /// 점유중인 글꼴을 내려놓습니다.
    /// </summary>
    public void Dispose()
    {
        if (pointer != IntPtr.Zero)
        {
            SDL_ttf.TTF_CloseFont(pointer);
            pointer = IntPtr.Zero;
        }

        GC.SuppressFinalize(this);
    }

    ~Font()
    {
        Dispose();
    }
}
