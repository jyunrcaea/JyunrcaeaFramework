#define WINDOWS
using SDL2;
using System.Reflection;

namespace JyunrcaeaFramework;

/// <summary>
/// 크기를 구하거나 렌더 방향을 지정할수 있는 동적인 그룹입니다.
/// </summary>
public class DynamicGroup : Group, DetailOfObject.Size
{
    public int DisplayedWidth => contentrange.w;
    public int DisplayedHeight => contentrange.h;

    internal SDL.SDL_Rect contentrange = new();

    public override void Update(float ms)
    {
        base.Update(ms);
        int i = 0;
        while (this.Objects[i] is not DrawableObject || this.Objects[i] is not DynamicGroup)
        {
            // 그릴수 있는 객체가 없는경우
            if (i++ >= this.Objects.Count)
            {
                contentrange.x = -1;
                contentrange.y = -1;
                contentrange.w = 0;
                contentrange.h = 0;
                return;
            }
        }
        int left = ((DrawableObject)this.Objects[i]).renderPosition.x, right = left + ((DrawableObject)this.Objects[i]).renderPosition.w;
        int top = ((DrawableObject)this.Objects[i]).renderPosition.y, bottom = top + ((DrawableObject)this.Objects[i]).renderPosition.h;
        int ww, hh;
        for (;i<this.Objects.Count;i++)
        {
            if (this.Objects[i] is DrawableObject)
            {
                // 왼쪽
                ww = ((DrawableObject)this.Objects[i]).renderPosition.x;
                if (ww < left) left = ww;
                // 오른쪽
                ww += ((DrawableObject)this.Objects[i]).renderPosition.w;
                if (ww > right) right = ww;
                //위
                hh = ((DrawableObject)this.Objects[i]).renderPosition.y;
                if (hh < top) top = hh;
                //아레
                hh += ((DrawableObject)this.Objects[i]).renderPosition.h;
                if (hh > bottom) bottom = hh;
            }
            if (this.Objects[i] is DynamicGroup)
            {
                // 왼쪽
                ww = ((DynamicGroup)this.Objects[i]).contentrange.x;
                if (ww < left) left = ww;
                // 오른쪽
                ww += ((DynamicGroup)this.Objects[i]).contentrange.w;
                if (ww > right) right = ww;
                //위
                hh = ((DynamicGroup)this.Objects[i]).contentrange.y;
                if (hh < top) top = hh;
                //아레
                hh += ((DynamicGroup)this.Objects[i]).contentrange.h;
                if (hh > bottom) bottom = hh;
            }
        }

        this.contentrange.x = left;
        this.contentrange.y = top;
        this.contentrange.w = right - left;
        this.contentrange.h = bottom - top;
    }


}


/// <summary>
/// 최상위 그룹입니다.
/// </summary>
public class TopGroup : Group
{
    public TopGroup()
    {
        this.Ancestor = this;
        this.Parent = this;
        PushEvent(this);
    }

    internal EventList EventManager = new();

    internal override void PushEvent(BaseObject target)
    {
        EventManager.Add(target);
    }
}

/// <summary>
/// 크기 조정 클래스입니다.
/// </summary>
public class Size2D
{
    public int Width, Height;
    public Size2D(int Width = 0, int Height = 0)
    {
        this.Width = Width;
        this.Height = Height;
    }
}

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

/// <summary>
/// 배율 조정 구조체입니다.
/// </summary>
public struct Scale2D
{
    public double X, Y;
    public Scale2D(double x,double y)
    {
        X = x;
        Y = y;
    }
    public Scale2D(double xy)
    {
        X = Y = xy;
    }
    public Scale2D()
    {
        X = 1d;
        Y = 1d;
    }
}

/// <summary>
/// 모든 객체의 기본이 되는 핵심 객체 
/// </summary>
public class BaseObject
{
    public bool hide = false;

    public int X { get; set; }
    public int Y { get; set; }
    internal virtual int Rx => X + Cx;
    internal virtual int Ry => Y + Cy;

