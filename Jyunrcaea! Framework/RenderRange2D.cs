namespace JyunrcaeaFramework;

public class RenderRange2D : Size2D
{
    HorizontalPositionType drawX;
    VerticalPositionType drawY;

    public RenderRange2D(int width=0,int height=0,HorizontalPositionType drawX = HorizontalPositionType.Middle,VerticalPositionType drawY = VerticalPositionType.Middle) : base(width,height)
    {
        this.Width = width;
        this.Height = height;
        this.drawX = drawX;
        this.drawY = drawY;
    }
}