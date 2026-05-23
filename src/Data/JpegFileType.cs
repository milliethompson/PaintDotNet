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
using System.Windows.Forms;

namespace PaintDotNet
{
    // This class handles the layered bitmap format
    public class JpegFileType
        : GdiPlusFileType
    {
        public JpegFileType()
            : base("JPEG", ImageFormat.Jpeg, false, new string[] { ".jpg", ".jpeg", ".jpe", ".jfif" })
        {
        }

        protected override SaveConfigToken OnCreateDefaultSaveConfigToken()
        {
            return new JpegSaveConfigToken(95);
        }

        public override SaveConfigWidget CreateSaveConfigWidget()
        {
            return new JpegSaveConfigWidget();
        }

        protected override void OnSave(Document input, System.IO.Stream output, SaveConfigToken token, ProgressEventHandler callback)
        {
            JpegSaveConfigToken jsct = (JpegSaveConfigToken)token;

            ImageCodecInfo icf = GdiPlusFileType.GetImageCodecInfo(ImageFormat.Jpeg);
            EncoderParameters parms = new EncoderParameters(1);
            EncoderParameter parm = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, jsct.Quality); // force '95% quality'
            parms.Param[0] = parm;

            using (Surface surface = new Surface(input.Width, input.Height))
            {
                surface.Clear(ColorBgra.White);

                using (RenderArgs ra = new RenderArgs(surface))
                {
                    input.Render(ra, true);
                }
                
                using (Bitmap bitmap = surface.CreateAliasedBitmap())
                {
                    GdiPlusFileType.LoadProperties(bitmap, input);
                    bitmap.Save(output, icf, parms);
                }
            }
        }
    }
}