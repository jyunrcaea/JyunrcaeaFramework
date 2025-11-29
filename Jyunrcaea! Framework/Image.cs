using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JyunrcaeaFramework;
/// <summary>
/// 이미지를 출력하는 객체입니다.
/// </summary>
public class Image : ExtendDrawableObject, Animation.Available.Opacity
{
    /// <summary>
    /// Image 객체를 생성합니다.
    /// </summary>
    /// <param name="Texture">텍스쳐</param>
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

    public override byte Opacity {
        get => Texture.Opacity;
        set {
            Texture.Opacity = value;
        }
    }

    internal override int RealWidth => (int)((absoluteSize is null ? Texture.Width : absoluteSize.Width) * scale.X * (this.RelativeSize ? Window.AppropriateSize : 1));
    internal override int RealHeight => (int)((absoluteSize is null ? Texture.Height : absoluteSize.Height) * scale.Y * (this.RelativeSize ? Window.AppropriateSize : 1));
}
