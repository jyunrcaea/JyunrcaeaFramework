namespace JyunrcaeaFramework.Collections;

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
        this.ResourceReady = true;
        for (int i = 0; i < this.Objects.Count; i++)
        {
            this.Ancestor!.PushEvent(this.Objects[i]);

            if (this.Objects[i] is Group)
            {
                ((Group)this.Objects[i]).Objects.Ancestor = ((Group)this.Objects[i]).Ancestor = this.Ancestor;
            }

            if (this.Objects[i] is Events.IPrepare prepare)
                prepare.Prepare();
        }
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
        }
    }

    public virtual void Update(float ms)
    {
        if (this.Objects.AddQueueCount != 0)
            this.Objects.Update();
        for (int i = 0; i < this.Objects.Count; i++)
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
        for (int i = 0; i < group.Objects.Count; i++)
        {
            this.PushEvent(group.Objects[i]);
        }
    }

    internal override void UpdatePosition(int parentX, int parentY)
    {
        Framework.DrawPosStack.Push(new() { Width = Framework.DrawPos.x, Height = Framework.DrawPos.y });
        int wx = parentX + this.Rx;
        int hy = parentY + this.Ry;

        for (int i = 0; i < this.Objects.Count; i++)
        {
            Framework.DrawPos.x = wx;
            Framework.DrawPos.y = hy;
            this.Objects[i].UpdatePosition(wx, hy);
        }

        var ret = Framework.DrawPosStack.Pop();
        Framework.DrawPos.x = ret.Width;
        Framework.DrawPos.y = ret.Height;
    }

    internal override void Render(IntPtr renderer)
    {
        for (int i = 0; i < this.Objects.Count; i++)
        {
            if (!this.Objects[i].hide)
                this.Objects[i].Render(renderer);
        }
    }
}
