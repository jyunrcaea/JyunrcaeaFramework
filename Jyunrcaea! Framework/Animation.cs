namespace JyunrcaeaFramework;

/// <summary>
/// 부드러운 움직임을 구현하기 위한 편리한 기능이 모여있습니다.
/// </summary>
public static class Animation
{
    internal class LinkedListForAnimation : LinkedList<Info.General>
    {
        public void Add(Info.General info)
        {
            if (info.EndTime <= Framework.RunningTime)
            {
                info.Done();
                return;
            }
            //this.AddLast(info);
            AddTarget.Enqueue(info);
        }

        Queue<Info.General> AddTarget = new();
        Queue<Info.General> RemoveTarget = new();

        public new void Remove(Info.General info)
        {
            RemoveTarget.Enqueue(info);
        }

        public void Update()
        {
            while (AddTarget.Count != 0) base.AddLast(AddTarget.Dequeue());
            foreach (var a in AnimationQueue) if (a is not null) lock (a) { a.Update(); }
            while(RemoveTarget.Count!=0) base.Remove(RemoveTarget.Dequeue());
        }
    }

    internal static LinkedListForAnimation AnimationQueue = new();

    public static void Add(Info.General info)
    {
        AnimationQueue.Add(info);
    }

    public static void Add(InfoForGroup.General group)
    {
        group.Add();
    }

    /// <summary>
    /// 특정한 애니메이션 정보를 담는 클래스가 모여있습니다.
    /// </summary>
    public static class Info
    {
        /// <summary>
        /// 기본적인 애니메이션 정보를 담는 클래스입니다.
        /// </summary>
        public abstract class General
        {
            public General(BaseObject zo,double? st,double animatime,uint RepeatCount = 1,Action<BaseObject>? fff = null,FunctionForAnimation? ffa = null)
            {
                Target = zo;
                FunctionForFinish = fff;
                this.RepeatCount = RepeatCount;
                if (ffa is not null) AnimationCalculator = ffa;
                StartTime = st is null ? Framework.RunningTime : (double)st;
                AnimationTime = animatime;
            }

            public BaseObject Target { get; internal set; } = null!;
            public double StartTime { get; internal set; }
            public double EndTime { get; internal set; }
            double animatime;
            public double AnimationTime { get => animatime; set
                {
                    animatime = value;
                    this.EndTime = this.StartTime + value;
                }
            }
            public bool Finished { get; internal set; } = false;
            public Action<BaseObject>? FunctionForFinish { get; internal set; } = null;
            public FunctionForAnimation AnimationCalculator { get; internal set; } = Animation.Type.Default;
            public uint RepeatCount = 0;

            internal double Progress = 0d;

            internal bool CheckTime()
            {
                Progress = Framework.RunningTime;
                if (Progress <= StartTime) return true;
                if (Progress >= EndTime)
                {
                    if (RepeatCount != 1)
                    {
                        this.StartTime = EndTime;
                        this.EndTime = StartTime + AnimationTime;
                        if (RepeatCount != 0) RepeatCount--;
                        Progress = AnimationCalculator((Progress - StartTime) / AnimationTime);
                        return false;
                    }
                    Done();
                    return true;
                }
                Progress = AnimationCalculator((Progress - StartTime) / AnimationTime);
                return false;
            }

            internal abstract void Update();
            /// <summary>
            /// 애니메이션을 즉시 마무리 한뒤 끝내버립니다.
            /// </summary>
            public virtual void Done()
            {
                this.Finished = true;
                if (FunctionForFinish is not null) FunctionForFinish(Target);
                AnimationQueue.Remove(this);
            }

            /// <summary>
            /// 애니메이션을 마무리 없이 즉시 종료합니다.
            /// </summary>
            /// <param name="CallFinishFunction">애니메이션 완료시 호출해야될 함수 호출 여부 </param>
            public virtual void Stop(bool CallFinishFunction = false)
            {
                this.Finished = true;
                if (CallFinishFunction && FunctionForFinish is not null) FunctionForFinish(Target);
                AnimationQueue.Remove(this);
            }

            /// <summary>
            /// 애니메이션을 취소합니다. (이전 상태로 되돌립니다.)
            /// </summary>
            public virtual void Undo()
            {
                this.Finished = false;
            }

