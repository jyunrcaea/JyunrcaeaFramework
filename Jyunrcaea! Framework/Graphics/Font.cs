using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JyunrcaeaFramework.Graphics;

public class Font : IDisposable
{
    /// <summary>
    /// ?? ????? ??? ?????.
    /// Text ?? ??? ???? ??? null? ????? ? ??? ?????.
    /// </summary>
    public static string DefaultPath { get; set; } = string.Empty;

    internal IntPtr pointer = IntPtr.Zero;

    internal int sz = 1;

    /// <summary>
    /// ??? ?????.
    /// </summary>
    public int Size {
        get => sz; set {
            if (SDL_ttf.TTF_SetFontSize(this.pointer , this.sz = value) == -1)
                throw new JyunrcaeaFrameworkException($"?? ??? ??????. SDL_TTF Error: {SDL_ttf.TTF_GetError()}");
        }
    }

    /// <summary>
    /// ??? ?????.
    /// </summary>
    /// <param name="filename">?? ?? ??</param>
    /// <param name="size">?? ?? (?? ??)</param>
    public Font(string filename , int size)
    {
        this.pointer = SDL_ttf.TTF_OpenFont(filename , this.sz = size);
        if (pointer == IntPtr.Zero)
            throw new JyunrcaeaFrameworkException($"???? ?? ?? ?? SDL Error: {SDL.SDL_GetError()}");
    }

    /// <summary>
    /// ??? ??? ????? ?????.
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
