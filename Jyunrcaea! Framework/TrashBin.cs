/*
            while (Running)
            {
                if (!AsyncRendering)
                {
                    Framework.Function.Draw();
                    while (SDL.SDL_PollEvent(out sdle) == 1)
                    {
                        EventProcess(sdle);
                    }
                } else
                {
                    while (SDL.SDL_PollEvent(out var e) == 1)
                    {
                        EventProcess(e);
                        if (RequestRenderPresent)
                        {
                            RequestRenderPresent = false;
                            SDL.SDL_RenderPresent(renderer);
                        }
                    }
                    if (RequestRenderPresent)
                    {
                        RequestRenderPresent = false;
                        SDL.SDL_RenderPresent(renderer);
                    }
                    Thread.Sleep(1);

                    //while (SDL.SDL_PollEvent(out sdle) == 1)
                    //{
                    //    //if (sdle.type == SDL.SDL_EventType.SDL_WINDOWEVENT && sdle.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED) continue;
                    //    EventProcess(sdle);
                    //}
                    //Thread.Sleep(1);
                    ////if (SDL.SDL_WaitEvent(out sdle) == 0 ) throw new JyunrcaeaFrameworkException(SDL.SDL_GetError());
                    ////EventProcess(sdle);
                }

                #region 이벤트

                //if (EventMultiThreading)
                //{
                //    Queue<Thread> tl = new();
                //    while (SDL.SDL_PollEvent(out var e) == 1)
                //    {
                //        Thread t = new((e) => EventProcess(e)); t.Start(e);
                //        tl.Enqueue(t);
                //    }
                //    while(tl.Count != 0) tl.Dequeue().Join();
                //}
                //else
                //{

                //}

                #endregion
            }
*/

/*
 *             if (AsyncRendering)
            {
                long endtime = Framework.RunningTimeTick;
                while(Running)
                {
                    while (SDL.SDL_PollEvent(out var e) == 1)
                    {
                        AsyncEventProcess(e);
                    }
                    if (endtime <= Framework.RunningTimeTick)
                    {
                        endtime += Display.framelatelimit;
                        SDL.SDL_RenderPresent(renderer);
                        SDL.SDL_SetRenderDrawColor(renderer, Window.BackgroundColor.colorbase.r, Window.BackgroundColor.colorbase.g, Window.BackgroundColor.colorbase.b, Window.BackgroundColor.colorbase.a);
                        SDL.SDL_RenderClear(renderer);
                        if (endtime <= Framework.RunningTimeTick)
                        {
                            endtime = Framework.RunningTimeTick + Display.framelatelimit;
                        }
                    } else Thread.Sleep(1);
                }
            } else
            {
                SDL.SDL_Event e;
                while (Running)
                {
                    Framework.Function.Draw();
                    while (SDL.SDL_PollEvent(out e) == 1) EventProcess(e);
                }
            }
*/

/*
using JyunrcaeaFramework;

internal static double Nothing(double x) => x;

internal static double EaseInSine(double x)
{
    return 1d - Math.Cos((x * Math.PI) * 0.5d);
}

internal static double EaseOutSine(double x)
{
    return Math.Sin((x * Math.PI) * 0.5d);
}

internal static double EaseInOutSine(double x)
{
    return -(Math.Cos(Math.PI * x) - 1d) * 0.5d;
}

internal static double EaseInQuad(double x)
{
    return x * x;
}

internal static double EaseOutQuad(double x)
{
    x = 1d - x;
    return 1 - x * x;
}

internal static double EaseInOutQuad(double x)
{
    return x < 0.5d ? 2d * x * x : 1d - Math.Pow(-2d * x + 2d, 2d) * 0.5d;
}

public static FunctionForAnimation GetAnimation(AnimationType type)
{
    switch (type)
    {
        case AnimationType.Normal:
            return Nothing;
        case AnimationType.Easing:
            return EaseInOutSine;
        case AnimationType.Ease_In:
            return EaseInSine;
        case AnimationType.Ease_Out:
            return EaseOutSine;
        case AnimationType.EaseInQuad:
            return EaseInQuad;
        case AnimationType.EaseOutQuad:
            return EaseOutQuad;
        case AnimationType.EaseInOutQuad:
            return EaseInOutQuad;

    }
    throw new JyunrcaeaFrameworkException("존재하지 않는 애니메이션 타입");
}
*/