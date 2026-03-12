using JyunrcaeaFramework.Core;
using JyunrcaeaFramework.Interfaces;
using JyunrcaeaFramework.Structs;
using SDL2;

namespace JyunrcaeaFramework.Objects;

/// <summary>
/// 그릴수 있는 객체
/// </summary>
public abstract class DrawableObject : BaseObject, DetailOfObject.Size
{
    internal int rw = 0, rh = 0;
    internal int ww = 0, hh = 0;

    /// <summary>
    /// 실제 너비
    /// </summary>
    internal virtual int RealWidth { get; }

    /// <summary>
    /// 실제 높이
    /// </summary>
    internal virtual int RealHeight { get; }

    public int DisplayedWidth => this.RealWidth;
    public int DisplayedHeight => this.RealHeight;

    public abstract byte Opacity { get; set; }

    /// <summary>
    /// 렌더링 될 이미지의 절대적 크기
    /// (null 로 설정시 원본 이미지의 크기를 따릅니다.)
    /// </summary>
    public Size2D? absoluteSize = null;

    /// <summary>
    /// 이미지의 너비 및 높이의 배율
    /// </summary>
    public Scale2D scale = new();

    public double ScaleX
    {
        get => scale.X;
        set => scale.X = value;
    }

    public double ScaleY
    {
        get => scale.Y;
        set => scale.Y = value;
    }

    /// <summary>
    /// 창 크기에 맞춰 자동으로 크기 조정을 사용할지에 대한 여부입니다.
    /// </summary>
    public bool RelativeSize { get; set; } = true;

    /// <summary>
    /// 실제 렌더링 범위
    /// </summary>
    internal SDL.SDL_Rect renderPosition = new();

    public bool MouseOver =>
        SDL.SDL_PointInRect(ref Input.Mouse.position, ref this.renderPosition) == SDL.SDL_bool.SDL_TRUE;

    public bool OverLap(DrawableObject otherTarget) =>
        SDL.SDL_IntersectRect(ref this.renderPosition, ref otherTarget.renderPosition, out _) == SDL.SDL_bool.SDL_TRUE;

    public HorizontalPositionType DrawX { get; set; } = HorizontalPositionType.Middle;
    public VerticalPositionType DrawY { get; set; } = VerticalPositionType.Middle;

    internal override void UpdatePosition(int parentX, int parentY)
    {
        Framework.DrawPos.x = parentX;
        Framework.DrawPos.y = parentY;
        Framework.DrawPos.w = this.RealWidth;
        Framework.DrawPos.h = this.RealHeight;
        Framework.DrawPos.x += this.Rx;
        Framework.DrawPos.y += this.Ry;
        if (this.DrawX != HorizontalPositionType.Right) Framework.DrawPos.x -= this.DrawX == HorizontalPositionType.Middle ? (int)(Framework.DrawPos.w * 0.5f) : Framework.DrawPos.w;
        if (this.DrawY != VerticalPositionType.Bottom) Framework.DrawPos.y -= this.DrawY == VerticalPositionType.Middle ? (int)(Framework.DrawPos.h * 0.5f) : Framework.DrawPos.h;
        this.renderPosition.w = Framework.DrawPos.w;
        this.renderPosition.h = Framework.DrawPos.h;
        this.renderPosition.x = Framework.DrawPos.x;
        this.renderPosition.y = Framework.DrawPos.y;
    }
}
