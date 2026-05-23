/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
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
            : base("Bitmap (BMP)", ImageFormat.Bmp, false, new string[] { ".bmp" })
        {
		}

        public override bool IsReflexive(SaveConfigToken token)
        {
            return false;
        }

        public override Document Load(Stream input)
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
                return base.Load(input);
            }
        }


        public override void Save(Document input, Stream output, SaveConfigToken token)
        {
            using (Surface surface = new Surface(input.Width, input.Height))
            {
                surface.Clear(ColorBgra.White);

                using (RenderArgs ra = new RenderArgs(surface))
                {
                    input.RenderFlat(ra);
                }

                using (Bitmap bitmap = surface.CreateAliasedBitmap(input.Bounds, false))
                {
                    bitmap.Save(output, ImageFormat.Bmp);
                }
            }    
        }	
    }
}
