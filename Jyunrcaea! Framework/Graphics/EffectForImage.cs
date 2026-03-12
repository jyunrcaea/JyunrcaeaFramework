namespace JyunrcaeaFramework.Graphics;

/// <summary>
/// In-memory image effect helpers.
/// </summary>
class EffectForImage
{
    /// <summary>
    /// Applies a simple 3x3 blur-like convolution to an image.
    /// </summary>
    internal static PaintOnMemory Bluring(ImageOnMemory image)
    {
        if (image.Width == 0 || image.Height == 0)
        {
            return new PaintOnMemory(0, 0);
        }

        // For very small images, neighborhood sampling used by the blur kernel is not valid.
        // In this case, just return an exact copy.
        if (image.Width < 3 || image.Height < 3)
        {
            PaintOnMemory copy = new(image.Width, image.Height);
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    image.GetRGBA(x, y, out byte cr, out byte cg, out byte cb, out byte ca);
                    copy.Point(x, y, cr, cg, cb, ca);
                }
            }

            return copy;
        }

        byte r, g, b, a;
        int maxw = image.Width - 1;
        int maxh = image.Height - 1;

        double[,] redMap = new double[image.Width, image.Height];
        double[,] blueMap = new double[image.Width, image.Height];
        double[,] greenMap = new double[image.Width, image.Height];
        double[,] alphaMap = new double[image.Width, image.Height];

        void Add(int x, int y, double n)
        {
            redMap[x, y] += r * n;
            blueMap[x, y] += b * n;
            greenMap[x, y] += g * n;
            alphaMap[x, y] += a * n;
        }

        void OneBlur(int x, int y)
        {
            image.GetRGBA(x, y, out r, out g, out b, out a);
            Add(x, y, 0.25);
            if (x != 0)
            {
                Add(x - 1, y, 0.125);
                if (y != 0) Add(x - 1, y - 1, 0.0625);
                if (y != maxh) Add(x - 1, y + 1, 0.0625);
            }

            if (y != 0)
            {
                Add(x, y - 1, 0.125);
            }

            if (x != maxw)
            {
                Add(x + 1, y, 0.125);
                if (y != 0) Add(x + 1, y - 1, 0.0625);
                if (y != maxh) Add(x + 1, y + 1, 0.0625);
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

        image.GetRGBA(1, 1, out r, out g, out b, out a);
        Add(0, 0, 0.4375);
        image.GetRGBA(maxw - 1, maxh - 1, out r, out g, out b, out a);
        Add(maxw, maxh, 0.4375);
        image.GetRGBA(1, maxh - 1, out r, out g, out b, out a);
        Add(0, maxh, 0.4375);
        image.GetRGBA(maxw - 1, 1, out r, out g, out b, out a);
        Add(maxw, 0, 0.4375);

        for (int x = 1; x < maxw; x++)
        {
            image.GetRGBA(x, 1, out r, out g, out b, out a);
            Add(x, 0, 0.25);
            image.GetRGBA(x, maxh - 1, out r, out g, out b, out a);
            Add(x, maxh, 0.25);
        }

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
                paint.Point(x, y, (byte)redMap[x, y], (byte)greenMap[x, y], (byte)blueMap[x, y], (byte)alphaMap[x, y]);
            }
        }

        return paint;
    }
}