    public Group? Parent { get; internal set; } = null;

    public T TypedParent<T>() where T : Group
    {
        if (this.Parent is null) throw new JyunrcaeaFrameworkException("이 객체가 포함된 그룹이 없습니다.");
        return (T)this.Parent;
    }

    internal int Cx = 0, Cy = 0;

    internal double CxD = 0.5, CyD = 0.5;

    //public bool CxIsChanged = false;
    //public bool CyIsChanged = false;

    public double CenterX
    {
        get => CxD;
        //{
        //    // 이미 값이 변경되었고, 부모가 null이 아닐경우 계산
        //    if (CxIsChanged && Parent is not null)
        //    {
        //        CxIsChanged = false;
        //        Cx = (int)(Parent.RealWidth * CxD);
        //    }
        //    return CxD;
        //}
        set
        {
            CxD = value;
            //if (CxIsChanged && Parent is not null) Cx = (int)(Parent.RealWidth * value);
        }
    }
    
    public double CenterY
    {
        get => CyD;
        set
        {
            CyD = value;
            //if (Parent is not null) Cy = (int)(Parent.RealWidth * value);
        }
    }
    

    internal bool MoveAnimation { get; set; } = false;
}

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

    public double ScaleX {
        get => scale.X;
        set => scale.X = value;
    }

    public double ScaleY {
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
}

/// <summary>
/// 그리기도 가능하고 회전, 뒤집기 등이 가능한 확장된 객체
/// </summary>
public abstract class ExtendDrawableObject : DrawableObject
{
    /// <summary>
    /// 회전값
    /// </summary>
    public double Rotation { get; set; } = 0;

    public int AbsoluteWidth => this.RealWidth;
    public int AbsoluteHeight => this.RealHeight;
}

/// <summary>
/// 다른 객체를 묶어주는 그룹 객체입니다.
/// </summary>
public class Group : BaseObject, Events.IUpdate, Events.IResize, Events.IPrepare
{
    internal override int Rx => this.X;
    internal override int Ry => this.Y;

    public Group()
    {
        Objects = new(this);
    }

    internal TopGroup? Ancestor { get; set; } = null;

    public bool ResourceReady { get; internal set; } = false;

    /// <summary>
    /// 묶을 객체들
    /// </summary>
    public ObjectList Objects { get; protected set; } = null!;

    internal virtual void ResetPosition(Size2D Position)
    {
        for (int i = 0; i < Objects.Count; i++)
        {
            Objects[i].Cx = (int)(Position.Width * Objects[i].CxD);
            Objects[i].Cy = (int)(Position.Height * Objects[i].CyD);
            if (Objects[i] is Group) ((Group)Objects[i]).ResetPosition(Position);
        }
    }

    internal int RealWidth => base.Parent is null ? Window.Width : base.Parent.RealWidth;

    internal int RealHeight => base.Parent is null ? Window.Height : base.Parent.RealHeight;

    /// <summary>
    /// 오브젝트 준비
    /// </summary>
    public virtual void Prepare()
    {
        for (int i = 0; i < this.Objects.Count; i++)
        {
            this.Ancestor!.PushEvent(this.Objects[i]);

            if (this.Objects[i] is Group)
            {
                ((Group)this.Objects[i]).Objects.Ancestor =((Group)this.Objects[i]).Ancestor = this.Ancestor;
            }
            else if (this.Objects[i] is Text)
            {
                ((Text)Objects[i]).TFT.Ready();
            }

            if (this.Objects[i] is Events.IPrepare prepare)
                prepare.Prepare();
        }
        this.ResourceReady = true;
    }

    /// <summary>
    /// 하위 객체들의 리소스를 해제하는 함수입니다. (override 할 경우 base.Release() 가 한번 실행되어야 합니다.)
    /// </summary>
    public virtual void Release()
    {
        this.ResourceReady = false;
        for (int i = 0; i < this.Objects.Count; i++)
        {
            if (this.Objects[i] is Group)
            {
                ((Group)this.Objects[i]).Release();
                continue;
            }

            if (this.Objects[i] is Image)
            {
                ((Image)this.Objects[i]).Texture.Dispose();
            }

            if (this.Objects[i] is Text)
            {
                ((Text)this.Objects[i]).TFT.Dispose();
            }
        }
    }

