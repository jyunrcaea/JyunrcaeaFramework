using JyunrcaeaFramework;

namespace Jyunrcaea.Design
{
    public class ButtonBox : Group, Events.MouseMove, Events.MouseKeyDown, Events.MouseKeyUp, DetailOfObject.Size, Animation.Available.Opacity
    {
        public static class DefaultValue
        {
            public static int Margin = 2;
            public static Color HoverBoxColor = Color.White;
        }

        public ZeneretyDrawableObject? Background
        {
            get => bg; set
            {
                if (bg is not null) this.Objects.Remove(bg);
                if ((bg = value) is not null) this.Objects.Insert(0, bg);
            }
        }
        ZeneretyDrawableObject? bg;

        public byte Opacity { get => (Background is null ? (byte)0 : Background.Opacity); set { if(Background is not null) Background.Opacity = value; } }

        public bool RelativeSize
        {
            get => this.hoverbox.RelativeSize; set
            {
                this.hoverbox.RelativeSize = value;
                if (this.bg is not null) this.bg.RelativeSize = value;
            }
        }

        public int DisplayedWidth => this.hoverbox.DisplayedWidth;
        public int DisplayedHeight => this.hoverbox.DisplayedHeight;

        Box hoverbox;
        public Color HoverColor { get => this.hoverbox.Color; set => this.hoverbox.Color = value; }

        public int Margin
        {
            get => mg; set
            {
                this.hoverbox.Size.Height = th + (mg = value) * 2;
            }
        }
        public int Height
        {
            get => th;
            set
            {
                this.hoverbox.Size.Height = (th=mg) + mg * 2;
            }
        }
        public int Width
        {
            get => this.hoverbox.Size.Width;
            set => this.hoverbox.Size.Width = value;
        }

        int mg = 0;
        int th;

        /// <summary>
        /// 버튼을 생성합니다.
        /// </summary>
        /// <param name="content">버튼에 표시할 문자</param>
        /// <param name="width">버튼의 너비</param>
        /// <param name="height">글자의 높이</param>
        /// <param name="margin">글자와 간격</param>
        public ButtonBox(int width = 0, int height = 0, int? margin = null)
        {
            //this.bg = DefaultValue.Background is null ? null : DefaultValue.Background();
            this.hoverbox = new Box(width, (th = height) + (mg = (margin ?? DefaultValue.Margin)) * 2, DefaultValue.HoverBoxColor.Copy);
            this.AnimationInfo = new(this.hoverbox, 0, null, this.AnimationTime, 1, Animation.Type.EaseInSine);
            if (this.Background is not null) this.Objects.Add(this.Background); this.Objects.Add(hoverbox);
        }

        public Animation.Info.Opacity AnimationInfo { get; internal set; }
        public double AnimationTime = 100;
        public byte OpacityOnHover = 100;

        bool nowhover = false;
        public bool IsHover { get; internal set; } = false;

        public override void Prepare()
        {
            base.Prepare();
            this.AnimationInfo.Done();
        }

        /// <summary>
        /// 마우스가 버튼을 눌렀을때 호출되는 함수.
        /// </summary>
        public virtual void MouseClick()
        {

        }

        /// <summary>
        /// 마우스 포인터가 버튼에 닿았을때 호출되는 함수.
        /// </summary>
        public virtual void MouseOver()
        {
            HoverAnimating(this.OpacityOnHover);
        }

        internal virtual void HoverAnimating(byte opacity)
        {
            if (!this.AnimationInfo.Finished) this.AnimationInfo.Stop();
            this.AnimationInfo.Modify(opacity);
            this.AnimationInfo.ResetStartTime = null;
            Animation.Add(this.AnimationInfo);
        }

        /// <summary>
        /// 마우스 포인터가 버튼 밖으로 나갔을때 호출되는 함수.
        /// </summary>
        public virtual void MouseOut()
        {
            HoverAnimating(0);
        }

        public virtual void MouseMove()
        {
            IsHover = Convenience.MouseOver(this.hoverbox);
            if (nowhover != IsHover)
            {
                if (nowhover = IsHover)
                {
                    this.MouseOver();
                }
                else
                {
                    this.MouseOut();
                }
            }
        }

        bool ispress = false;

        public virtual void MouseKeyDown(Input.Mouse.Key k)
        {
            if (k != Input.Mouse.Key.Left) return;
            if (this.nowhover) this.ispress = true;
        }

        public virtual void MouseKeyUp(Input.Mouse.Key k)
        {
            if (k != Input.Mouse.Key.Left) return;
            if (this.ispress)
            {
                this.ispress = false;
                if (nowhover)
                {
                    this.MouseClick();
                }
            }
        }
    }

    /// <summary>
    /// 호버링 기능이 존재하는 평범한 버튼
    /// </summary>
    public class TextButton : Group, Events.MouseMove, Events.MouseKeyDown, Events.MouseKeyUp, DetailOfObject.Size
    {
        public static class DefaultValue
        {
            //public delegate ZeneretyDrawableObject BackgroundGenerator();