            /// <summary>
            /// 시간을 수정하여 애니메이션을 재개합니다. (애니메이션이 이미 종료된 상태여야 합니다.)
            /// </summary>
            /// <param name="StartTime">시작 시간</param>
            /// <param name="EndTime">종료 시간</param>
            /// <returns>재개 성공 여부</returns>
            [Obsolete("불안정")]
            public bool ResumeAt(double StartTime,double EndTime)
            {
                if (this.Finished is false) return false;
                this.Finished = false;
                this.StartTime = StartTime;
                this.EndTime = EndTime;
                AnimationQueue.Add(this);
                return true;
            }
        }
        /// <summary>
        /// 움직임과 관련된 정보를 담는 클래스
        /// </summary>
        public class Movement : General
        {
            /// <summary>
            /// 특정 대상을 원하는 (절대적) 위치로 부드럽게 움직입니다.
            /// </summary>
            /// <param name="Target">대상 (모든 Zenerety 렌더링 지원 객체)</param>
            /// <param name="X">이동할 X좌표</param>
            /// <param name="Y">이동할 Y좌표</param>
            /// <param name="StartTime">시작 시간 (null 일경우 현재 프레임워크 실행시간으로 설정 (즉시 시작))</param>
            /// <param name="AnimationTime">이동 시간</param>
            /// <param name="FunctionWhenFinished">완료되었을때 실행할 함수 (null 일경우 아무것도 하지 않음)</param>
            /// <param name="TimeClaculator">애니메이션 계산기 (null 일경우 기본)</param>
            /// <param name="RepeatCount">반복 횟수, 0일경우 무한</param>
            public Movement(BaseObject Target,int? X = null,int? Y = null,double? StartTime = null, double AnimationTime = 1000,uint RepeatCount = 1, FunctionForAnimation? TimeClaculator = null, Action<BaseObject>? FunctionWhenFinished = null) : base(Target,StartTime,AnimationTime,RepeatCount,FunctionWhenFinished,TimeClaculator) {
                BX = Target.X;
                BY = Target.Y;
                if (X is null)
                {
                    MX = false;
                } else
                {
                    MX = true;
                    AX = (int)X;
                    LX = AX - BX;
                }

                if (Y is null)
                {
                    MY = false;
                } else
                {
                    MY = true;
                    AY = (int)Y;
                    LY = AY - BY;
                }
            }

            bool MX, MY;
            int BX, BY, AX, AY, LX, LY;
            internal override void Update()
            {
                if (CheckTime()) return;
                if(MX) Target.X = BX + (int)(LX * Progress);
                if(MY) Target.Y = BY + (int)(LY * Progress);
            }

            
            public override void Done()
            {
                if(MX) Target.X = AX;
                if(MY) Target.Y = AY;
                base.Done();
            }

            public override void Undo()
            {
                if(MX) Target.X = BX;
                if(MY) Target.Y = BY;
                base.Done();
            }

            public void EditEndPoint(int? X,int? Y)
            {
                if (X is null)
                {
                    MX = false;
                }
                else
                {
                    MX = true;
                    AX = (int)X;
                    LX = AX - BX;
                }

                if (Y is null)
                {
                    MY = false;
                }
                else
                {
                    MY = true;
                    AY = (int)Y;
                    LY = AY - BY;
                }
                Update();
            }
        }

        /// <summary>
        /// 회전과 관련된 정보를 담는 클래스
        /// </summary>
        public class Rotation : General
        {
            public Rotation(ExtendDrawableObject Target,double Rotate,double? StartTime,double AnimationTime, uint RepeatCount = 1,  FunctionForAnimation? TimeClaculator = null, Action<BaseObject>? FunctionWhenFinished = null) : base(Target, StartTime, AnimationTime,RepeatCount, FunctionWhenFinished, TimeClaculator)
            {
                BR = Target.Rotation;
                RL = Rotate;
                AR = BR + RL;
            }

            double BR, RL, AR;

            internal override void Update()
            {
                if (CheckTime()) return;
                ((ExtendDrawableObject)Target).Rotation = BR + RL * Progress;
            }

            public override void Done()
            {
                ((ExtendDrawableObject)Target).Rotation = AR;
                base.Done();
            }

            public override void Undo()
            {
                ((ExtendDrawableObject)Target).Rotation = BR;
                base.Undo();
            }
        }

        /// <summary>
        /// 투명도와 관련된 정보를 담는 클래스
        /// </summary>
        public class Opacity : General
        {
            public Opacity(DrawableObject Target, byte TargetOpacity, double? StartTime = null, double AnimationTime = 1000, uint RepeatCount = 1, FunctionForAnimation? TimeCalculator = null, Action<BaseObject>? FunctionWhenFinished = null) : base(Target, StartTime, AnimationTime, RepeatCount, FunctionWhenFinished, TimeCalculator)
            {
                this.BO = Target.Opacity;
                this.AO = TargetOpacity;
                this.RO = (short)(AO - BO);
            }

            byte BO, AO;
            short RO;

            internal override void Update()
            {
                if (CheckTime()) return;
                ((DrawableObject)Target).Opacity = (byte)(BO + RO * Progress);
            }

            public override void Done()
            {
                ((DrawableObject)Target).Opacity = AO;
                base.Done();
            }

            public override void Undo()
            {
                ((DrawableObject)Target).Opacity = BO;
                base.Undo();
            }

