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

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for PngFileType.
    /// </summary>
    public class PngFileType
        : GdiPlusFileType
    {
        public PngFileType()
            : base("PNG", ImageFormat.Png, false, new string[] { ".png" })
        {
        }

        public override bool IsReflexive(SaveConfigToken token)
        {
            return true;
        }
    }
}
