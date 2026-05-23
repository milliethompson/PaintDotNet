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
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Allows the user to click on the image to zoom to that location
    /// </summary>
    public class ZoomTool
        : Tool
    {
        private MouseButtons mouseDown;
        private IrregularSurface savedSurface = null;
        private Point downPt, lastPt;
        private Rectangle rect = Rectangle.Empty;
        private Pen pen;
        private RenderArgs renderArgs = null;
        private BitmapLayer bitmapLayer;
        private Cursor cursorZoomIn;
        private Cursor cursorZoomOut;
        private Cursor cursorZoom;
        private Cursor cursorZoomPan;

        public ZoomTool(DocumentWorkspace parent)
            : base(parent,
                   Utility.GetImageResource("Icons.ZoomToolIcon.bmp"),
                   "Zoom",
                   "Zooms in or out on the image.",
                   "Left click to zoom in, right click to zoom out, middle click to slide",
                   'z')
        {
            cursorZoom = new Cursor(Utility.GetResourceStream("Cursors.ZoomToolCursor.cur"));
            cursorZoomIn = new Cursor(Utility.GetResourceStream("Cursors.ZoomInToolCursor.cur"));
            cursorZoomOut = new Cursor(Utility.GetResourceStream("Cursors.ZoomOutToolCursor.cur"));
            cursorZoomPan = new Cursor(Utility.GetResourceStream("Cursors.ZoomPanToolCursor.cur"));

            Cursor = cursorZoom;
            mouseDown = MouseButtons.None;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DisposeImage();

                if (pen != null)
                {
                    pen.Dispose();
                    pen = null;
                }

                if (cursorZoom != null)
                {
                    cursorZoom.Dispose();
                    cursorZoom = null;
                }

                if (cursorZoomIn != null)
                {
                    cursorZoomIn.Dispose();
                    cursorZoomIn = null;
                }

                if (cursorZoomOut != null)
                {
                    cursorZoomOut.Dispose();
                    cursorZoomOut = null;
                }

                if (cursorZoomPan != null)
                {
                    cursorZoomPan.Dispose();
                    cursorZoomPan = null;
                }
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate ();
            
            bitmapLayer = (BitmapLayer)Workspace.ActiveLayer;
            renderArgs = new RenderArgs(bitmapLayer.Surface);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (mouseDown == MouseButtons.None) 
            {
                switch(e.Button) 
                {
                    case MouseButtons.Left:
                        Cursor = cursorZoomIn;
                        break;

                    case MouseButtons.Middle:
                        Cursor = cursorZoomPan;
                        break;

                    case MouseButtons.Right:
                        Cursor = cursorZoomOut;
                        break;
                }

                mouseDown = e.Button;
                lastPt = downPt = new Point(e.X, e.Y);
                OnMouseMove(e);

                if (pen != null)
                {
                    pen.Dispose();
                    pen = null;
                }

                pen = new Pen(Color.Blue, 1.0f);
                pen.Alignment = PenAlignment.Outset;
                pen.LineJoin = LineJoin.Round;
                pen.MiterLimit = 2;
                pen.DashStyle = DashStyle.Dash;
                pen.DashPattern = new float[] { 2, 2 };
                pen.DashOffset = 4.0f;
                pen.Width = 1;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove (e);

            Point thisPt = new Point(e.X, e.Y);

            if ((e.Button == MouseButtons.Left && 
                 mouseDown == MouseButtons.Left && 
                 Utility.Distance(thisPt, downPt) > 10) ||
                 !rect.IsEmpty) //don't undraw the rectangle
            {
                // if they've moved the mouse more than 10 pixels since they clicked
                rect = Utility.PointsToRectangle(downPt, thisPt);
                rect.Intersect(renderArgs.Surface.Bounds);
                UpdateDrawnRect();
            } 
            else if (e.Button == MouseButtons.Middle && mouseDown == MouseButtons.Middle)
            {
                PointF lastScrollPosition = Workspace.DocumentView.DocumentScrollPosition;
                lastScrollPosition.X += thisPt.X - lastPt.X;
                lastScrollPosition.Y += thisPt.Y - lastPt.Y;
                Workspace.DocumentView.DocumentScrollPosition = lastScrollPosition;
                Workspace.DocumentView.Update();
            }
            else
            {
                rect = Rectangle.Empty;
            }

            lastPt = thisPt;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp (e);
            OnMouseMove(e);

            Cursor = Cursors.Arrow;
            Cursor = cursorZoom;

            if (mouseDown == MouseButtons.Left || mouseDown == MouseButtons.Right) 
            {
                Rectangle zoomTo = rect;

                rect = Rectangle.Empty;
                UpdateDrawnRect();

                if (e.Button == MouseButtons.Left) 
                {
                    if (Utility.Magnitude(new PointF(zoomTo.Width, zoomTo.Height)) < 10) 
                    {
                        Workspace.ZoomIn();
                        Workspace.DocumentView.RecenterView(new PointF(e.X, e.Y));
                    } 
                    else
                    {
                        Workspace.ZoomToRectangle(zoomTo);
                    }
                }
                else
                {
                    Workspace.ZoomOut();
                    Workspace.DocumentView.RecenterView(new PointF(e.X, e.Y));
                }
            }

            mouseDown = MouseButtons.None;
        }

        private Rectangle[] EdgesFromRectangle(Rectangle rectangle)
        {
            Rectangle[] edges = new Rectangle[4];

            edges[0] = new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, 1); // top
            edges[1] = new Rectangle(rectangle.Left, rectangle.Top, 1, rectangle.Height); // left
            edges[2] = new Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, 1); // bottom
            edges[3] = new Rectangle(rectangle.Right, rectangle.Top, 1, rectangle.Height); // right

            return edges;
        }

        private void UpdateDrawnRect() 
        {
            if (savedSurface != null) 
            {
                savedSurface.Draw(renderArgs.Surface);
                bitmapLayer.Invalidate(savedSurface.Region);
                savedSurface.Dispose();
                savedSurface = null;
            }

            if (!rect.IsEmpty)
            {
                int extra = (int)Math.Ceiling(2 * pen.Width);

                Rectangle[] edges = EdgesFromRectangle(rect);
                Rectangle[] inflatedEdges = Utility.InflateRectangles(edges, extra);

                for (int i = 0; i < inflatedEdges.Length; ++i)
                {
                    inflatedEdges[i].Intersect(renderArgs.Bounds);
                }

                savedSurface = new IrregularSurface(renderArgs.Surface, inflatedEdges);

                renderArgs.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                renderArgs.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                renderArgs.Graphics.DrawRectangle(pen, rect);

                bitmapLayer.Invalidate(savedSurface.Region);
                Workspace.Update();
            }
        }
    }
}