            public static int Margin = 2;
            public static Color? TextColor = null;
            public static Color HoverBoxColor = Color.White;
        }

        public ZeneretyDrawableObject? Background {
            get => bg; set
            {
                if (bg is not null) this.Objects.Remove(bg);
                if ((bg = value) is not null) this.Objects.Insert(0, bg);
            }
        }
        ZeneretyDrawableObject? bg;
        public Text Text { get; internal set; }

        public bool RelativeSize { get => this.hoverbox.RelativeSize; set
            {
                this.hoverbox.RelativeSize = this.Text.RelativeSize = value;
                if (this.bg is not null) this.bg.RelativeSize = value;
            }
        }

        public int DisplayedWidth => this.hoverbox.DisplayedWidth;
        public int DisplayedHeight => this.hoverbox.DisplayedHeight;

        Box hoverbox;
        public Color HoverColor { get => this.hoverbox.Color; set => this.hoverbox.Color = value; }

        public int Margin
        {
            get => mg; set
            {
                this.hoverbox.Size.Height = th + (mg = value) * 2;
            }
        }
        public int TextHeight
        {
            get => th;
            set
            {
                this.Text.FontSize = (th = value);
                this.hoverbox.Size.Height = th + mg * 2;
            }
        }
        public int Width
        {
            get => this.hoverbox.Size.Width;
            set => this.hoverbox.Size.Width = value;
        }
        int mg = 0;
        int th;
        public string Content { get => Text.Content; set => Text.Content = value; }

        /// <summary>
        /// 버튼을 생성합니다.
        /// </summary>
        /// <param name="content">버튼에 표시할 문자</param>
        /// <param name="width">버튼의 너비</param>
        /// <param name="height">글자의 높이</param>
        /// <param name="margin">글자와 간격</param>
        public TextButton(string content,int width=0,int height=0,int? margin = null)
        {
            //this.bg = DefaultValue.Background is null ? null : DefaultValue.Background();
            this.hoverbox = new Box(width, (th = height) + (mg = (margin ?? DefaultValue.Margin)) * 2, DefaultValue.HoverBoxColor.Copy);
            this.Text = new Text(content,height, DefaultValue.TextColor == null ? null : DefaultValue.TextColor.Copy);
            this.AnimationInfo = new(this.hoverbox,0,null,this.AnimationTime,1,Animation.Type.EaseInSine);
            if(this.Background is not null) this.Objects.Add(this.Background);this.Objects.Add(hoverbox);this.Objects.Add(Text);
        }

        public Animation.Info.Opacity AnimationInfo { get; internal set; }
        public double AnimationTime = 100;
        public byte OpacityOnHover = 100;

        bool nowhover = false;
        public bool IsHover { get; internal set; } = false;

        public override void Prepare()
        {
            base.Prepare();
            this.AnimationInfo.Done();
        }

        /// <summary>
        /// 마우스가 버튼을 눌렀을때 호출되는 함수.
        /// </summary>
        public virtual void MouseClick()
        {
            
        }

        /// <summary>
        /// 마우스 포인터가 버튼에 닿았을때 호출되는 함수.
        /// </summary>
        public virtual void MouseOver()
        {
            HoverAnimating(this.OpacityOnHover);
        }

        internal virtual void HoverAnimating(byte opacity)
        {
            if (!this.AnimationInfo.Finished) this.AnimationInfo.Stop();
            this.AnimationInfo.Modify(opacity);
            this.AnimationInfo.ResetStartTime = null;
            Animation.Add(this.AnimationInfo);
        }

        /// <summary>
        /// 마우스 포인터가 버튼 밖으로 나갔을때 호출되는 함수.
        /// </summary>
        public virtual void MouseOut()
        {
            HoverAnimating(0);
        }

        public virtual void MouseMove()
        {
            IsHover = Convenience.MouseOver(this.hoverbox);
            if (nowhover != IsHover)
            {
                if (nowhover = IsHover)
                {
                    this.MouseOver();
                }else
                {
                    this.MouseOut();
                }
            }
        }

        bool ispress = false;

        public virtual void MouseKeyDown(Input.Mouse.Key k)
        {
            if (k != Input.Mouse.Key.Left) return;
            if (this.nowhover) this.ispress = true;
        }

        public virtual void MouseKeyUp(Input.Mouse.Key k)
        {
            if (k != Input.Mouse.Key.Left) return;
            if (this.ispress)
            {
                this.ispress = false;
                if (nowhover)
                {
                    this.MouseClick();
                }
            }
        }
    }

    /// <summary>
    /// 하나만 선택 가능한 여러 버튼 목록
    /// </summary>
    [Obsolete("개발중")]
    public class OptionSelector : Group, Events.MouseMove, Events.MouseKeyDown, Events.MouseKeyUp, Animation.Available.Size
    {
        public List<Text> Labels { get; internal set; }
        public Box Select;
        public GhostBox backspace;

        int mg = 0;
        bool refresh = false;

