using JyunrcaeaFramework;

namespace Zenerety
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /**************************************************************/
            //창 만들기
            Framework.Init("Zenerety", 1600, 900, null, null, new());

            //Zenerety 렌더링 방식 사용하기
            Framework.NewRenderingSolution = true;

            //프레임워크 함수 커스텀
            Framework.Function = new FF();

            //직사각형 객체 만들기
            Box obj1 = new Box
            {
                Size = new ZeneretySize(614, 614),
                X = 10,
                Y = 10,
            };

            Image obj2 = new Image
            {
                Texture = new TextureFromFile("Icon.png"),
                X = 20,
                Y = 20,
            };

            //타겟에 추가
            Display.Target.ObjectList = new()
            {
                obj1,
                obj2
            };

            Animation.Information.Movement animation1 = null!;


            animation1 = Animation.Move(obj1,100,100,3000,5000,null,null);



            //실행
            Framework.Run();
            /**************************************************************/
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