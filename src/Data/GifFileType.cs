/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Data.Quantize;
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
            : base("GIF", ImageFormat.Gif, false, new string[] { ".gif" }, true)
        {
        }

        protected override SaveConfigToken OnCreateDefaultSaveConfigToken()
        {
            return new GifSaveConfigToken(128, true, 7);
        }

        public override SaveConfigWidget CreateSaveConfigWidget()
        {
            return new GifSaveConfigWidget();
        }

        protected override void OnSave(Document input, System.IO.Stream output, SaveConfigToken token, ProgressEventHandler callback)
        {
            GifSaveConfigToken gsct = (GifSaveConfigToken)token;

            // Flatten and pre-process the image
            using (Surface surface = new Surface(input.Width, input.Height))
            {
                surface.Clear(ColorBgra.FromBgra(255, 255, 255, 0));

                using (RenderArgs ra = new RenderArgs(surface))
                {
                    input.Render(ra, true);
                }

                for (int y = 0; y < surface.Height; ++y)
                {
                    unsafe
                    {
                        ColorBgra* ptr = surface.GetRowAddressUnchecked(y);
                        
                        for (int x = 0; x < surface.Width; ++x)
                        {
                            if (ptr->A < gsct.Threshold)
                            {
                                ptr->Bgra = 0;
                            }
                            else
                            {
                                if (gsct.PreMultiplyAlpha)
                                {
                                    int r = ((ptr->R * ptr->A) + (255 * (255 - ptr->A))) / 255;
                                    int g = ((ptr->G * ptr->A) + (255 * (255 - ptr->A))) / 255;
                                    int b = ((ptr->B * ptr->A) + (255 * (255 - ptr->A))) / 255;
                                    int a = 255;

                                    *ptr = ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, (byte)a);
                                }
                                else
                                {
                                    ptr->Bgra |= 0xff000000;
                                }
                            }

                            ++ptr;
                        }
                    }
                }

                using (Bitmap bitmap = surface.CreateAliasedBitmap(input.Bounds, true))
                {
                    OctreeQuantizer quantizer = new OctreeQuantizer(255, 8);

                    quantizer.DitherLevel = gsct.DitherLevel;

                    using (Bitmap quantized = quantizer.Quantize(bitmap, callback))
                    {
                        quantized.Save(output, ImageFormat.Gif);
                    }
                }
            }
        }
    }
}
