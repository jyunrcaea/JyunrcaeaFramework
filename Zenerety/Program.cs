using JyunrcaeaFramework;

namespace Zenerety
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //창 만들기
            Framework.Init("Zenerety", 1600, 900, null, null, new());

            //Zenerety 렌더링 방식 사용하기
            Framework.NewRenderingSolution = true;

            //성능 제한 해제
            //Framework.SavingPerformance = false;

            //프레임 2배
            Display.FrameLateLimit = 120;

            //프레임워크 함수 커스텀
            Framework.Function = new FF();

            //직사각형 객체 만들기
            Box obj1 = new Box
            {
                Size = new ZeneretySize(Window.Width, Window.Height),
            };

            //이미지 객체 만들기
            Image obj2 = new Image
            {
                Texture = new TextureFromFile("Icon.png"),
            };

            //타겟에 추가
            Display.Target.ObjectList.Add(obj1);
            Display.Target.ObjectList.Add(obj2);

            //회전 반복을 위한 함수
            void RotateRepeat(ZeneretyObject zo)
            {
                //애니메이션 큐 추가
                Animation.Add(

                    //회전 애니메이션
                    new Animation.Information.Rotation(

                        // 회전할 대상: 이 애니메이션을 실행한 객체
                        (ZeneretyExtendObject)zo,
                        
                        // 회전값: (시계방향) 360도 회전
                        360,

                        // 시작시간: 현재 실행시간 + 0.5초
                        Framework.RunningTime + 500,

                        // 이동시간: 1초
                        1000,

                        // 애니메이션 종료시 실행될 함수: 지금 이 함수
                        RotateRepeat,

                        // 애니메이션 계산기: Easing 애니메이션
                        Animation.GetAnimation(AnimationType.Easing)

                    )

                );
            }
            //1회 실행
            RotateRepeat(obj2);

            //실행
            Framework.Run();
        }
    }

    //창을 종료할때 나갈수 있도록 하기위한 프레임워크 함수 커스텀
    class FF : FrameworkFunction
    {
        public override void WindowQuit()
        {
            Framework.Stop();
        }
    }
}