using JyunrcaeaFramework.Core;
using JyunrcaeaFramework.EventSystem;
using JyunrcaeaFramework.Graphics;
using SDL2;

namespace JyunrcaeaFramework.Objects;

/// <summary>
/// 동적 <see cref="TextTexture"/>을 기반으로 하는 렌더링 가능한 텍스트 객체입니다.
/// </summary>
public class Text : ExtendDrawableObject, Events.IUpdate, Events.IResize
{
    /// <summary>
    /// 선택적 콘텐츠, 크기, 색상 및 글꼴을 사용하여 텍스트 객체를 생성합니다.
    /// </summary>
    /// <param name="content">표시할 텍스트 콘텐츠입니다. null인 경우 빈 문자열로 초기화됩니다.</param>
    /// <param name="size">기본 글꼴 크기(픽셀)입니다. 기본값은 16입니다.</param>
    /// <param name="textcolor">텍스트의 전경색입니다. null인 경우 검은색(Color.Black)이 사용됩니다.</param>
    /// <param name="font">사용할 글꼴입니다. null인 경우 <see cref="Font.DefaultPath"/>에서 내부 글꼴이 생성됩니다.</param>
    public Text(string? content = null, int size = 16, Color? textcolor = null, Font? font = null)
    {
        bool ownFont = font is null;
        Font targetFont = font ?? new Font(Font.DefaultPath, size);

        TFT = new TextTexture(
            targetFont,
            realsize = size,
            content ?? string.Empty,
            textcolor ?? Color.Black,
            ownsFont: ownFont
        );
    }

    internal TextTexture TFT;

    /// <summary>
    /// 이 객체에 표시되는 텍스트 콘텐츠입니다.
    /// </summary>
    public string Content
    {
        get => TFT.Text;
        set
        {
            TFT.Text = value;
            refresh = true;
        }
    }

    internal int realsize;

    /// <summary>
    /// 상대 크기 조정 전에 사용되는 기본 글꼴 크기입니다.
    /// </summary>
    public int FontSize
    {
        get => realsize;
        set
        {
            realsize = value;
            TFT.Resize(RelativeSize ? (int)(Window.AppropriateSize * realsize) : realsize);
            refresh = true;
        }
    }

    /// <summary>
    /// 픽셀 단위의 최대 텍스트 줄바꿈 너비입니다.
    /// 0으로 설정하면 줄바꿈 제한이 없습니다.
    /// </summary>
    public uint WrapWidth
    {
        get => TFT.WarpLength;
        set
        {
            TFT.WarpLength = value;
            refresh = true;
        }
    }

    /// <summary>
    /// 렌더링된 텍스트의 전경색입니다.
    /// </summary>
    public Color TextColor
    {
        get => TFT.Color;
        set
        {
            TFT.Color = value;
            refresh = true;
        }
    }

    /// <summary>
    /// 음영 텍스트 렌더링을 위한 선택적 배경색입니다.
    /// </summary>
    public Color? Background
    {
        get => TFT.BackgroundColor;
        set
        {
            TFT.BackgroundColor = value;
            refresh = true;
        }
    }

    internal override int RealWidth => (int)((absoluteSize is null ? TFT.Width : absoluteSize.Width) * scale.X);
    internal override int RealHeight => (int)((absoluteSize is null ? TFT.Height : absoluteSize.Height) * scale.Y);

    /// <summary>
    /// true일 때 텍스트의 안티앨리어싱(부드러운 렌더링)을 활성화합니다.
    /// </summary>
    public bool Blended
    {
        get => TFT.Blended;
        set
        {
            TFT.Blended = value;
            refresh = true;
        }
    }

    internal bool refresh = false;

    public override byte Opacity
    {
        get => TFT.Opacity;
        set => TFT.Opacity = value;
    }

    /// <summary>
    /// 콘텐츠 또는 스타일이 변경되었을 때 텍스처를 다시 생성합니다.
    /// </summary>
    /// <param name="ms">경과 시간(밀리초)입니다.</param>
    public virtual void Update(float ms)
    {
        if (!refresh)
        {
            return;
        }

        if (FontSize != 0)
        {
            TFT.ReRender();
        }

        refresh = false;
    }

    /// <summary>
    /// 상대 크기 모드에서 효과적인 글꼴 크기를 다시 계산합니다.
    /// </summary>
    public virtual void Resize()
    {
        if (!RelativeSize)
        {
            return;
        }

        TFT.Resize((int)(Window.AppropriateSize * realsize));
        refresh = true;
    }

    internal override void Render(IntPtr renderer)
    {
        _ = SDL.SDL_RenderCopyEx(renderer, TFT.texture, ref TFT.src, ref renderPosition, Rotation, IntPtr.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
    }

    /// <summary>
    /// 텍스처 리소스를 해제합니다. 내부적으로 소유한 글꼴 리소스도 함께 해제됩니다.
    /// </summary>
    public override void Destroy()
    {
        TFT?.Dispose();
    }
}
