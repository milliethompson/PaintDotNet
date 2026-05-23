/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Resources;
using System.Diagnostics;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for FillTool.
    /// </summary>
    public class PaintBucketTool
        : FloodTool
    {
        private Cursor cursorMouseUp;
        private Cursor cursorMouseDown;
        private Brush brush;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            brush = Workspace.Environment.CreateBrush((e.Button != MouseButtons.Left));
            Cursor = cursorMouseDown;

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            Cursor = cursorMouseUp;
            base.OnMouseUp (e);
        }

        protected override void RegionSelected(PdnRegion fillRegion, Rectangle boundingBox)
        {
            Surface surface = ((BitmapLayer)Workspace.ActiveLayer).Surface;
            RenderArgs ra = new RenderArgs(surface);
            HistoryAction ha;

            using (PdnRegion affected = Utility.SimplifyAndInflateRegion(fillRegion))
            {
                ha = new BitmapHistoryAction(Name, Image, Workspace, Workspace.ActiveLayerIndex, affected);
            }

            ra.Graphics.FillRegion(brush, fillRegion);

            Workspace.History.PushNewAction(ha);
            Workspace.ActiveLayer.Invalidate(boundingBox);
            Workspace.Update();
        }

        public PaintBucketTool(DocumentWorkspace parent) 
            : base(parent,
                   Utility.GetImageResource("Icons.PaintBucketIcon.bmp"),
                   "Paint Bucket",
                   "Fills a Homogenous Color Region",
                   "Left click to fill a region with the foreground color, right click to fill with the background color",
                   'f')
        {
            // cursor-transitions
            cursorMouseUp = new Cursor(Utility.GetResourceStream("Cursors.PaintBucketToolCursor.cur"));
            cursorMouseDown = new Cursor(Utility.GetResourceStream("Cursors.PaintBucketToolCursorMouseDown.cur"));
            Cursor = cursorMouseUp;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DisposeImage();
                
                if (brush != null)
                {
                    brush.Dispose();
                    brush = null;
                }

                if (cursorMouseUp != null)
                {
                    cursorMouseUp.Dispose();
                    cursorMouseUp = null;
                }

                if (cursorMouseDown != null)
                {
                    cursorMouseDown.Dispose();
                    cursorMouseDown = null;
                }
            }
        }
    }
}
