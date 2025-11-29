using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JyunrcaeaFramework;

public class Text : ExtendDrawableObject, Events.IUpdate, Events.IResize
{
    public Text(string? content = null , int size = 16 , Color? textcolor = null , Font? font = null)
    {
        this.TFT = new TextTexture(font is null ? new(Font.DefaultPath, size) : font , realsize = size , content is null ? string.Empty : content , textcolor is null ? Color.Black : textcolor);
    }

    internal TextTexture TFT;
    public string Content {
        get => TFT.Text;
        set {
            TFT.Text = value;
            refresh = true;
        }
    }
    internal int realsize;
    public int FontSize {
        get => realsize;
        set {
            realsize = value;
            TFT.Resize(this.RelativeSize ? (int)(Window.AppropriateSize * (float)realsize) : realsize);
            refresh = true;
        }
    }
    public uint WrapWidth {
        get => TFT.WarpLength;
        set {
            TFT.WarpLength = value;
            refresh = true;
        }
    }
    public Color TextColor {
        get => TFT.Color;
        set {
            TFT.Color = value;
            refresh = true;
        }
    }

    public Color? Background {
        get => TFT.BackgroundColor;
        set {
            TFT.BackgroundColor = value;
            refresh = true;
        }
    }

    internal override int RealWidth => (int)((absoluteSize is null ? TFT.Width : absoluteSize.Width) * scale.X);
    internal override int RealHeight => (int)((absoluteSize is null ? TFT.Height : absoluteSize.Height) * scale.Y);

    public bool Blended { get => TFT.Blended; set => TFT.Blended = value; }
    internal bool refresh = false;

    public override byte Opacity { get => TFT.Opacity; set => TFT.Opacity = value; }

    /// <summary>
    /// 변경 사항이 있을경우 텍스트를 다시 렌더링합니다. (일반적으로 이 코드는 마지막에 호출하는게 좋습니다.)
    /// </summary>
    /// <param name="ms"></param>
    public virtual void Update(float ms)
    {
        if (refresh)
        {
            if (this.FontSize != 0)
                TFT.ReRender();
            refresh = false;
        }
    }

    public virtual void Resize()
    {
        if (this.RelativeSize)
        { TFT.Resize((int)(Window.AppropriateSize * (float)realsize)); refresh = true; }
    }
}