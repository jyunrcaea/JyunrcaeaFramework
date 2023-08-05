using JyunrcaeaFramework;

namespace Jyunrcaea.Intro
{
    public class Control
    {
        public static Scene scene = null!;

        public static Animation.InfoForGroup.Opacity ai=null!;

        public static void Disappear()
        {
            ai = new(scene, 0, null, 250);
            ai.TimeCalculator = Animation.Type.EaseInSine;
            Animation.Add(ai);
        }
    }

    public class Scene : Group
    {
        Box background = new(Window.Width,Window.Height,Color.Black);
        Text Welcome = new Text("This is Jyunrcaea!",34,Color.White);

        public Scene()
        {
            background.RelativeSize = false;

            this.Objects.AddRange(
                background,
                Welcome
            );

            Control.scene = this;
        }

        public override void Prepare()
        {
            base.Prepare();
        }

        public override void Resize()
        {
            base.Resize();
            this.background.Size.Width = Window.Width;
            this.background.Size.Height = Window.Height;
        }

        public override void Update(float ms)
        {
            base.Update(ms);
            if (Welcome.Opacity == 0)
            {
                this.Parent!.Objects.Remove(this);
            }
        }
    }


}
