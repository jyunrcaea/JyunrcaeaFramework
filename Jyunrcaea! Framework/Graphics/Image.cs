using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JyunrcaeaFramework.Graphics;
/// <summary>
/// ???? ???? ?????.
/// </summary>
public class Image : ExtendDrawableObject, Animation.Available.Opacity
{
    /// <summary>
    /// Image ??? ?????.
    /// </summary>
    /// <param name="Texture">???</param>
    public Image(Texture? Texture = null)
    {
        if (Texture is null)
            return;
        this.Texture = Texture;
    }

    public Image(string ImageFilePath)
    {
        this.Texture = new Texture(ImageFilePath);
    }

    public Texture Texture = null!;

    public override byte Opacity
    {
        get => Texture.Opacity;
        set
        {
            Texture.Opacity = value;
        }
    }

    internal override int RealWidth => (int)((absoluteSize is null ? Texture.Width : absoluteSize.Width) * scale.X * (this.RelativeSize ? Window.AppropriateSize : 1));
    internal override int RealHeight => (int)((absoluteSize is null ? Texture.Height : absoluteSize.Height) * scale.Y * (this.RelativeSize ? Window.AppropriateSize : 1));

    internal override void Render(IntPtr renderer)
    {
        SDL.SDL_RenderCopyEx(renderer, this.Texture.texture, ref this.Texture.src, ref this.renderPosition, this.Rotation, IntPtr.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
    }
}

