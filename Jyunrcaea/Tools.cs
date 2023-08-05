using JyunrcaeaFramework;

namespace Jyunrcaea.Tools
{
    class StatusScene : Group
    {
        public StatusScene()
        {
            this.Objects.Add(new FrameAnalyze());
        }
    }

    class FrameAnalyze : Text
    {
        public FrameAnalyze() : base("측정중...", 20)
        {
            this.CenterX = 0;
            this.CenterY = 1;
            this.DrawX = HorizontalPositionType.Right;
            this.DrawY = VerticalPositionType.Top;
        }

        int framecount = 0;

        uint endtime = 1000;

        public override void Update(float ms)
        {
            if (endtime <= Framework.RunningTime)
            {
                endtime += 1000;
                this.Content = "FPS: " + framecount;
                framecount = 0;
            }
            framecount++;
            base.Update(ms);
        }
    }
}
