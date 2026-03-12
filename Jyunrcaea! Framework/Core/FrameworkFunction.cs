using JyunrcaeaFramework.Collections;
using JyunrcaeaFramework.EventSystem;
using JyunrcaeaFramework.Interfaces;
using JyunrcaeaFramework.Structs;
using SDL2;

namespace JyunrcaeaFramework.Core;

/// <summary>
/// 프레임워크 이벤트 처리 기본 구현입니다.
/// </summary>
public class FrameworkFunction : ObjectInterface, IAllEventInterface
{
    static EventList EventManager => Display.Target.EventManager;
    static readonly float tickToMilliseconds = 1000f / System.Diagnostics.Stopwatch.Frequency;

    static void InvokeSafely<T>(List<T> targets, Action<T> action)
    {
        var snapshot = targets.ToArray();
        for (int i = 0; i < snapshot.Length; i++)
        {
            action(snapshot[i]);
        }
    }

    internal static void Prepare(Group group)
    {
        group.Prepare();
    }

    public virtual void Prepare()
    {
        Prepare(Display.Target);
    }

    internal static void Destroy(Group group)
    {
        group.Destroy();
    }

    public virtual void Destroy()
    {
        Destroy(Display.Target);
    }

    public override void Start()
    {
        Display.Target.Prepare();
    }

    public override void Stop()
    {
        Display.Target.Destroy();
    }

    public override void Resize()
    {
        Framework.Positioning(Display.Target);
        Display.Target.Resize();
        Framework.Positioning(Display.Target);
    }

    internal static long endtime = 0;

    internal override void Draw()
    {
        if (endtime > Framework.frametimer.ElapsedTicks)
        {
            if (Framework.SavingPerformance && endtime > Framework.frametimer.ElapsedTicks + 2000)
                SDL.SDL_Delay(1);
            return;
        }

        Update(((updateMs = Framework.frametimer.ElapsedTicks) - updateTime) * tickToMilliseconds);

        Framework.RenderRange = Window.size;
        _ = SDL.SDL_RenderSetViewport(Framework.renderer, ref Window.size);
        Framework.Rendering(Display.Target);

        SDL.SDL_RenderPresent(Framework.renderer);

        _ = SDL.SDL_RenderSetViewport(Framework.renderer, ref Window.size);
        _ = SDL.SDL_SetRenderDrawColor(Framework.renderer, Window.BackgroundColor.Red, Window.BackgroundColor.Green, Window.BackgroundColor.Blue, Window.BackgroundColor.Alpha);
        _ = SDL.SDL_RenderClear(Framework.renderer);
        if (endtime <= Framework.frametimer.ElapsedTicks - Display.framelatelimit)
            endtime = Framework.frametimer.ElapsedTicks + Display.framelatelimit;
        else
            endtime += Display.framelatelimit;
    }

    internal static long updateTime = 0, updateMs = 0;

    public virtual void Update(float ms)
    {
        Display.Target.ResetPosition(new(Window.Width, Window.Height));
        Framework.RenderRange = Window.size;
        Framework.Positioning(Display.Target);
        Display.Target.Update(ms);
        Animation.AnimationQueue.Update();
        Framework.RenderRange = Window.size;
        Framework.Positioning(Display.Target);
        updateTime = updateMs;
    }

    public virtual void Resized()
    {
        InvokeSafely(EventManager.Resized, x => x.Resized());
    }

    public virtual void InputText()
    {
    }

    public virtual void WindowMaximized()
    {
        InvokeSafely(EventManager.windowMaximizeds, x => x.WindowMaximized());
    }

    public virtual void WindowMinimized()
    {
        InvokeSafely(EventManager.windowMinimizeds, x => x.WindowMinimized());
    }

    public virtual void WindowRestore()
    {
        InvokeSafely(EventManager.windowRestores, x => x.WindowRestore());
    }

    public virtual void WindowMove()
    {
        InvokeSafely(EventManager.windowMoves, x => x.WindowMove());
    }

    public virtual void WindowQuit()
    {
        if (Window.FrameworkStopWhenClose)
            Framework.Stop();
    }

    public virtual void DropFile(string filename)
    {
        InvokeSafely(EventManager.dropFiles, x => x.DropFile(filename));
    }

    public virtual void KeyDown(Keycode e)
    {
        InvokeSafely(EventManager.KeyDown, x => x.KeyDown(e));
    }

    public virtual void MouseMove()
    {
        SDL.SDL_GetMouseState(out Input.Mouse.position.x, out Input.Mouse.position.y);
        InvokeSafely(EventManager.mouseMoves, x => x.MouseMove());
    }

    public virtual void MouseKeyDown(MouseKey key)
    {
        InvokeSafely(EventManager.mouseKeyDowns, x => x.MouseKeyDown(key));
    }

    public virtual void MouseKeyUp(MouseKey key)
    {
        InvokeSafely(EventManager.mouseKeyUps, x => x.MouseKeyUp(key));
    }

    public virtual void KeyUp(Keycode key)
    {
        InvokeSafely(EventManager.keyUp, x => x.KeyUp(key));
    }

    public virtual void KeyFocusIn()
    {
        InvokeSafely(EventManager.keyFocusIns, x => x.KeyFocusIn());
    }

    public virtual void KeyFocusOut()
    {
        InvokeSafely(EventManager.keyFocusOuts, x => x.KeyFocusOut());
    }

    public virtual void MouseFocusIn()
    {
    }

    public virtual void MouseFocusOut()
    {
    }

    public virtual void DisplayChange()
    {
        if (Window.Fullscreen)
        {
            Window.Resize(Display.MonitorWidth, Display.MonitorHeight);
        }
        if (Display.FrameLateLimit == 0)
            Display.FrameLateLimit = 0;
    }
}
