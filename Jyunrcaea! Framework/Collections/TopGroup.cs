namespace JyunrcaeaFramework.Collections;

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
