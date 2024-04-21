using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfApplication
{
    internal class Rendering
    {
        static void Rednering(string[] args)
        {
            Mat Draw(string imgPath, (Point center, int radius, string text)[] boxNumber)
            {
                Mat img = Cv2.ImRead(imgPath);
                int height = img.Rows;
                int thickness = (int)Math.Round(height / 1000.0);
                foreach (var (center, radius, text) in boxNumber)
                {
                    img.Circle(center, radius, Scalar.Red, thickness);
                    int baseLine;
                    int x = (int)Math.Round((double)(Cv2.GetTextSize(text, HersheyFonts.HersheySimplex,
                        0.1 * Math.Round(height / 300.0), thickness, out baseLine).Width / 2));
                    img.PutText(text, new Point(center.X - x, center.Y - radius), HersheyFonts.HersheySimplex,
                        0.1 * Math.Round(height / 300.0), Scalar.Blue, thickness);
                }

                return img;
            }

        }
    }
}
