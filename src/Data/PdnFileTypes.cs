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

namespace PaintDotNet
{
	/// <summary>
	/// This is the default Paint.NET FileTypeFactory.
	/// </summary>
	public class PdnFileTypes
        : IFileTypeFactory
	{
        public static readonly FileType Bmp = new BmpFileType();
        public static readonly FileType Jpeg = new JpegFileType();
        public static readonly FileType Gif = new GifFileType();
        public static readonly FileType Tiff = new GdiPlusFileType("TIFF", ImageFormat.Tiff, false, new string[] { ".tif", ".tiff" });
        public static readonly FileType Png = new PngFileType();
        public static readonly FileType Pdn = new PdnFileType();

        private static FileType[] fileTypes = new FileType[] { 
                                                                 Bmp, 
                                                                 Pdn, 
                                                                 Jpeg, 
                                                                 Png, 
                                                                 Tiff, 
                                                                 Gif 
                                                             };

        internal FileTypeCollection GetFileTypeCollection()
        {
            return new FileTypeCollection(fileTypes);
        }

        public FileType[] GetFileTypeInstances()
        {
            return (FileType[])fileTypes.Clone();
        }
	}
}
