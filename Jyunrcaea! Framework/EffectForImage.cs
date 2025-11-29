namespace JyunrcaeaFramework;

class EffectForImage
{
    internal static PaintOnMemory Bluring(ImageOnMemory image)
    {
        byte r, g, b, a;
        int maxw = image.Width - 1;
        int maxh = image.Height - 1;

        double[,] RedMap = new double[image.Width, image.Height];
        double[,] BlueMap = new double[image.Width, image.Height];
        double[,] GreenMap = new double[image.Width, image.Height];
        double[,] AlphaMap = new double[image.Width, image.Height];

        void Add(int x, int y, double n)
        {
            RedMap[x, y] += r * n;
            BlueMap[x, y] += b * n;
            GreenMap[x, y] += g * n;
            AlphaMap[x, y] += a * n;
        }

        //1단계
        void OneBlur(int x, int y)
        {
            image.GetRGBA(x, y, out r, out g, out b, out a);
            Add(x, y, 0.25);
            if (x != 0)
            {
                Add(x - 1, y, 0.125);
                if (y != 0)
                {
                    Add(x - 1, y - 1, 0.0625);
                }
                if (y != maxh)
                {
                    Add(x - 1, y + 1, 0.0625);
                }
            }
            if (y != 0)
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

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                OneBlur(x, y);
            }
        }
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
        for (int y = 1; y < maxh; y++)
        {
            image.GetRGBA(1, y, out r, out g, out b, out a);
            Add(0, y, 0.25);
            image.GetRGBA(maxw - 1, y, out r, out g, out b, out a);
            Add(maxw, y, 0.25);
        }

        PaintOnMemory paint = new(image.Width, image.Height);

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                //점찍기
                paint.Point(x, y, (byte)RedMap[x, y], (byte)GreenMap[x, y], (byte)BlueMap[x, y], (byte)AlphaMap[x, y]);
            }
        }

        return paint;
    }
}