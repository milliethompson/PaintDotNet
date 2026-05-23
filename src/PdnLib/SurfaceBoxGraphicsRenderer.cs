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

namespace PaintDotNet
{
    /// <summary>
    /// This class handles rendering something to a SurfaceBox via a Graphics context.
    /// </summary>
    public abstract class SurfaceBoxGraphicsRenderer
        : SurfaceBoxRenderer
    {
        public SurfaceBoxGraphicsRenderer(SurfaceBoxRendererList ownerList)
            : base(ownerList)
        {
        }
        
        public abstract void RenderToGraphics(Graphics g, Point offset);

        public virtual bool ShouldRender()
        {
            return true;
        }

        public override sealed void Render(Surface dst, Point offset)
        {
            if (ShouldRender())
            {
                using (RenderArgs ra = new RenderArgs(dst))
                {
                    RenderToGraphics(ra.Graphics, offset);
                }
            }
        }
    }
}