    public virtual void Update(float ms)
    {
        if(this.Objects.AddQueueCount != 0)
            this.Objects.Update();
        for (int i=0;i<this.Objects.Count;i++)
        {
            if (Objects[i] is Events.IUpdate) ((Events.IUpdate)Objects[i]).Update(ms);
        }
    }

    public virtual void Resize()
    {
        for (int i = 0; i < this.Objects.Count; i++)
        {
            if (Objects[i] is Events.IResize) ((Events.IResize)Objects[i]).Resize();
        }
    }

    internal virtual void PushEvent(BaseObject target)
    {
        this.Ancestor!.PushEvent(target);
        if (target is not Group)
        {
            return;
        }
        Group group = (Group)target;
        for (int i =0;i<group.Objects.Count;i++)
        {
            this.PushEvent(group.Objects[i]);
        }
    }
}

/// <summary>
/// 직사각형을 그리는 객체입니다.
/// </summary>
public class Box : DrawableObject, Animation.Available.Opacity, Animation.Available.Size
{
    public Box(int width = 0,int height = 0,Color? color = null)
    {
        this.Size = new(width, height);
        this.Color = color is null ? Color.White : color;
    }

    /// <summary>
    /// 직사각형의 너비와 높이
    /// </summary>
    public Size2D Size { get; set; }

    /// <summary>
    /// 출력할 색상
    /// </summary>
    public Color Color;

    public override byte Opacity { get => Color.Alpha; set => Color.Alpha = value; }

    internal override int RealWidth =>  (int)(Size.Width * scale.X * (this.RelativeSize ? Window.AppropriateSize : 1));
    internal override int RealHeight => (int)(Size.Height * scale.Y * (this.RelativeSize ? Window.AppropriateSize : 1));
}

/// <summary>
/// 원을 그리는 객체입니다.
/// </summary>
public class Circle : DrawableObject, Animation.Available.Opacity
{
    public Circle(short radius=0, Color? color = null)
    {
        this.Radius = radius;
        this.Color = color is null ? Color.White : color;
    }

    public short Radius;

    public Color Color;

    public override byte Opacity { get => Color.Alpha; set => Color.Alpha = value; }

    internal override int RealWidth => (int)(Radius * 2 * (RelativeSize ? Window.AppropriateSize : 1));
    internal override int RealHeight => (int)(Radius * 2 * (RelativeSize ? Window.AppropriateSize : 1));
}

/// <summary>
/// 그려지지 않는 가짜 객체입니다.
/// </summary>
public class GhostBox : DrawableObject
{
    public GhostBox(int width=0,int height=0)
    {
        this.Size = new(width, height);
    }
    public Size2D Size;

    public override byte Opacity { get; set; } = 0;

    internal override int RealWidth => (int)(Size.Width * scale.X * (this.RelativeSize ? Window.AppropriateSize : 1));
    internal override int RealHeight => (int)(Size.Height * scale.Y * (this.RelativeSize ? Window.AppropriateSize : 1));
}

/// <summary>
/// 모서리가 둥근 직사각형을 그리는 객체입니다.
/// </summary>
public class RoundBox : Box
{
    public RoundBox(int Width = 0,int Height = 0,short Radius = 0,Color? color = null) : base(Width,Height,color)
    {
        this.Radius = Radius;
    }

    /// <summary>
    /// 모서리의 둥글기 정도 (픽셀 기준)
    /// </summary>
    public short Radius;
}

class AddTarget
{
    public BaseObject target;
    public int index;

    public AddTarget(BaseObject obj,int pos = -1)
    {
        this.index = pos;
        this.target = obj;
    }
}