        public OptionSelector(bool relativesize = true,ushort width=100,ushort size = 0,Color? textcolor = null,int margin = 0,string? fontpath=null,params string[] titles)
        {
            mg = margin;
            backspace = new(width, size);
            Labels = new(titles.Length);
            for (int i = 0; i < titles.Length; i++) Labels[i] = new(titles[i], size, textcolor, fontpath) { RelativeSize = relativesize };
            Select = new(width, size) { RelativeSize = relativesize };

            this.Objects.Add(Select); this.Objects.AddRange(Labels);
        }



        /// <summary>
        /// 너비와 높이를 같이 조정합니다.
        /// </summary>
        public ZeneretySize Size { get => new(this.backspace.Size.Width, this.backspace.Size.Height);
            set {
                this.Width = value.Width;
                this.Height = value.Height;
            }
        }
        public int Width
        {
            get => this.backspace.Size.Width;
            set
            {
                this.Select.Size.Width = this.backspace.Size.Width = value;
            }
        }
        public int Height
        {
            get => this.Select.Size.Height - mg * 2;
            set
            {
                this.Select.Size.Height = value + mg * 2;
                this.backspace.Size.Height = value + mg * Labels.Count * 2;
                for (int i = 0; i < Labels.Count; i++) Labels[i].FontSize = value;
                refresh = true;
            }
        }
        public int Margin
        {
            get => mg;
            set
            {
                this.backspace.Size.Height = this.Height + value * Labels.Count * 2;
                this.Select.Size.Height = this.Height + (mg = value * 2);
                refresh = true;
            }
        }

        public override void Update(float ms)
        {
            if (this.refresh)
            {
                this.refresh = false;
                int len = this.Select.Size.Height;
                int backsize = backspace.Size.Height / 2;
                float start = this.Labels.Count * 0.5f - 0.5f,end = -start;
                for(int i=0;i<this.Labels.Count;i++)
                {
                    this.Labels[i].Y = (int)Math.Round(backsize - len * start);
                    start -= 1f;
                }
            }
            base.Update(ms);
        }

        public int SelectedIndex { get; internal set; } = -1;
        int beforeindex = 0;

        Animation.Info.Movement movement;

        internal virtual void Animating()
        {

        }

        public virtual void MouseMove()
        {
            if (this.backspace.MouseOver) SelectedIndex = -1;
            else
            {
                int backsize = backspace.Size.Height / 2;
                for (int i=0;i<this.Labels.Count;i++)
                {
                    if (Input.Mouse.Y >= this.Labels[i].Y -  backsize)
                    {
                        SelectedIndex = i;
                        break;
                    }
                }
            }


        }
        public virtual void MouseKeyDown(Input.Mouse.Key k) { }
        public virtual void MouseKeyUp(Input.Mouse.Key k) { }
    }


    public class VerticalList : Group, Events.Resized
    {
        public static int DefaultMargin = 5;

        public VerticalList(int? Margin = null,params ZeneretyObject[] targets) : base()
        {
            this.mg = Margin?? DefaultMargin;
            this.Objects.AddRange(targets);
        }

        public int Margin { get => mg; set
            {
                mg = value;
                arrange = true;
            }
        }
        int mg;
        bool arrange = true;
        public virtual void ArrangeList()
        {
            arrange = false;
            if (this.Objects.Count == 0) return;
            this.Objects[0].Y = 0;
            for(int i=1;i<this.Objects.Count;i++)
            {
                if (this.Objects[i - 1] is not DetailOfObject.Size) continue;
                this.Objects[i].Y = this.Objects[i - 1].Y + mg + ((DetailOfObject.Size)this.Objects[i-1]).DisplayedHeight;
            }
            if (this.dx == HorizontalPositionType.Middle) return;
            for (int i =0;i<this.Objects.Count;i++)
            {
                if (this.Objects[i] is not DetailOfObject.Size) continue;
                this.Objects[i].X = (int)( ((DetailOfObject.Size)this.Objects[i]).DisplayedWidth * (this.dx == HorizontalPositionType.Right ? 0.5 : -0.5) );
            }
        }

        /// <summary>
        /// 리스트 내 객체들이 차지하는 높이. 객체가 존재하지 않으면 null이 반환됩니다.
        /// </summary>
        public int? ContentHeight => this.Objects.Count == 0 ? null : this.Objects[this.Objects.Count-1].Y;

        //public HorizontalPositionType ApplyDrawXAll { set
        //    {
        //        for (int i = 0; i < this.Objects.Count; i++)
        //        {
        //            if (this.Objects[i] is not ZeneretyDrawableObject) return;
        //            ((ZeneretyDrawableObject)this.Objects[i]).DrawX = value;
        //        }
        //    }
        //}

        HorizontalPositionType dx = HorizontalPositionType.Middle;

        public HorizontalPositionType DrawX { get => dx; set { dx = value; arrange = true; } }

        public override void Update(float ms)   
        {
            base.Update(ms);
            if (arrange)
            {
                this.ArrangeList();
            }
        }

        public override void Resize()
        {
            base.Resize();
            arrange = true;
        }

        public virtual void Resized()
        {
            arrange = true;
        }
    }
}
