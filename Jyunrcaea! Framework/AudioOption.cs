namespace JyunrcaeaFramework;

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