/// <summary>
/// 오브젝트 리스트입니다. 추가 및 제거시 자동으로 리소스를 할당/해제 해줍니다.
/// </summary>
public class ObjectList : List<BaseObject>, IDisposable
{
    internal TopGroup? Ancestor;

    Group Parent;
    public ObjectList(Group group)
    {
        Parent = group;
        Ancestor = group.Ancestor;
    }

    Queue<AddTarget> AddList = new();
    public int AddQueueCount => AddList.Count;

    /// <summary>
    /// 객체 목록에 변경해야될 사항이 있는 경우 목록을 새로 고칩니다.
    /// 만약 즉시 추가해야될 객체가 있는경우 이 함수를 호출하여 즉시 추가하세요.
    /// </summary>
    public void Update()
    {
        if (AddList.Count == 0) return;
        while (AddList.Count != 0)
        {
            var target = AddList.Dequeue();
            if (target.index == -1) base.Add(target.target);
            else base.Insert(target.index, target.target);
            if (target.target is Text)
            {
                Console.WriteLine(((Text)target.target).TFT.ResourceReady);
            }
            //AddProcedure(target);
        }
    }

    void AddProcedure(BaseObject obj)
    {
        if (this.Ancestor is not null) this.Ancestor.EventManager.Add(obj);
        if (obj is Group)
        {
            ((Group)obj).Ancestor = this.Ancestor;
            ((Group)obj).Prepare();
            return;
        }
        if (obj is Image)
        {
            ((Image)obj).Texture.Ready();
            return;
        }
        if (obj is Text)
        {
            if (!((Text)obj).TFT.ResourceReady) ((Text)obj).TFT.Ready();
        }
        //if(Ancestor is not null) Ancestor.EventManager.Add(obj);
    }

    /// <summary>
    /// 객체를 추가합니다.
    /// </summary>
    /// <param name="obj">객체</param>
    public new void Add(BaseObject obj)
    {
        if (obj.Parent is not null) throw new JyunrcaeaFrameworkException("이미 다른 부모 객체에게 상속된 객체입니다.");
        obj.Parent = this.Parent;
        if (this.Parent.ResourceReady)
        {
            AddProcedure(obj);
            this.AddList.Enqueue(new(obj));
        }
        else base.Add(obj);
    }

    public new void AddRange(IEnumerable<BaseObject> objs)
    {
        foreach(var a in objs)
        {
            Add(a);
        }
    }

    public void AddRange(params BaseObject[] objs)
    {
        for (int i =0;i < objs.Length;i++)
        {
            this.Add(objs[i]);
        }
    }

    public void AddRange(int index = 0,params BaseObject[] objs)
    {
        for (int i = 0; i < objs.Length; i++)
        {
            this.Insert(index + i, objs[i]);
        }
    }

    public new void Insert(int index,BaseObject zo)
    {
        if (zo.Parent is not null) throw new JyunrcaeaFrameworkException("이미 다른 부모 객체에게 상속된 객체입니다.");
        zo.Parent = this.Parent;
        if (this.Parent.ResourceReady) {
            AddProcedure(zo);
            this.AddList.Enqueue(new(zo, index));
        }
        else base.Insert(index, zo);
    }


    public bool Switch(BaseObject target,int index = -1)
    {
        if (!base.Remove(target)) return false;

        if (index == -1)
        {
            base.Add(target);
        } else
        {
            base.Insert(index, target);
        }
        return true;
    }

    /// <summary>
    /// 객체를 제거합니다.
    /// </summary>
    /// <param name="obj">객체</param>
    public new bool Remove(BaseObject obj)
    {
        if(base.Remove(obj) is false) return false;
        if (obj is Group)
        {
            ((Group)obj).Release();
            return true;
        }
        if (obj is Image)
        {
            ((Image)obj).Texture.Dispose();
            return true;
        }
        if (obj is Text)
        {
            ((Text)obj).TFT.Dispose();
            return true;
        }
        return true;
    }

