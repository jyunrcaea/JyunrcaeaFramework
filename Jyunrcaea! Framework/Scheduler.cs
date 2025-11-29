namespace JyunrcaeaFramework;

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