/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
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

        protected override void PerimeterFound(Point[][] polygonSet)
        {
            using (PdnGraphicsPath path = new PdnGraphicsPath())
            {
                path.AddPolygons(polygonSet);

                using (PdnRegion fillRegion = new PdnRegion(path))
                {
                    Rectangle boundingBox = fillRegion.GetBoundsInt();

                    Surface surface = ((BitmapLayer)Workspace.ActiveLayer).Surface;
                    RenderArgs ra = new RenderArgs(surface);
                    HistoryAction ha;

                    using (PdnRegion affected = Utility.SimplifyAndInflateRegion(fillRegion))
                    {
                        ha = new BitmapHistoryAction(Name, Image, Workspace, Workspace.ActiveLayerIndex, affected);
                    }

                    ra.Graphics.CompositingMode = Workspace.Environment.GetCompositingMode();
                    ra.Graphics.FillRegion(brush, fillRegion.GetRegionReadOnly());

                    Workspace.History.PushNewAction(ha);
                    Workspace.ActiveLayer.Invalidate(boundingBox);
                    Update();
                }
            }
        }

        protected override void OnActivate()
        {
            // cursor-transitions
            cursorMouseUp = new Cursor(PdnResources.GetResourceStream("Cursors.PaintBucketToolCursor.cur"));
            cursorMouseDown = new Cursor(PdnResources.GetResourceStream("Cursors.PaintBucketToolCursorMouseDown.cur"));
            Cursor = cursorMouseUp;

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
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

            base.OnDeactivate ();
        }


        public PaintBucketTool(DocumentWorkspace parent) 
            : base(parent,
                   PdnResources.GetImage("Icons.PaintBucketIcon.png"),
                   PdnResources.GetString("PaintBucketTool.Name"),
                   PdnResources.GetString("PaintBucketTool.HelpText"),
                   'f')
        {
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
            }
        }
    }
}
