/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using ImageManipulation;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for GifFileType.
	/// </summary>
	public class GifFileType
        : GdiPlusFileType
	{
		public GifFileType()
            : base("GIF", ImageFormat.Gif, false, new string[] { ".gif" })
        {
		}

        public override void Save(Document input, System.IO.Stream output, SaveConfigToken token)
        {
            using (Surface surface = new Surface(input.Width, input.Height))
            {
                surface.Clear(ColorBgra.White);

                using (RenderArgs ra = new RenderArgs(surface))
                {
                    input.RenderFlat(ra);
                }

                using (Bitmap bitmap = surface.CreateAliasedBitmap(input.Bounds, true))
                {
                    OctreeQuantizer quantizer = new OctreeQuantizer(255, 8);

                    using (Bitmap quantized = quantizer.Quantize(bitmap))
                    {
                        quantized.Save(output, ImageFormat.Gif);
                    }
                }
            }
        }
	}
}