            public virtual void Modify(byte opacity)
            {
                if (this.Finished)
                {
                    this.BO = ((Available.Opacity)this.Target).Opacity;
                }
                this.AO = opacity;
                this.RO = (short)(AO - BO);
            }

            public virtual double? ResetStartTime { set
                {
                    this.EndTime = (this.StartTime = value ?? Framework.RunningTime) + this.AnimationTime;
                }
            }
        }
    }

    /// <summary>
    /// 그룹용 애니메이션 정보를 담는 클래스가 모여있습니다.
    /// </summary>
    public static class InfoForGroup
    {
        public abstract class General
        {
            protected ObjectList targets;
            protected double starttime;
            protected double animationtime;

            public bool ApplySubGroup = false;

            /// <summary>
            /// 반복횟수 (기본 1회, 0 일경우 무제한)
            /// </summary>
            public uint RepeatCount = 1;
            /// <summary>
            /// 애니메이션 시간 계산기
            /// </summary>
            public FunctionForAnimation TimeCalculator = Animation.Type.Default;

            protected List<Info.General> ApplyTargets = new();

            public General(Group target, double? StartTime, double AnimationTime) {
                this.targets = target.Objects;
                this.starttime = StartTime ?? Framework.RunningTime;
                this.starttime = AnimationTime;
            }

            /// <summary>
            /// 시작 시간 (null 일경우 Framework.RunningTime 적용)
            /// </summary>
            public double? StartTime
            {
                get => starttime;
                set => starttime = (value ?? Framework.RunningTime);
            }
            /// <summary>
            /// 움직이는 시간 (밀리초 기준)
            /// </summary>
            public double AnimationTime
            {
                get => animationtime;
                set => animationtime = value;
            }

            internal virtual void Add()
            {
                if (this.ApplyTargets.Count != 0)
                {
                    this.ApplyTargets.Clear();
                }
            }

            /// <summary>
            /// 애니메이션을 즉시 마무리 한뒤 끝내버립니다.
            /// </summary>
            public void Done()
            {
                for (int i =0;i<this.ApplyTargets.Count;i++)
                {
                    this.ApplyTargets[i].Done();
                }
            }

            /// <summary>
            /// 애니메이션을 마무리 없이 즉시 종료합니다.
            /// </summary>
            public void Stop()
            {
                for (int i = 0; i < this.ApplyTargets.Count; i++)
                {
                    this.ApplyTargets[i].Stop();
                }
            }

            /// <summary>
            /// 애니메이션이 끝났는지에 대한 여부입니다.
            /// (적용된 객체가 하나도 없을경우 시간 상으로 계산됩니다.)
            /// </summary>
            public bool Finished => this.ApplyTargets.Count == 0 ? (this.starttime + this.animationtime <= Framework.RunningTime) : this.ApplyTargets[0].Finished;
        }

        /// <summary>
        /// 투명도에 대한 애니메이션 정보. (투명도 적용이 불가능한 객체는 제외됩니다.)
        /// </summary>
        public class Opacity : General
        {
            public byte TargetOpacity;

            public Opacity(Group target,byte opacity,double? StartTime=null,double AnimationTime=1000) : base(target,StartTime,AnimationTime)
            {
                this.TargetOpacity = opacity;
            }

            internal override void Add()
            {
                base.Add();
                AddOnGroup(this.targets);
            }

            internal virtual void AddOnGroup(ObjectList targets)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] is not DrawableObject)
                    {
                        if (this.ApplySubGroup && targets[i] is Group) AddOnGroup(((Group)targets[i]).Objects);
                        continue;
                    }
                    var ai = new Info.Opacity((DrawableObject)targets[i], this.TargetOpacity, this.starttime, this.animationtime, this.RepeatCount, this.TimeCalculator);
                    Animation.Add(ai);
                    this.ApplyTargets.Add(ai);
                }
            }
        }
    }

    public static class Type
    {
        public static double Default(double x) => x;

        public static double EaseInSine(double x)
        {
            return 1d - Math.Cos((x * Math.PI) * 0.5d);
        }

        public static double EaseOutSine(double x)
        {
            return Math.Sin((x * Math.PI) * 0.5d);
        }

        public static double EaseInOutSine(double x)
        {
            return -(Math.Cos(Math.PI * x) - 1d) * 0.5d;
        }

        public static double EaseInQuad(double x)
        {
            return x * x;
        }

        public static double EaseOutQuad(double x)
        {
            x = 1d - x;
            return 1 - x * x;
        }

        public static double EaseInOutQuad(double x)
        {
            return x < 0.5d ? 2d * x * x : 1d - Math.Pow(-2d * x + 2d, 2d) * 0.5d;
        }
    }

    public static class Available
    {
        public interface Opacity
        {
            public byte Opacity { get; set; }
        }

        public interface Size
        {
            public Size2D Size { get; set; }
        }


    }
}