    public void Dispose()
    {
        if (Framework.Running)
        foreach(var obj in this)
        {
            if (obj is Group)
            {
                ((Group)obj).Release();
                return;
            }
            if (obj is Image)
            {
                ((Image)obj).Texture.Dispose();
                return;
            }
            if (obj is Text)
            {
                ((Text)obj).TFT.Dispose();
                return;
            }  
        }
    }
}

/// <summary>
/// 창을 생성할때 쓰일 창 옵션입니다.
/// </summary>
public struct WindowOption
{
    internal SDL.SDL_WindowFlags option;

    public WindowOption(bool resize = true, bool borderless = false, bool fullscreen = false, bool hide = true)
    {
        option = SDL.SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI;
        if (resize) option |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
        if (borderless) option |= SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
        if (fullscreen) option |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
        // 보더리스 지원 포기
        //if (fullscreen_desktop) option |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
        if (hide) option |= SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN;
    }
}

/// <summary>
/// 프레임워크를 초기화 할때 쓰일 렌더러 옵션입니다.
/// </summary>
public struct RenderOption
{
    internal SDL.SDL_RendererFlags option = new();
    public bool anti_alising = true;

    public RenderOption(bool sccelerated = true, bool software = true, bool vsync = false, bool anti_aliasing = true)
    {
        if (sccelerated) option |= SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED;
        if (software) option |= SDL.SDL_RendererFlags.SDL_RENDERER_SOFTWARE;
        if (vsync) option |= SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC;
        //option |= SDL.SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE;
        this.anti_alising = anti_aliasing;
    }
}

/// <summary>
/// 프레임워크를 초기화 할때 쓰일 오디오 옵션입니다.
/// </summary>
public struct AudioOption
{
    internal int ch, cs, hz;
    internal bool trylow;

    public AudioOption(byte Channals = 8, bool TryLowChannals = true, int ChunkSize = 8192, int Hz = 48000)
    {
        trylow = TryLowChannals;
        ch = Channals;
        cs = ChunkSize;
        hz = Hz;
    }
}

/// <summary>
/// 객체의 기본이 되는 객체 인터페이스입니다. (사실 추상 클래스이긴 하지만...)
/// </summary>
public abstract class ObjectInterface
{
    public abstract void Resize();
    public abstract void Stop();
    internal abstract void Draw();
    public abstract void Start();
    public bool Hide = false;

    //public abstract int X { get; set; }
    //public abstract int Y { get; set; }
}

public interface IDefaultObjectPositionInterface
{
    public int X { get; set; }
    public int Y { get; set; }
}

/// <summary>
/// RGBA 형식으로 색을 표현하는 자료형입니다. 0~255 사이의 값을 담습니다.
/// </summary>
public class Color
{
    public Color(byte red=255,byte green=255,byte blue=255,byte alpha = 255)
    {
        this.colorbase = new()
        {
            r = red,
            g = green,
            b = blue,
            a = alpha
        };
    }

    public byte Red { get => this.colorbase.r; set => this.colorbase.r = value; }

    public byte Green { get => this.colorbase.g; set => this.colorbase.g = value; }

    public byte Blue { get => this.colorbase.b; set => this.colorbase.b = value; }

    public byte Alpha { get => this.colorbase.a; set => this.colorbase.a = value; }

    public Color Copy => new(this.Red, this.Green, this.Blue, this.Alpha);



    internal SDL.SDL_Color colorbase = new();

