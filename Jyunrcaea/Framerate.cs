using JyunrcaeaFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jyunrcaea
{
    internal class Framerate : Canvas
    {
        public Framerate(bool hide = true)
        {
            this.Hide = hide;
        }

        float finishtime = 0;

        LinkedList<float> framelist = new();

        public override void Start()
        {
            base.Start();
        }

        public override void Render()
        {
            float a = Framework.RunningTime;
            float b = a - finishtime;
            this.finishtime = a;
            if (b > 27)
            {
                framelist.AddFirst(-1);
            }
            else framelist.AddFirst(b);

            int i = 0,h;
            foreach (var frame in framelist)
            {
                if (frame == -1)
                {
                    Renderer.Rectangle(
                        1,
                        Window.Height,
                        i,
                        0,
                        255,
                        200,
                        200,
                        150
                        );
                    continue;
                }
                Renderer.Rectangle(
                        1,
                        h = (int)(frame * 10),
                        i,
                        Window.Height - h,
                        200,
                        255,
                        200,
                        150
                    );
                i++;
            }
            if (framelist.Count > Display.MonitorWidth) { framelist.RemoveLast(); }
        }
    }
}
