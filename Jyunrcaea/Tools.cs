using JyunrcaeaFramework;

namespace Jyunrcaea.Tools
{
    public class StatusScene : Group
    {
        private FrameAnalyze analyze;
        
        public StatusScene()
        {
            this.Objects.Add(analyze= new FrameAnalyze());
        }

        public override void Prepare()
        {
            base.Prepare();
            this.Disable();
        }

        public void Enable()
        {
            this.Hide = false;
            analyze.endtime = (uint)Framework.RunningTime + 1000;
            this.analyze.Content = "측정중...";
            this.Resize();
        }

        public void Disable()
        {
            this.Hide = true;
        }

        public override void Update(float ms)
        {
            if (this.Hide) return;
            base.Update(ms);
        }
    }

    class FrameAnalyze : Text
    {
        public FrameAnalyze() : base("측정중...", 20,Color.Silver)
        {
            this.CenterX = 0;
            this.CenterY = 1;
            this.DrawX = HorizontalPositionType.Right;
            this.DrawY = VerticalPositionType.Top;
        }

        int framecount;

        public uint endtime = 1000;

        public override void Update(float ms)
        {
            //너무 시간차가 심하면
            if (endtime + 2000 < Framework.RunningTime)
            {
                this.Content = $"({Math.Round((Framework.RunningTime - endtime)*0.001,1)}초 지연 발생)";
                framecount = 0;
                endtime = (uint)Framework.RunningTime + 1000;
            }
            else if (endtime <= Framework.RunningTime)
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
