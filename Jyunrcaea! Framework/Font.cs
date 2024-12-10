using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JyunrcaeaFramework;

public class Font : IDisposable
{
    /// <summary>
    /// 기본 글꼴파일의 경로를 설정합니다.
    /// Text 객체 생성시 글꼴파일 경로를 null로 설정할경우 이 경로가 채택됩니다.
    /// </summary>
    public static string DefaultPath { get; set; } = string.Empty;

    internal IntPtr pointer = IntPtr.Zero;

    internal int sz = 1;

    /// <summary>
    /// 글꼴의 크기입니다.
    /// </summary>
    public int Size {
        get => sz; set {
            if (SDL_ttf.TTF_SetFontSize(this.pointer , this.sz = value) == -1)
                throw new JyunrcaeaFrameworkException($"폰트 로드에 실패했습니다. SDL_TTF Error: {SDL_ttf.TTF_GetError()}");
        }
    }

    /// <summary>
    /// 글꼴을 불러옵니다.
    /// </summary>
    /// <param name="filename">글꼴 파일 경로</param>
    /// <param name="size">글자 크기 (높이 기준)</param>
    public Font(string filename , int size)
    {
        this.pointer = SDL_ttf.TTF_OpenFont(filename , this.sz = size);
        if (pointer == IntPtr.Zero)
            throw new JyunrcaeaFrameworkException($"불러올수 없는 글꼴 파일 SDL Error: {SDL.SDL_GetError()}");
    }

    /// <summary>
    /// 불러온 글꼴을 메모리에서 해제합니다.
    /// </summary>
    public void Dispose()
    {
        SDL_ttf.TTF_CloseFont(this.pointer);
        GC.SuppressFinalize(this);
    }
    ~Font()
    {
        Dispose();
    }
}