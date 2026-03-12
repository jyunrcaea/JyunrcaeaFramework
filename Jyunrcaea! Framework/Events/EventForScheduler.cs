using JyunrcaeaFramework.Core;

namespace JyunrcaeaFramework.EventSystem;

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

