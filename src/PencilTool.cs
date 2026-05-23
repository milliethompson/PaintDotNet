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
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for PencilTool.
    /// </summary>
    public class PencilTool
        : Tool 
    {
        private bool mouseDown = false;
        private ColorBgra color;
        private MouseButtons mouseButton;
        private ArrayList savedSurfaces;
        private BitmapLayer bitmapLayer;
        private RenderArgs renderArgs;
        private ArrayList tracePoints;
        private PdnRegion clipRegion;
        private Point lastPoint;
        private Point difference;
        private Cursor pencilToolCursor;
        private BinaryPixelOp blendOp = new BinaryPixelOps.AlphaBlend();
        private BinaryPixelOp copyOp = new BinaryPixelOps.AssignFromRhs();

        protected override void OnActivate()
        {
            base.OnActivate();

            this.pencilToolCursor = new Cursor(PdnResources.GetResourceStream("Cursors.PencilToolCursor.cur"));
            this.Cursor = this.pencilToolCursor;

            savedSurfaces = new ArrayList();

            if (Workspace.ActiveLayer != null)
            {
                bitmapLayer = (BitmapLayer)Workspace.ActiveLayer;
                renderArgs = new RenderArgs(bitmapLayer.Surface);
                tracePoints = new ArrayList();
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

            if (savedSurfaces != null)
            {
                foreach (PlacedSurface ps in savedSurfaces)
                {
                    ps.Dispose();
                }

                savedSurfaces.Clear();
                savedSurfaces = null;
            }

            tracePoints = null;
            bitmapLayer = null;

            if (renderArgs != null)
            {
                renderArgs.Dispose();
                renderArgs = null;
            }

            mouseDown = false;

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

        private void DrawLines(RenderArgs ra, ArrayList points, int startIndex, int length, ColorBgra color)
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
                for (int i = 1; i < points.Count; ++i)
                {
                    Point[] linePoints = Utility.GetLinePoints((Point)points[i - 1], (Point)points[i]);
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
                tracePoints = new ArrayList();
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

                    saveRect.Inflate(2,2);
                    saveRect.Intersect(Workspace.ActiveLayer.Bounds);

                    // drawing outside of the canvas is a no-op, so don't do anything in that case!
                    // also make sure it's within the clipping bounds
                    if (saveRect.Width > 0 && saveRect.Height > 0 && renderArgs.Graphics.IsVisible(saveRect))
                    {
                        PlacedSurface savedPI = new PlacedSurface(renderArgs.Surface, saveRect);
                        savedSurfaces.Add(savedPI);

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

                if (savedSurfaces.Count > 0)
                {
                    PdnRegion saveMeRegion = new PdnRegion();
                    saveMeRegion.MakeEmpty();

                    foreach (PlacedSurface pi1 in savedSurfaces)
                    {
                        saveMeRegion.Union(pi1.Bounds);
                    }

                    PdnRegion simplifiedRegion = Utility.SimplifyAndInflateRegion(saveMeRegion);

                    // draw in *reverse* order: that's why we don't use foreach
                    for (int i = savedSurfaces.Count - 1; i >= 0; --i)
                    {
                        PlacedSurface pi = (PlacedSurface)savedSurfaces[i];
                        pi.Draw(renderArgs.Surface);
                        pi.Dispose();
                    }

                    savedSurfaces.Clear();

                    HistoryAction ha = new BitmapHistoryAction(Name, Image, Workspace, Workspace.ActiveLayerIndex, simplifiedRegion);
                    DrawLines(renderArgs, tracePoints, 0, tracePoints.Count, color);
                    bitmapLayer.Invalidate(simplifiedRegion);
                    Workspace.History.PushNewAction(ha);

                    simplifiedRegion.Dispose();
                    saveMeRegion.Dispose();
                }

                tracePoints = null;
            }
        }

        public PencilTool(DocumentWorkspace parent)
            : base(parent,
                   PdnResources.GetImage("Icons.PencilToolIcon.bmp"),
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
