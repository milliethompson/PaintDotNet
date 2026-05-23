using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

class png2jpg
{
    private static ImageCodecInfo GetImageCodecInfo(ImageFormat format)
    {
        ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

        foreach(ImageCodecInfo icf in encoders)
        {
            if (icf.FormatID == format.Guid)
            {
                return icf;
            }
        }

        return null;
    }
    
    public static int Main(string[] args)
    {
        string[] fileNames = Directory.GetFiles(".", "*.png");

        foreach (string fileName in fileNames)
        {
            string newName = Path.ChangeExtension(fileName, ".jpg");
            Console.WriteLine(fileName + " -> " + newName);

            using (Image pngImage = Image.FromFile(fileName))
            {
                ImageCodecInfo ici = GetImageCodecInfo(ImageFormat.Jpeg);
                EncoderParameters parms = new EncoderParameters(1);
                EncoderParameter parm = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L); // force '90% quality'
                parms.Param[0] = parm;
                pngImage.Save(newName, ici, parms);
            }
        }

        return 0;
    }
}