    //흑백 계열
    public static Color White => new(255, 255, 255);
    public static Color Black => new(0,0,0,255);
    public static Color Gray => new(127, 127, 127);
    public static Color Silver => new(192, 192, 192);
    public static Color DarkGray => new(63, 63, 63);
    //보라 계열
    public static Color Purple = new(128, 0, 128);
    public static Color Violet => new(127, 0, 255);
    public static Color Lilac => new(200, 162, 200);
    public static Color Lavender => new(230,230,255);
    //남보라 계열
    public static Color Periwinkle => new(128, 128, 255);
    public static Color LightPeriwinkle => new(204, 204, 255);
    //남색 계열
    public static Color Indigo => new(75, 0, 130);
    public static Color Navy => new(0, 0, 128);
    //초록 계열
    public static Color Lime => new(0, 255, 0);
    public static Color DarkGreen => new(0, 128, 0);
    //연두 계열
    public static Color YellowGreen => new (154, 205, 50);
    public static Color GreenYellow = new (173, 255, 47);
    public static Color Chartreuse => new(127, 255, 0);
    public static Color GrassGreen => new (117, 166, 74);
    public static Color YellowishGreen => new (160, 176, 54);
    //청록 계열
    //추가 예정
    //노랑 계열
    public static Color Yellow => new(255, 255, 0);
    public static Color Turbo = new(255, 204, 33);
    public static Color MoonYellow => new (240, 196, 32);
    public static Color VividYellow => new (255, 227, 2);
    public static Color GoldenYellow => new (255, 140, 0);
    //주황 계열
    public static Color Orange => new(255, 127, 0);
    public static Color DarkOrange => new (255, 140, 0);
}

/// <summary>
/// 수학 공식을 까먹은 당신을 위해... 편리한 기능을 제공하는 함수들이 모여있습니다.
/// </summary>
public static class Convenience
{
    /// <summary>
    /// 두 객체가 서로 겹치는 부분이 있는지 (닿았는지) 판단합니다. (직사각형 기준)
    /// </summary>
    /// <param name="sp1">첫번째 객체</param>
    /// <param name="sp2">두번째 객체</param>
    /// <returns>겹친 부분이 있을경우 True 를 반환합니다.</returns>
    public static bool Overlap(DrawableObject sp1,DrawableObject sp2)
    {
        return SDL.SDL_IntersectRect(ref sp1.renderPosition, ref sp2.renderPosition , out _) == SDL.SDL_bool.SDL_TRUE;
    }

    public static bool MouseOver(DrawableObject Target)
    {
        if (Target is Circle)
        {
            return Math.Sqrt(Math.Pow((Target.Rx - Input.Mouse.X), 2) + Math.Pow((Target.Ry - Input.Mouse.Y), 2)) <= ((Circle)Target).Radius;
        }
        return SDL.SDL_PointInRect(ref Input.Mouse.position, ref Target.renderPosition) == SDL.SDL_bool.SDL_TRUE;
    }

    /// <summary>
    /// 두 객체의 거리를 구합니다.
    /// </summary>
    /// <param name="sp1">첫번째 객체</param>
    /// <param name="sp2">두번째 객체</param>
    /// <returns>거리</returns>
    public static double Distance(DrawableObject sp1, DrawableObject sp2)
    {
        int x = sp1.renderPosition.x - sp2.renderPosition.x, y = sp1.renderPosition.y - sp2.renderPosition.y;
        return Math.Sqrt(x * x + y * y);
    }
}

/// <summary>
/// 너비와 높이를 얻을수 있는 객체에게만 상속되는 인터페이스입니다.
/// </summary>
public interface CanGetLenght
{
    public int Width { get; }
    public int Height { get; }
}

public class RectSize
{
    internal SDL.SDL_Rect size;
    public int X { get => size.x; set => size.x = value; }
    public int Y { get => size.y; set => size.y = value; }
    public int Width { get => size.w; set => size.w = value; }
    public int Height { get => size.h; set => size.h = value; }
    public RectSize(int x = 0,int y = 0, int w = 0, int h = 0)
    {
        size = new() { x = x, y = y, w = w, h = h };
    }
}

public class EventForScheduler
{
    public Action Function { get; internal set; }
    public double StartTime { get; internal set; }

    public EventForScheduler(Action Function,double StartTime=0)
    {
        this.Function = Function;
        this.StartTime = Framework.RunningTime + StartTime;
    }
}

public class Scheduler
{
    public LinkedList<EventForScheduler> Queue { get; internal set; } = new();
    internal Queue<EventForScheduler> CallList = new();

