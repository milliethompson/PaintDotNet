/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet
{
    /// <summary>
    /// Encapsulates a surface that can be copied to the clipboard.
    /// </summary>
    [Serializable]
    public class SurfaceForClipboard
    {
        public IrregularSurface Surface;
        public GraphicsPathWrapper Outline;

        public SurfaceForClipboard(IrregularSurface surface, GraphicsPathWrapper outline)
        {
            this.Surface = surface;
            this.Outline = outline;
        }
    }
}
