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
    /// Summary description for EraserTool.
    /// </summary>
    public class EraserTool 
        : Tool
    {
        private bool mouseDown;
        private MouseButtons mouseButton;
        private ArrayList savedSurfaces;
        private Point lastMouseXY;
        private RenderArgs renderArgs;
		private BitmapLayer bitmapLayer;
		private Cursor cursorMouseDown;
        private Cursor cursorMouseUp;

        protected override void OnActivate()
        {
            base.OnActivate ();
            
            if (savedSurfaces != null)
            {
                foreach (PlacedSurface ps in savedSurfaces)
                {
                    ps.Dispose();
                }
            }

            savedSurfaces = new ArrayList();

            if (Workspace.ActiveLayer != null)
            {
                bitmapLayer = (BitmapLayer)Workspace.ActiveLayer;
                renderArgs = new RenderArgs(bitmapLayer.Surface);
            }
            else
            {
                bitmapLayer = null;
                renderArgs = null;
            }
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate ();

            if (mouseDown)
            {
                OnMouseUp(new MouseEventArgs(mouseButton, 0, lastMouseXY.X, lastMouseXY.Y, 0));
            }

            if (savedSurfaces != null)
            {
                if (savedSurfaces != null)
                {
                    foreach (PlacedSurface ps in savedSurfaces)
                    {
                        ps.Dispose();
                    }
                }

                savedSurfaces.Clear();
                savedSurfaces = null;
            }

            Utility.Dispose(renderArgs);
            renderArgs = null;

            bitmapLayer = null;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown (e);

			Cursor = cursorMouseDown;

            if (((e.Button & MouseButtons.Left) == MouseButtons.Left) ||
                ((e.Button & MouseButtons.Right) == MouseButtons.Right))
            {
                mouseDown = true;
                mouseButton = e.Button;

                lastMouseXY.X = e.X;
                lastMouseXY.Y = e.Y;

                PdnRegion clipRegion;

                if (!Workspace.Environment.IsSelectionEmpty)
                {
                    clipRegion = Workspace.Environment.CreateSelectedRegion();
                }
                else
                {
                    clipRegion = new PdnRegion();
                    clipRegion.MakeInfinite();
                }

                renderArgs.Graphics.SetClip(clipRegion, CombineMode.Replace);
                clipRegion.Dispose();

                OnMouseMove(e);
            }

        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp (e);

			Cursor = cursorMouseUp;

            if (mouseDown)
            {
                OnMouseMove(e);
                mouseDown = false;

                if (savedSurfaces.Count > 0)
                {
                    PdnRegion saveMeRegion = new PdnRegion();
                    saveMeRegion.MakeEmpty();

                    foreach (PlacedSurface pi1 in savedSurfaces)
                    {
                        saveMeRegion.Union(pi1.Bounds);
                    }

                    PdnRegion simplifiedRegion = Utility.SimplifyAndInflateRegion(saveMeRegion);

                    using (IrregularSurface weDrewThis = new IrregularSurface(renderArgs.Surface, simplifiedRegion))
                    {
                        for (int i = savedSurfaces.Count - 1; i >= 0; --i)
                        {
                            PlacedSurface ps = (PlacedSurface)savedSurfaces[i];
                            ps.Draw(renderArgs.Surface);
                            ps.Dispose();
                        }

                        savedSurfaces.Clear();

                        //HistoryAction ha = bitmapLayer.CreateHistoryAction(Name, Image, simplifiedRegion);
                        HistoryAction ha = new BitmapHistoryAction(Name, Image, Workspace, Workspace.ActiveLayerIndex, simplifiedRegion);
                        weDrewThis.Draw(bitmapLayer.Surface);
                        Workspace.History.PushNewAction(ha);
                    }
                }
            }
        }

        private static void DrawCircleOverLine(Graphics g, Pen pen, Point a, Point b)
        {
            Point[] coords = Utility.GetLinePoints(a, b);
            float halfPenWidth = (pen.Width / 2.0f);
            RectangleF rectBase = new RectangleF(-halfPenWidth, -halfPenWidth, pen.Width, pen.Width);

            foreach (PointF p in coords)
            {
                RectangleF rect = new RectangleF(new PointF(rectBase.X + p.X, rectBase.Y + p.Y), rectBase.Size);
				g.DrawEllipse(pen,rect);
				g.FillEllipse(pen.Brush, rect);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove (e);

            if (mouseDown && ((e.Button & mouseButton) != MouseButtons.None))
            {
                Pen pen = Workspace.Environment.PenInfo.CreatePen(Workspace.Environment.BrushInfo, Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 0, 0));
				pen.Width = pen.Width / 2.0f;

                Point a = lastMouseXY;
                Point b = new Point(e.X, e.Y);

                Rectangle saveRect = Utility.PointsToRectangle(a, b);

                saveRect.Inflate((int)Math.Ceiling(pen.Width), (int)Math.Ceiling(pen.Width));

                if (renderArgs.Graphics.SmoothingMode == SmoothingMode.AntiAlias)
                {
                    saveRect.Inflate(1, 1);
                }

                saveRect.Intersect(Workspace.ActiveLayer.Bounds);

                // drawing outside of the canvas is a no-op, so don't do anything in that case!
                // also make sure we're within the clip region
                if (saveRect.Width > 0 && saveRect.Height > 0 && renderArgs.Graphics.IsVisible(saveRect))
                {
                    PlacedSurface savedPS = new PlacedSurface(renderArgs.Surface, saveRect);
                    savedSurfaces.Add(savedPS);

                    if (Workspace.Environment.AntiAliasing)
                    {
                        renderArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    }
                    else
                    {
                        renderArgs.Graphics.SmoothingMode = SmoothingMode.None;
                    }

                    new UnaryPixelOps.InvertWithAlpha().Apply(renderArgs.Surface, saveRect);
                    renderArgs.Graphics.CompositingMode = CompositingMode.SourceOver;
                    DrawCircleOverLine(renderArgs.Graphics, pen, a, b);
                    new UnaryPixelOps.InvertWithAlpha().Apply(renderArgs.Surface, saveRect);
                    new BinaryPixelOps.SetColorChannels().Apply(renderArgs.Surface, saveRect.Location, savedPS.What, new Point(0, 0), saveRect.Size);

                    bitmapLayer.Invalidate(saveRect);
                    Workspace.Update();
                }

                lastMouseXY = b;
                pen.Dispose();
            }
        }
        
        public EraserTool(DocumentWorkspace parent) 
            : base(parent,
                   Utility.GetImageResource("Icons.EraserToolIcon.bmp"),
                   "Eraser", 
                   "Brush-like erasing tool",
                   "Click and drag to erase a portion of the image",
                   'e')
        {
			// cursor-transitions
			cursorMouseUp = new Cursor(Utility.GetResourceStream("Cursors.EraserToolCursor.cur"));
			cursorMouseDown = new Cursor(Utility.GetResourceStream("Cursors.EraserToolCursorMouseDown.cur"));
			this.Cursor = cursorMouseUp;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DisposeImage();

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