    /// <summary>
    /// 곧 호출해야할 함수들을 호출리스트에 넣습니다.
    /// </summary>
    internal void Update()
    {
        
    }

    /// <summary>
    /// 
    /// </summary>
    internal void Refresh()
    {

    }
}

class EffectForImage
{
    internal static PaintOnMemory Bluring(ImageOnMemory image)
    {
        byte r, g, b, a;
        int maxw = image.Width - 1;
        int maxh = image.Height - 1;

        double[,] RedMap = new double[image.Width, image.Height];
        double[,] BlueMap = new double[image.Width, image.Height];
        double[,] GreenMap = new double[image.Width, image.Height];
        double[,] AlphaMap = new double[image.Width, image.Height];

        void Add(int x, int y, double n)
        {
            RedMap[x, y] += r * n;
            BlueMap[x, y] += b * n;
            GreenMap[x, y] += g * n;
            AlphaMap[x, y] += a * n;
        }

        //1단계
        void OneBlur(int x, int y)
        {
            image.GetRGBA(x, y, out r, out g, out b, out a);
            Add(x, y, 0.25);
            if (x != 0)
            {
                Add(x - 1, y, 0.125);
                if (y != 0)
                {
                    Add(x - 1, y - 1, 0.0625);
                }
                if (y != maxh)
                {
                    Add(x - 1, y + 1, 0.0625);
                }
            }
            if (y != 0)
            {
                Add(x, y - 1, 0.125);
            }
            if (x != maxw)
            {
                Add(x + 1, y, 0.125);
                if (y != 0)
                {
                    Add(x + 1, y - 1, 0.0625);
                }
                if (y != maxh)
                {
                    Add(x + 1, y + 1, 0.0625);
                }
            }
            if (y != maxh)
            {
                Add(x, y + 1, 0.125);
            }
        }

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                OneBlur(x, y);
            }
        }
        //가장자리 처리
        image.GetRGBA(1, 1, out r, out g, out b, out a);
        Add(0, 0, 0.4375);
        image.GetRGBA(maxw - 1, maxh - 1, out r, out g, out b, out a);
        Add(maxw, maxh, 0.4375);
        image.GetRGBA(1, maxh - 1, out r, out g, out b, out a);
        Add(0, maxh, 0.4375);
        image.GetRGBA(maxw - 1, 1, out r, out g, out b, out a);
        Add(maxw, 0, 0.4375);

        //가로 처리
        for (int x = 1; x < maxw; x++)
        {
            image.GetRGBA(x, 1, out r, out g, out b, out a);
            Add(x, 0, 0.25);
            image.GetRGBA(x, maxh - 1, out r, out g, out b, out a);
            Add(x, maxh, 0.25);
        }
        //세로 처리
        for (int y = 1; y < maxh; y++)
        {
            image.GetRGBA(1, y, out r, out g, out b, out a);
            Add(0, y, 0.25);
            image.GetRGBA(maxw - 1, y, out r, out g, out b, out a);
            Add(maxw, y, 0.25);
        }

        PaintOnMemory paint = new(image.Width, image.Height);

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                //점찍기
                paint.Point(x, y, (byte)RedMap[x, y], (byte)GreenMap[x, y], (byte)BlueMap[x, y], (byte)AlphaMap[x, y]);
            }
        }

        return paint;
    }
}

public class TextureFromStringForXPM : Texture
{
    public TextureFromStringForXPM(string[] xpmdata)
    {
        this.StringForXPM = xpmdata;
        IntPtr surface = SDL_image.IMG_ReadXPMFromArray(StringForXPM);
        if (surface == IntPtr.Zero)
            throw new JyunrcaeaFrameworkException($"불러올수 없는 XPM 문자열 SDL Error: {SDL.SDL_GetError()}");
        this.texture = SDL.SDL_CreateTextureFromSurface(Framework.renderer , surface);
        SDL.SDL_FreeSurface(surface);
        if (this.texture == IntPtr.Zero)
            throw new JyunrcaeaFrameworkException($"텍스쳐로 변환 실패함 SDL Error: {SDL.SDL_GetError()}");
        this.needresettexture = true;
        SDL.SDL_QueryTexture(this.texture , out _ , out _ , out this.absolutesrc.x , out this.absolutesrc.y);
        if (!this.FixedRenderRange)
        {
            this.src.w = this.absolutesrc.x;
            this.src.h = this.absolutesrc.y;
        }
        Ready();
    }

