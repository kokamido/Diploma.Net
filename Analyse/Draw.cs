using System.Drawing;
using System.Drawing.Imaging;

namespace Analyse
{
    public static class Draw
    {
        public static void Heatmap(double[,] values)
        {
            Bitmap a = new Bitmap(values.GetLength(0),values.GetLength(1));
           // a.LockBits()
            using (Bitmap b = new Bitmap(50, 50)) {
                using (Graphics g = Graphics.FromImage(b)) {
                    g.Clear(Color.Green);
                }
                b.Save(@"C:\green.png", ImageFormat.Png);
            }
        }
    }
}