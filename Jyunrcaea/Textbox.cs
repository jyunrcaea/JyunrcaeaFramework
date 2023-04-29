using JyunrcaeaFramework;

namespace Jyunrcaea
{
    public class CustomTextbox : TextboxForAnimation, Events.Resized
    {
        public static string FontFileDirectory = "cache/font.ttf";

        public int PinnedSize;

        public CustomTextbox(int Size=30,string Text = "") : base(FontFileDirectory,Size,Text)
        {
            this.PinnedSize = Size;
        }

        public override void Start()
        {
            base.Start();
            Resized();
        }
        int BeforeSize;

        public override void Resize()
        {
            this.Scale = (PinnedSize * Window.AppropriateSize / (float)this.BeforeSize);
            base.Resize();
        }

        public void Resized()
        {
            this.Scale = 1;
            this.Size = (int)(PinnedSize * Window.AppropriateSize);
            this.BeforeSize = this.Size;
        }
    }
}
