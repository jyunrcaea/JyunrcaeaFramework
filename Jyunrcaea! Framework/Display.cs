using SDL2;

namespace JyunrcaeaFramework;

/// <summary>
/// 모니터의 정보를 얻거나, 또는 장면을 추가/제거 하기위한 클래스입니다.
/// </summary>
public static class Display
{
    static TopGroup _target = new();
    /// <summary>
    /// Zenerety 렌더링 방식에 사용되는 장면 탐색기입니다.
    /// </summary>
    public static TopGroup Target {
        get => _target;
        set {
            if (Framework.Running)
            {
                _target.Release();
                value.Prepare();
            }
            _target = value;
        }
    }

    internal static SDL.SDL_DisplayMode dm;

    static float fps = 60;
    /// <summary>
    /// 모니터의 픽셀 너비를 구합니다.
    /// </summary>
    public static int MonitorWidth => dm.w;
    /// <summary>
    /// 모니터의 픽셀 높이를 구합니다.
    /// </summary>
    public static int MonitorHeight => dm.h;
    /// <summary>
    /// 모니터의 주사율을 구합니다.
    /// </summary>
    public static int MonitorRefreshRate => dm.refresh_rate;

    internal static long framelatelimit = 166666;

    /// <summary>
    /// 초당 프레임을 제한합니다. 0을 할경우 모니터 주사율에 맞춥니다.
    /// 무한 프레임을 하고 싶다면 적당히 큰 수를 넣으면 됩니다.
    /// (주의) 프레임워크를 초기화 한뒤 사용해야합니다.
    /// </summary>
    public static float FrameLateLimit {
        get => fps;
        set {
            if ((fps = value) == 0) {
                if (dm.refresh_rate == 0) throw new JyunrcaeaFrameworkException("알수없는 디스플레이 정보");
                fps = dm.refresh_rate;
            }
            framelatelimit = (long)(1d / fps * 10000000);
        }
    }

    public static bool KeepRenderingWhenResize { get; internal set; }
}