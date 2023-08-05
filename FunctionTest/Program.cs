using JyunrcaeaFramework;

namespace FunctionTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //기본
            Framework.Init("Jyunrcaea! Framework", 1000, 500);
            Framework.NewRenderingSolution = true;
            Window.Resizable = false;
            //블러 테스트
            ImageOnMemory img = new("paimon.png");
            //img.Blur();
            //img.Resize(img.Width / 4, img.Height / 4);
            //img.Blur();
            //img.Resize(img.Width * 2, img.Height *2);
            //img.Blur();
            //img.Resize(img.Width * 2, img.Height * 2);
            //img.Blur();
            img.Resize(img.Width/4, img.Height/4);
            img.Resize(img.Width * 4, img.Height * 4);
            //객체 추가
            Display.Target.Objects.Add(new Image("paimon.png") { X = -img.Width/2});
            Display.Target.Objects.Add(new Image(img.GetTexture()) { X= img.Width/2 });
            //실행
            Framework.Run();
        }

        static PaintOnMemory Bluring(ImageOnMemory image,byte level = 1)
        {
            byte r, g, b, a;
            int maxw = image.Width - 1;
            int maxh = image.Height - 1;

            double[,] RedMap = new double[image.Width,image.Height];
            double[,] BlueMap = new double[image.Width, image.Height];
            double[,] GreenMap = new double[image.Width, image.Height];
            double[,] AlphaMap = new double[image.Width, image.Height];

            void Add(int x,int y,double n)
            {
                RedMap[x, y] += r * n;
                BlueMap[x, y] += b * n;
                GreenMap[x, y] += g * n;
                AlphaMap[x,y] += a * n;
            }

            //1단계
            void OneBlur(int x,int y)
            {
                image.GetRGBA(x, y,out r,out g,out b,out a);
                Add(x, y, 0.25);
                if (x != 0)
                {
                    Add(x - 1, y, 0.125);
                    if (y!=0)
                    {
                        Add(x - 1, y - 1, 0.0625);
                    }
                    if (y != maxh)
                    {
                        Add(x - 1, y + 1, 0.0625);
                    }
                }
                if (y!=0)
                {
                    Add(x, y - 1, 0.125);
                }
                if (x != maxw)
                {
                    Add(x + 1, y, 0.125);
                    if (y != 0)
                    {
                        Add(x + 1, y - 1, 0.0625);
                    }
                    if (y != maxh)
                    {
                        Add(x + 1, y + 1, 0.0625);
                    }
                }
                if (y != maxh)
                {
                    Add(x, y + 1, 0.125);
                }
            }

            //2단계
            void TwoBlur(int x,int y)
            {
                const double center = 0.1501831501831502;
                const double one = 0.0952380952380952;
                const double two = 0.0586080586080586;
                const double three = 0.0256410256410256;
                const double four = 0.0146520146520147;
                const double five = 0.0036630036630037;

                image.GetRGBA(x, y, out r, out g, out b, out a);
                Add(x, y, center);
                //one
                if (x != 0) Add(x - 1, y, one); //left
                if (y != 0) Add(x, y - 1, one); //top
                if (x != maxw) Add(x + 1, y, one); // right
                if (y != maxh) Add(x, y + 1, one); //bottom
                //two
                if (x != 0) //left
                {
                    if (y != 0) Add(x - 1, y - 1, two);//top
                    if (y != maxh) Add(x-1,y+1,two);//bottom
                }
                if (x != maxw)
                {
                    if (y != 0) Add(x + 1, y - 1, two); //top
                    if (y != maxh) Add(x+1, y+1, two); //bottom
                }
                //three
                if (x > 1) Add(x - 2, y, three); //left
                if (x < maxw - 1) Add(x + 2, y, three); //right
                if (y > 1) Add(x, y - 2, three); //top
                if (y < maxh -1 ) Add(x, y + 2, three); //bottom
                //four
                if (x != 0) //left
                {
                    if (y > 1) Add(x - 1, y - 2, four); //top2
                    if (y < maxh - 1) Add(x - 1, y + 2, four); //bottom2
                    if (x != 1) //left
                    {
                        if (y !=0 ) Add(x - 2, y - 1, four); //top
                        if (y != maxh) Add(x - 2, y + 1, four);//bottom
                    }
                }
                if (x != maxw) //right
                {
                    if (y > 1) Add(x + 1, y - 2, four); //top2
                    if (y < maxh - 1) Add(x + 1, y + 2, four); //bottom2
                    if (x != maxw-1) //right
                    {
                        if (y != 0) Add(x + 2, y - 1, four); //top
                        if (y != maxh) Add(x + 2, y + 1, four);//bottom
                    }
                }
                //five
                if (x > 1) //left
                {
                    if (y > 1) Add(x - 2, y - 2, five); //top
                    if (y < maxh - 1) Add(x - 2, y + 2, five); //bottom
                }
                if (x < maxw - 1) //right
                {
                    if (y > 1) Add(x + 2, y - 2, five); //top
                    if (y < maxh - 1) Add(x + 2, y + 2, five); //bottom
                }
            }

            for (int x=0; x < image.Width; x++)
            {
                for (int y =0; y < image.Height;y++)
                {
                    if (level == 2) TwoBlur(x, y); else OneBlur(x, y);
                }
            }

            if (level == 1)
            {
                //가장자리 처리
                image.GetRGBA(1, 1, out r, out g, out b, out a);
                Add(0, 0, 0.4375);
                image.GetRGBA(maxw - 1, maxh - 1, out r, out g, out b, out a);
                Add(maxw, maxh, 0.4375);
                image.GetRGBA(1, maxh - 1, out r, out g, out b, out a);
                Add(0, maxh, 0.4375);
                image.GetRGBA(maxw - 1, 1, out r, out g, out b, out a);
                Add(maxw, 0, 0.4375);

                //가로 처리
                for (int x = 1; x < maxw; x++)
                {
                    image.GetRGBA(x, 1, out r, out g, out b, out a);
                    Add(x, 0, 0.25);
                    image.GetRGBA(x, maxh - 1, out r, out g, out b, out a);
                    Add(x, maxh, 0.25);
                }
                //세로 처리
                for (int y = 1; y < maxw; y++)
                {
                    image.GetRGBA(1, y, out r, out g, out b, out a);
                    Add(0, y, 0.25);
                    image.GetRGBA(maxw - 1, y, out r, out g, out b, out a);
                    Add(maxw, y, 0.25);
                }
            }

            PaintOnMemory paint = new(image.Width, image.Height);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    //점찍기
                    paint.Point(x,y,(byte)RedMap[x,y],(byte)GreenMap[x, y],(byte)BlueMap[x, y],(byte)AlphaMap[x, y]);
                }
            }

            return paint;
        }

        public static double sigma = 1.1614;

        static PaintOnMemory GayBluring(ImageOnMemory image,byte size=1)
        {
            if (size == 0)
            {
                throw new Exception("?");
            }

            //int sizelen = size * 2 + 1;
            //int kernel_len = size * 2 + 1;
            var ksize = size * 2 + 1;
            sigma = 0.3 * ((ksize - 1) * 0.5 - 1) + 0.8;
            Console.WriteLine(sigma);
                
            byte r, g, b, a;
            int maxw = image.Width - 1;
            int maxh = image.Height - 1;

            double[,] RedMap = new double[image.Width, image.Height];
            double[,] BlueMap = new double[image.Width, image.Height];
            double[,] GreenMap = new double[image.Width, image.Height];
            double[,] AlphaMap = new double[image.Width, image.Height];

            double[,] RatioMap = new double[ksize, ksize];
            double AppropriateRatio = 0;
            for (int w = -size; w <= size; w++)
            {
                for (int h = -size; h <= size; h++)
                {
                    AppropriateRatio += (RatioMap[w+size, h+size] = G(w, h));
                }
            }
            AppropriateRatio = 1d / AppropriateRatio;
            double test = 0;
            for (int w = -size; w <= size; w++)
            {
                for (int h = -size; h <= size; h++)
                {
                    RatioMap[w+size, h+size] *= AppropriateRatio;
                    test += RatioMap[w+size, h+size];
                    Console.WriteLine("({0},{1}) : {2}", w, h, RatioMap[w + size, h + size]);
                }
            }
            Console.WriteLine(test);

            void GetColor(int x,int y)
            {
                image.GetRGBA(x, y, out r, out g, out b, out a);
            }

            void Add(int x, int y, double n)
            {
                RedMap[x, y] += r * n;
                BlueMap[x, y] += b * n;
                GreenMap[x, y] += g * n;
                AlphaMap[x, y] += a * n;
            }

            void One(int x, int y)
            {
                GetColor(x, y);
                int tx, ty;
                for (int w = -size; w <= size; w++)
                {
                    tx = x + w;
                    if (tx < 0 || tx > maxw) continue;
                    for (int h = -size; h <= size; h++)
                    {
                        ty = y + h;
                        if (ty < 0 || ty > maxh) continue;
                        Add(tx,ty, RatioMap[w + size, h + size]);
                    }
                }
            }

            for (int x= 0; x < image.Width;x++)
            {
                for (int y= 0; y < image.Height;y++)
                {
                    One(x, y);
                }
            }

            PaintOnMemory result = new(image.Width,image.Height);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    result.Point(x, y, (byte)RedMap[x,y], (byte)GreenMap[x, y], (byte)BlueMap[x, y], (byte)AlphaMap[x, y]);
                }
            }

            return result;
        }

        static double G(int x ,int y)
        {
            return (1d / (2d * Math.PI * Math.Pow(sigma, 2))) * Math.Exp(-(Math.Pow(x,2) + Math.Pow(y,2)) / 2 * Math.Pow(sigma,2));
        }
    }
}