    public string[] StringForXPM = null!;

    public override void Dispose()
    {
        SDL.SDL_DestroyTexture(this.texture);
        this.texture = IntPtr.Zero;
    }
}

public class TextureFromTextFileForXPM : Texture
{
    public TextureFromTextFileForXPM(string FilePath)
    {
        this.FilePath = FilePath;
        string[] data = File.ReadAllLines(FilePath);
        IntPtr surface = SDL_image.IMG_ReadXPMFromArray(data);
        if (surface == IntPtr.Zero)
            throw new JyunrcaeaFrameworkException($"불러올수 없는 XPM 문자열 SDL Error: {SDL.SDL_GetError()}");
        this.texture = SDL.SDL_CreateTextureFromSurface(Framework.renderer , surface);
        SDL.SDL_FreeSurface(surface);
        if (this.texture == IntPtr.Zero)
            throw new JyunrcaeaFrameworkException($"텍스쳐로 변환 실패함 SDL Error: {SDL.SDL_GetError()}");
        Ready();
    }

    public string FilePath = string.Empty;

    public override void Dispose()
    {
        SDL.SDL_DestroyTexture(this.texture);
    }
}

/// <summary>
/// 객체에 대한 자세한 정보를 얻어냅니다.
/// 객체는 렌더링 때 좌표나 크기 등 값들이 새로고침 되므로, 업데이트 도중에 변경사항이 있어도 그 사항이 적용된 값을 제공하지 않는다는점 주의해주세요.
/// </summary>
public static class DetailOfObject
{
    public interface Size
    {
        public int DisplayedWidth { get; }
        public int DisplayedHeight { get; }
    }

    public static int DrawWidth(DrawableObject obj)
    {
        return obj.RealWidth;
    }

    public static int DrawHeight(DrawableObject obj)
    {
        return obj.RealHeight;
    }

    /// <summary>
    /// 실제 렌더링 위치를 알아냅니다. (객체의 왼쪽 위 모서리의 좌표)
    /// </summary>
    /// <param name="obj">객체</param>
    /// <param name="x">X 좌표</param>
    /// <param name="y">Y 좌표</param>
    public static void RealPosition(DrawableObject obj,out int x,out int y)
    {
        x = obj.Rx;
        y = obj.Ry;
    }

    /// <summary>
    /// 하위 객체의 위치를 다시 맞춥니다.
    /// </summary>
    [Obsolete("정확하지 않음")]
    public static void ResetPosition(Group target)
    {
        if (target.Parent is not null)
        {
            Stack<Group> top = new();
            top.Push(target.Parent);
            Group? g;
            while ((g = top.First().Parent) is not TopGroup)
            {
                if (g is null) break;
                top.Push(g);
            }
            int x=0, y=0;
            while (top.Count != 0)
            {
                g = top.Pop();
                x += g.Rx;
                y += g.Ry;
            }
            Framework.DrawPos.x = x;
            Framework.DrawPos.y = y;
        }
        Framework.Positioning(target);
    }
}


/// <summary>
/// 쥰르케아 프레임워크 내에 발생하는 예외적인 오류입니다.
/// 프레임워크의 예외 오류는 작동 원리만 잘 파악하면 예방할수 있습니다.
/// </summary>
public class JyunrcaeaFrameworkException : Exception
{
    public JyunrcaeaFrameworkException() { }
    /// <summary>
    /// 쥰르케아 프레임워크 예외 오류
    /// </summary>
    /// <param name="message">오류 내용</param>
    public JyunrcaeaFrameworkException(string message) : base(message) { }
}