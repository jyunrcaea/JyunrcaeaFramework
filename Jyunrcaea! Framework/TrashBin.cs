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