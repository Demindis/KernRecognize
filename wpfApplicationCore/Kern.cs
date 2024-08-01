using OpenCvSharp;
using System;



public class Kern
{
    public int CenterX { get; set; }
    public int CenterY { get; set; }
    public int Radius { get; set; }
    public string Number { get; set; }
}

public static class KernParser
{
    public static Kern[] ParseKernsFromCsv(string csvString)
    {
        string[] lines = csvString.Split(new string[] { "\r\n" }, StringSplitOptions.None);
        Kern[] kernArray = new Kern[lines.Length - 2];

        for (int i = 0; i < lines.Length - 2; i++)
        {
            string[] values = lines[i].Split(',');

            Kern kern = new Kern
            {
                CenterX = int.Parse(values[0]),
                CenterY = int.Parse(values[1]),
                Radius = int.Parse(values[2]),
                Number = values[3]
            };

            kernArray[i] = kern;
        }

        return kernArray;
    }
}

public static class Drawing
{
    public static Mat Draw(string imagePath, Kern[] kernArray)
    {
        Mat img = Cv2.ImRead(imagePath);
        int height = img.Rows;
        int thickness = (int)Math.Round(height / 1000.0);

        foreach (Kern kern in kernArray)
        {
            img.Circle(new Point(kern.CenterX, kern.CenterY), kern.Radius, Scalar.Red, thickness);
        }

        foreach (Kern kern in kernArray)
        {
            int baseLine;
            int x = (int)Math.Round((double)(Cv2.GetTextSize(kern.Number, HersheyFonts.HersheySimplex, 0.1 * Math.Round(height / 300.0), thickness, out baseLine).Width / 2));
            Cv2.PutText(img, kern.Number, new Point(kern.CenterX - x, kern.CenterY - kern.Radius), HersheyFonts.HersheySimplex, 0.1 * Math.Round(height / 300.0), Scalar.Blue, thickness);
        }
        return img;
    }
}
class SearchId
{
    private static double Dist(Point center1, Point center2)
    {
        return Math.Sqrt(Math.Pow(center1.X - center2.X, 2) + Math.Pow(center1.Y - center2.Y, 2));
    }

    public static int BoxInResult(int x, int y, Kern[] kernArray)
    {
        for (int i = 0; i < kernArray.Length; i++)
        {
            if (Dist(new Point(x, y), new Point(kernArray[i].CenterX, kernArray[i].CenterY)) < kernArray[i].Radius)
            {
                return i;
            }
        }

        return -1;
    }

}


