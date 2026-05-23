/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class PencilTool
        : Tool 
    {
        private bool mouseDown = false;
        private ColorBgra color;
        private MouseButtons mouseButton;
        private BitmapLayer bitmapLayer;
        private RenderArgs renderArgs;
        private List<Point> tracePoints;
        private List<Rectangle> savedRects;
        private PdnRegion clipRegion;
        private Point lastPoint;
        private Point difference;
        private Cursor pencilToolCursor;
        private BinaryPixelOp blendOp = new UserBlendOps.NormalBlendOp();
        private BinaryPixelOp copyOp = new BinaryPixelOps.AssignFromRhs();

        protected override void OnActivate()
        {
            base.OnActivate();

            this.pencilToolCursor = new Cursor(PdnResources.GetResourceStream("Cursors.PencilToolCursor.cur"));
            this.Cursor = this.pencilToolCursor;

            this.savedRects = new List<Rectangle>();

            if (Workspace.ActiveLayer != null)
            {
                bitmapLayer = (BitmapLayer)Workspace.ActiveLayer;
                renderArgs = new RenderArgs(bitmapLayer.Surface);
                tracePoints = new List<Point>();
            }
            else
            {
                bitmapLayer = null;
                Utility.Dispose(renderArgs);
                renderArgs = null;
            }
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();

            if (this.pencilToolCursor != null)
            {
                this.pencilToolCursor.Dispose();
                this.pencilToolCursor = null;
            }

            if (mouseDown)
            {
                Point lastPoint = (Point)tracePoints[tracePoints.Count - 1];
                OnMouseUp(new MouseEventArgs(mouseButton, 0, lastPoint.X, lastPoint.Y, 0));
            }

            this.savedRects = null;
            this.tracePoints = null;
            this.bitmapLayer = null;

            if (this.renderArgs != null)
            {
                this.renderArgs.Dispose();
                this.renderArgs = null;
            }

            this.mouseDown = false;

            Utility.Dispose(clipRegion);
        }

        // Draws a point, but first intersects it with the selection
        private void DrawPoint(RenderArgs ra, Point p, ColorBgra color)
        {
            if (Utility.IsPointInRectangle(p, ra.Surface.Bounds))
            {
                if (ra.Graphics.IsVisible(p))
                {
                    BinaryPixelOp op = Workspace.Environment.AlphaBlending ? blendOp : copyOp;
                    ra.Surface[p.X, p.Y] = op.Apply(ra.Surface[p.X, p.Y], color);
                }
            }
        }

        private void DrawLines(RenderArgs ra, List<Point> points, int startIndex, int length, ColorBgra color)
        {
            // Draw a point in the line
            if (points.Count == 0)
            {
                return;
            }
            else if (points.Count == 1)
            {
                Point p = (Point)points[0];

                if (Utility.IsPointInRectangle(p, ra.Surface.Bounds))
                {
                    DrawPoint(ra, p, color);
                }
            }
            else
            {
                for (int i = startIndex + 1; i < startIndex + length; ++i)
                {
                    Point[] linePoints = Utility.GetLinePoints(points[i - 1], points[i]);
                    int startPoint = 0;

                    if (i != 1)
                    {
                        startPoint = 1;
                    }

                    for (int pi = startPoint; pi < linePoints.Length; ++pi)
                    {
                        Point p = linePoints[pi];
                        DrawPoint(ra, p, color);
                    }
                }
            }       
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (mouseDown)
            {
                return;
            }

            if (((e.Button & MouseButtons.Left) == MouseButtons.Left) ||
                ((e.Button & MouseButtons.Right) == MouseButtons.Right))
            {
                mouseDown = true;
                mouseButton = e.Button;
                tracePoints = new List<Point>();
                bitmapLayer = (BitmapLayer)Workspace.ActiveLayer;
                renderArgs = new RenderArgs(bitmapLayer.Surface);

                if (clipRegion != null)
                {
                    clipRegion.Dispose();
                    clipRegion = null;
                }

                clipRegion = Workspace.Environment.Selection.CreateRegion();
                renderArgs.Graphics.SetClip(clipRegion.GetRegionReadOnly(), CombineMode.Replace);
                OnMouseMove(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseDown && ((e.Button & mouseButton) != MouseButtons.None))
            {
                Point mouseXY = new Point(e.X, e.Y);

                if (lastPoint == Point.Empty)
                {
                    lastPoint = mouseXY;
                }

                difference = new Point(mouseXY.X - lastPoint.X, mouseXY.Y - lastPoint.Y);

                if (tracePoints.Count > 0) 
                {
                    Point lastMouseXY = (Point)tracePoints[tracePoints.Count - 1];
                    if (lastMouseXY == mouseXY) 
                    {
                        return;
                    }
                }

                if ((mouseButton & MouseButtons.Left) == MouseButtons.Left)
                {
                    this.color = Workspace.Environment.ForeColor;
                }
                else // if ((mouseButton & MouseButtons.Right) == MouseButtons.Right)
                {   
                    // right mouse button = swap foreground/background
                    this.color = Workspace.Environment.BackColor;
                }

                if (!(tracePoints.Count > 0 && mouseXY == (Point)tracePoints[tracePoints.Count - 1]))
                {
                    tracePoints.Add(mouseXY);
                }

                if (Workspace.ActiveLayer is BitmapLayer)
                {
                    Rectangle saveRect;

                    if (tracePoints.Count == 1)
                    {
                        saveRect = Utility.PointsToRectangle(mouseXY, mouseXY);
                    }
                    else
                    {   
                        // >1 points
                        saveRect = Utility.PointsToRectangle((Point)tracePoints[tracePoints.Count - 1], (Point)tracePoints[tracePoints.Count - 2]);
                    }

                    saveRect.Inflate(2, 2);
                    saveRect.Intersect(Workspace.ActiveLayer.Bounds);

                    // drawing outside of the canvas is a no-op, so don't do anything in that case!
                    // also make sure it's within the clipping bounds
                    if (saveRect.Width > 0 && saveRect.Height > 0 && renderArgs.Graphics.IsVisible(saveRect))
                    {
                        SaveRegion(null, saveRect);
                        this.savedRects.Add(saveRect);

                        int startIndex;
                        int length;

                        if (tracePoints.Count == 1)
                        {
                            startIndex = 0;
                            length = 1;
                        }
                        else
                        {
                            startIndex = tracePoints.Count - 2;
                            length = 2;
                        }

                        DrawLines(this.renderArgs, tracePoints, startIndex, length, color);

                        bitmapLayer.Invalidate(saveRect);
                        Update();
                    }
                }
                else
                {
                    // will have to do something here if we add other layer types besides BitmapLayer
                }
                lastPoint = mouseXY;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (mouseDown)
            {
                OnMouseMove(e);
                mouseDown = false;

                if (savedRects.Count > 0)
                {
                    Rectangle[] savedScans = this.savedRects.ToArray();
                    PdnRegion saveMeRegion = Utility.RectanglesToRegion(savedScans);
                    HistoryAction ha = new BitmapHistoryAction(Name, Image, Workspace, Workspace.ActiveLayerIndex,
                        saveMeRegion, ScratchSurface);
                    Workspace.History.PushNewAction(ha);
                    saveMeRegion.Dispose();
                    this.savedRects.Clear();
                    ClearSavedMemory();
                }

                tracePoints = null;
            }
        }

        public PencilTool(DocumentWorkspace parent)
            : base(parent,
                   PdnResources.GetImage("Icons.PencilToolIcon.png"),
                   PdnResources.GetString("PencilTool.Name"),
                   PdnResources.GetString("PencilTool.HelpText"),
                   'p')
        {
            // initialize any state information you need
            mouseDown = false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DisposeImage();
            }
        }

    }
}
