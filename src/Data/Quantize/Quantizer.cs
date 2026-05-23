// Source: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnaspp/html/colorquant.asp

/* 
  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF 
  ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
  THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
  PARTICULAR PURPOSE. 
  
    This is sample code and is freely distributable. 
*/ 

using PaintDotNet;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PaintDotNet.Data.Quantize
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    internal unsafe abstract class Quantizer
    {
        /// <summary>
        /// Flag used to indicate whether a single pass or two passes are needed for quantization.
        /// </summary>
        private bool _singlePass;
        
        protected bool highquality;
        public bool HighQuality 
        {
            get 
            {
                return highquality;
            }

            set 
            {
                highquality = value;
            }
        }

        protected int ditherLevel;
        public int DitherLevel
        {
            get
            {
                return this.ditherLevel;
            }

            set
            {
                this.ditherLevel = value;
            }
        }

        /// <summary>
        /// Construct the quantizer
        /// </summary>
        /// <param name="singlePass">If true, the quantization only needs to loop through the source pixels once</param>
        /// <remarks>
        /// If you construct this class with a true value for singlePass, then the code will, when quantizing your image,
        /// only call the 'QuantizeImage' function. If two passes are required, the code will call 'InitialQuantizeImage'
        /// and then 'QuantizeImage'.
        /// </remarks>
        public Quantizer(bool singlePass)
        {
            _singlePass = singlePass;
        }

        /// <summary>
        /// Quantize an image and return the resulting output bitmap
        /// </summary>
        /// <param name="source">The image to quantize</param>
        /// <returns>A quantized version of the image</returns>
        public Bitmap Quantize(Image source, ProgressEventHandler progressCallback)
        {
            // Get the size of the source image
            int height = source.Height;
            int width = source.Width;

            // And construct a rectangle from these dimensions
            Rectangle bounds = new Rectangle(0, 0, width, height);

            // First off take a 32bpp copy of the image
            Bitmap copy;
            
            if (source is Bitmap && source.PixelFormat == PixelFormat.Format32bppArgb)
            {
                copy = (Bitmap)source;
            }
            else
            {
                copy = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                // Now lock the bitmap into memory
                using (Graphics g = Graphics.FromImage(copy))
                {
                    g.PageUnit = GraphicsUnit.Pixel;

                    // Draw the source image onto the copy bitmap,
                    // which will effect a widening as appropriate.
                    g.DrawImage(source, 0, 0, bounds.Width, bounds.Height);
                }
            }

            // And construct an 8bpp version
            Bitmap output = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            // Define a pointer to the bitmap data
            BitmapData sourceData = null;

            try
            {
                // Get the source image bits and lock into memory
                sourceData = copy.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                // Call the FirstPass function if not a single pass algorithm.
                // For something like an octree quantizer, this will run through
                // all image pixels, build a data structure, and create a palette.
                if (!_singlePass)
                {
                    FirstPass(sourceData, width, height, progressCallback);
                }

                // Then set the color palette on the output bitmap. I'm passing in the current palette 
                // as there's no way to construct a new, empty palette.
                output.Palette = this.GetPalette(output.Palette);

                // Then call the second pass which actually does the conversion
                SecondPass(sourceData, output, width, height, bounds, progressCallback);
            }

            finally
            {
                // Ensure that the bits are unlocked
                copy.UnlockBits(sourceData);
            }

            if (copy != source)
            {
                copy.Dispose();
            }

            // Last but not least, return the output bitmap
            return output;
        }

        /// <summary>
        /// Execute the first pass through the pixels in the image
        /// </summary>
        /// <param name="sourceData">The source data</param>
        /// <param name="width">The width in pixels of the image</param>
        /// <param name="height">The height in pixels of the image</param>
        protected virtual void FirstPass(BitmapData sourceData, int width, int height, ProgressEventHandler progressCallback)
        {
            // Define the source data pointers. The source row is a byte to
            // keep addition of the stride value easier (as this is in bytes)
            byte* pSourceRow = (byte*)sourceData.Scan0.ToPointer();
            Int32* pSourcePixel;

            // Loop through each row
            for (int row = 0; row < height; row++)
            {
                // Set the source pixel to the first pixel in this row
                pSourcePixel = (Int32*)pSourceRow;

                // And loop through each column
                for (int col = 0; col < width; col++, pSourcePixel++)
                {
                    InitialQuantizePixel((ColorBgra *)pSourcePixel);
                }

                // Add the stride to the source row
                pSourceRow += sourceData.Stride;

                if (progressCallback != null)
                {
                    progressCallback(this, new ProgressEventArgs(100.0 * (((double)(row + 1) / (double)height) / 2.0)));
                }
            }
        }

        /// <summary>
        /// Execute a second pass through the bitmap
        /// </summary>
        /// <param name="sourceData">The source bitmap, locked into memory</param>
        /// <param name="output">The output bitmap</param>
        /// <param name="width">The width in pixels of the image</param>
        /// <param name="height">The height in pixels of the image</param>
        /// <param name="bounds">The bounding rectangle</param>
        protected virtual void SecondPass(BitmapData sourceData, Bitmap output, int width, int height, Rectangle bounds, ProgressEventHandler progressCallback)
        {
            BitmapData outputData = null;
            Color[] pallete = output.Palette.Entries;
            int weight = ditherLevel;

            try
            {
                // Lock the output bitmap into memory
                outputData = output.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

                // Define the source data pointers. The source row is a byte to
                // keep addition of the stride value easier (as this is in bytes)
                byte* pSourceRow = (byte *)sourceData.Scan0.ToPointer();
                Int32* pSourcePixel = (Int32 *)pSourceRow;

                // Now define the destination data pointers
                byte* pDestinationRow = (byte *)outputData.Scan0.ToPointer();
                byte* pDestinationPixel = pDestinationRow;

                // And convert the first pixel, so that I have values going into the loop
                byte pixelValue = QuantizePixel((ColorBgra *)pSourcePixel);

                // Assign the value of the first pixel
                *pDestinationPixel = pixelValue;

                // Loop through each row
                int[] errorLastRowR = new int[width];
                int[] errorLastRowG = new int[width];
                int[] errorLastRowB = new int[width];

                errorLastRowB.Initialize();
                errorLastRowG.Initialize();
                errorLastRowR.Initialize();

                for (int row = 0; row < height; row++)
                {
                    // Set the source pixel to the first pixel in this row
                    pSourcePixel = (Int32*)pSourceRow;

                    // And set the destination pixel pointer to the first pixel in the row
                    pDestinationPixel = pDestinationRow;

                    int errorB = 0;
                    int errorG = 0;
                    int errorR = 0;

                    // Loop through each pixel on this scan line
                    for (int col = 0; col < width; col++, pSourcePixel++, pDestinationPixel++)
                    {
                        // Check if this is the same as the last pixel. If so use that value
                        // rather than calculating it again. This is an inexpensive optimisation.

                        // Quantize the pixel
                        ColorBgra *srcPixel = (ColorBgra *)pSourcePixel;
                        ColorBgra target = new ColorBgra();

                        target.B = Utility.ClampToByte(srcPixel->B - errorB * weight / 8);
                        target.G = Utility.ClampToByte(srcPixel->G - errorG * weight / 8);
                        target.R = Utility.ClampToByte(srcPixel->R - errorR * weight / 8);
                        target.A = srcPixel->A;

                        pixelValue = QuantizePixel(&target);
                    
                        ColorBgra actual = ColorBgra.FromColor(pallete[pixelValue]);

                        errorLastRowB[col] = errorB;
                        errorLastRowG[col] = errorG;
                        errorLastRowB[col] = errorR;
                        errorR = actual.R - target.R;
                        errorG = actual.G - target.G;
                        errorB = actual.B - target.B; 

                        // And set the pixel in the output
                        *pDestinationPixel = pixelValue;
                    }

                    // Add the stride to the source row
                    pSourceRow += sourceData.Stride;

                    // And to the destination row
                    pDestinationRow += outputData.Stride;

                    if (progressCallback != null)
                    {
                        progressCallback(this, new ProgressEventArgs(100.0 * (0.5 + ((double)(row + 1) / (double)height) / 2.0)));
                    }
                }
            }
            
            finally
            {
                // Ensure that I unlock the output bits
                output.UnlockBits(outputData);
            }
        }

        /// <summary>
        /// Override this to process the pixel in the first pass of the algorithm
        /// </summary>
        /// <param name="pixel">The pixel to quantize</param>
        /// <remarks>
        /// This function need only be overridden if your quantize algorithm needs two passes,
        /// such as an Octree quantizer.
        /// </remarks>
        protected virtual void InitialQuantizePixel(ColorBgra *pixel)
        {
        }

        /// <summary>
        /// Override this to process the pixel in the second pass of the algorithm
        /// </summary>
        /// <param name="pixel">The pixel to quantize</param>
        /// <returns>The quantized value</returns>
        protected abstract byte QuantizePixel(ColorBgra *pixel);

        /// <summary>
        /// Retrieve the palette for the quantized image
        /// </summary>
        /// <param name="original">Any old palette, this is overrwritten</param>
        /// <returns>The new color palette</returns>
        protected abstract ColorPalette GetPalette(ColorPalette original);
    }
}
