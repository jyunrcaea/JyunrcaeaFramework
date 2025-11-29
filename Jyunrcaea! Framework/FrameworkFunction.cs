using SDL2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JyunrcaeaFramework;


/// <summary>
/// 프레임워크가 이벤트를 받았으때 실행될 함수들이 모인 클래스입니다.
/// </summary>
public class FrameworkFunction : ObjectInterface, IAllEventInterface
{
    static EventList EventManager => Display.Target.EventManager;

    //Start와 비슷
    internal static void Prepare(Group group)
    {
        group.Prepare();
    }
    public virtual void Prepare()
    {
        Prepare(Display.Target);
    }

    //Stop, Free 와 비슷
    internal static void Release(Group group)
    {
        group.Release();
    }
    public virtual void Release()
    {
        Release(Display.Target);
    }

    /// <summary>
    /// 'Framework.Run' 함수를 호출시 실행되는 함수입니다.
    /// </summary>
    public override void Start()
    {
        Display.Target.Prepare();
    }
    /// <summary>
    /// 'Framework.Stop' 함수를 호출시 실행되는 함수입니다.
    /// </summary>
    public override void Stop()
    {
        Display.Target.Release();
    }

    /// <summary>
    /// 창의 크기가 조절될경우 실행되는 함수입니다.
    /// (창 크기 조절이 완전히 끝날때 실행되진 않습니다.)
    /// 창이 처음 생성될때는 실행되지 않습니다.
    /// </summary>
    public override void Resize()
    {
        Framework.Positioning(Display.Target);
        Display.Target.Resize();
        Framework.Positioning(Display.Target);
    }

    internal static long endtime = 0;
    /// <summary>
    /// Rendering
    /// </summary>
    internal override void Draw()
    {
        if (endtime > Framework.frametimer.ElapsedTicks)
        {
            if (Framework.SavingPerformance && endtime > Framework.frametimer.ElapsedTicks + 2000)
                SDL.SDL_Delay(1);
            return;
        }

        Update(((updateMs = Framework.frametimer.ElapsedTicks) - updateTime) * 0.0001f);

        Framework.RenderRange = Window.size;
        _ = SDL.SDL_RenderSetViewport(Framework.renderer , ref Window.size);
        Framework.Rendering(Display.Target);

        SDL.SDL_RenderPresent(Framework.renderer);

        _ = SDL.SDL_RenderSetViewport(Framework.renderer , ref Window.size);
        _ = SDL.SDL_SetRenderDrawColor(Framework.renderer , Window.BackgroundColor.Red , Window.BackgroundColor.Green , Window.BackgroundColor.Blue , Window.BackgroundColor.Alpha);
        _ = SDL.SDL_RenderClear(Framework.renderer);
        if (endtime <= Framework.frametimer.ElapsedTicks - Display.framelatelimit)
            endtime = Framework.frametimer.ElapsedTicks + Display.framelatelimit;
        else
            endtime += Display.framelatelimit;
    }

    internal static long updateTime = 0, updateMs = 0;

    public virtual void Update(float ms)
    {
        Display.Target.ResetPosition(new(Window.Width , Window.Height));
        Framework.RenderRange = Window.size;
        Framework.Positioning(Display.Target);
        Display.Target.Update(ms);
        Animation.AnimationQueue.Update();
        Framework.RenderRange = Window.size;
        Framework.Positioning(Display.Target);
        updateTime = updateMs;
    }
    /// <summary>
    /// 창 크기 조절이 완전히 끝날때 호출되는 함수입니다.
    /// </summary>
    public virtual void Resized()
    {
        for (int i = 0 ; i < Display.Target.EventManager.Resized.Count ; i++)
        {
            Display.Target.EventManager.Resized[i].Resized();
        }
    }

    public virtual void InputText()
    {
        //Thread t = new(() =>
        //{
        //    SDL.SDL_Delay(Input.Text.WaitTime);
        //    SDL.SDL_GetKeyboardState(out int r);
        //    SDL.SDL_Keycode key = (SDL.SDL_Keycode)r;
        //    if (key.HasFlag(SDL.SDL_Keycode.SDLK_BACKSPACE))
        //    {
        //        while(Input.Text.InputedText.Length > 0)
        //        {
        //            Input.Text
        //        }
        //    }
        //});
    }

