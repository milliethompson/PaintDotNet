/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for BmpFileType.
    /// </summary>
    public class BmpFileType
        : GdiPlusFileType
    {
        public BmpFileType()
            : base(PdnResources.GetString("BmpFileType.Name"),
                   ImageFormat.Bmp, 
                   false, 
                   new string[] { ".bmp" })
        {
        }

        public override bool IsReflexive(SaveConfigToken token)
        {
            return false;
        }

        protected override Document OnLoad(Stream input)
        {
            // This allows us to open images that were created in Explorer using New -> Bitmap Image
            // which actually just creates a 0-byte file
            if (input.Length == 0)
            {
                Document newDoc = new Document(800, 600);
                Layer layer = Layer.CreateBackgroundLayer(newDoc.Width, newDoc.Height);
                newDoc.Layers.Add(layer);
                return newDoc;
            }
            else
            {
                return base.OnLoad(input);
            }
        }

        private unsafe void SquishSurfaceTo24Bpp(Surface surface)
        {
            byte *dst = (byte *)surface.GetRowAddress(0);
            int byteWidth = surface.Width * 3;
            int stride24bpp = ((byteWidth + 3) / 4) * 4; // round up to multiple of 4
            int delta = stride24bpp - byteWidth;

            for (int y = 0; y < surface.Height; ++y)
            {
                ColorBgra *src = surface.GetRowAddress(y);
                ColorBgra *srcEnd = src + surface.Width;

                while (src < srcEnd)
                {
                    dst[0] = src->B;
                    dst[1] = src->G;
                    dst[2] = src->R;
                    ++src;
                    dst += 3;
                }

                dst += delta;
            }

            return;
        }

        private unsafe Bitmap CreateAliased24BppBitmap(Surface surface)
        {
            int stride = surface.Width * 3;
            int realStride = ((stride + 3) / 4) * 4; // round up to multiple of 4
            return new Bitmap(surface.Width, surface.Height, realStride, PixelFormat.Format24bppRgb, new IntPtr(surface.Scan0.VoidStar));
        }

        protected override void OnSave(Document input, Stream output, SaveConfigToken token, ProgressEventHandler callback)
        {
            ImageCodecInfo icf = GdiPlusFileType.GetImageCodecInfo(ImageFormat.Bmp);
            EncoderParameters parms = new EncoderParameters(1);
            EncoderParameter parm = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 24); // BMP's should always save as 24-bit
            parms.Param[0] = parm;

            using (Surface surface = new Surface(input.Width, input.Height))
            {
                surface.Clear(ColorBgra.White);

                using (RenderArgs ra = new RenderArgs(surface))
                {
                    input.RenderFlat(ra);
                }

                SquishSurfaceTo24Bpp(surface);
               
                using (Bitmap bitmap = CreateAliased24BppBitmap(surface))
                {
                    GdiPlusFileType.LoadProperties(bitmap, input);
                    bitmap.Save(output, icf, parms);
                }
            }                
        }
    }
}