    int winrestore, winmax, winmin;

    public virtual void WindowMaximized()
    {
        int len = Display.Target.EventManager.windowMaximizeds.Count;
        for (winmax=0 ;winmax < len ;winmax++)
        {
            Display.Target.EventManager.windowMaximizeds[winmax].WindowMaximized();
        }
    }
    public virtual void WindowMinimized()
    {
        int len = Display.Target.EventManager.windowMinimizeds.Count;
        for (winmin = 0 ; winmin < len ; winmin++)
        {
            Display.Target.EventManager.windowMinimizeds[winmin].WindowMinimized();
        }
    }
    public virtual void WindowRestore()
    {
        Display.Target.EventManager.windowRestores.ForEach(x => x.WindowRestore());
    }
    /// <summary>
    /// 창 위치가 조정될떄 호출되는 함수입니다.
    /// </summary>
    public virtual void WindowMove()
    {
        EventManager.windowMoves.ForEach(x => x.WindowMove());
    }

    static int iwq;
    /// <summary>
    /// 창 나가기 버튼을 클릭했을때 호출되는 함수입니다.
    /// </summary>
    public virtual void WindowQuit()
    {
        if (Window.FrameworkStopWhenClose)
            Framework.Stop();
    }
    /// <summary>
    /// 파일이 드래그 드롭될때 호출되는 함수입니다.
    /// </summary>
    /// <param name="filename"></param>
    public virtual void DropFile(string filename)
    {
        EventManager.dropFiles.ForEach(x => x.DropFile(filename));
    }

    static int ikd;
    /// <summary>
    /// 키보드의 특정 키가 눌렸을때 실행되는 함수입니다.
    /// </summary>
    /// <param name="e"></param>
    public virtual void KeyDown(Keycode e)
    {
        ikd = Display.Target.EventManager.KeyDown.Count;
        for (int i = 0 ; i < ikd ; i++)
        {
            Display.Target.EventManager.KeyDown[i].KeyDown(e);
        }
    }

    static int imm;
    /// <summary>
    /// 마우스가 움직일때 호출되는 함수입니다.
    /// </summary>
    public virtual void MouseMove()
    {
        SDL.SDL_GetMouseState(out Input.Mouse.position.x , out Input.Mouse.position.y);
        int len = Display.Target.EventManager.mouseMoves.Count;
        for (int i = 0 ; i < len ; i++)
        {
            Display.Target.EventManager.mouseMoves[i].MouseMove();
        }
    }

    static int imd, imu, iku;

    public virtual void MouseKeyDown(MouseKey key)
    {
        int len = Display.Target.EventManager.mouseKeyDowns.Count;
        for (int i = 0 ; i < len ; i++)
        {
            Display.Target.EventManager.mouseKeyDowns[i].MouseKeyDown(key);
        }
    }

    public virtual void MouseKeyUp(MouseKey key)
    {
        int len = Display.Target.EventManager.mouseKeyUps.Count;
        for (int i = 0 ; i < len ; i++)
        {
            Display.Target.EventManager.mouseKeyUps[i].MouseKeyUp(key);
        }
    }

    public virtual void KeyUp(Keycode key)
    {
        iku = Display.Target.EventManager.keyUp.Count;
        for (int i = 0 ; i < iku ; i++)
        {
            Display.Target.EventManager.keyUp[i].KeyUp(key);
        }
    }

    int kfi, kfo;

    public virtual void KeyFocusIn()
    {
        EventManager.keyFocusIns.ForEach(x => x.KeyFocusIn());
    }

    public virtual void KeyFocusOut()
    {
        EventManager.keyFocusOuts.ForEach(x => x.KeyFocusOut());
    }

    int mfi, mfo;

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
            Window.Resize(Display.MonitorWidth , Display.MonitorHeight);
        }
        if (Display.FrameLateLimit == 0)
            Display.FrameLateLimit = 0;
    }
}