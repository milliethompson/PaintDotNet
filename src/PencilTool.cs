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
		private MouseButtons mouseButton;
        private ArrayList savedSurfaces;
        private BitmapLayer bitmapLayer;
		private RenderArgs renderArgs;
        private ArrayList tracePoints;
        private Pen pen;

		protected override void OnActivate()
		{
			base.OnActivate();

            savedSurfaces = new ArrayList();

            if (Workspace.ActiveLayer != null)
            {
                bitmapLayer = (BitmapLayer)Workspace.ActiveLayer;
				renderArgs = new RenderArgs(bitmapLayer.Surface);
                tracePoints = new ArrayList();
                pen = null;
            }
            else
            {
                bitmapLayer = null;
				Utility.Dispose(renderArgs);
				renderArgs = null;
                pen = null;
            }
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();

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
			Utility.Dispose(renderArgs);
			renderArgs = null;
            mouseDown = false;
            Utility.Dispose(pen);
            pen = null;
		}

        private void DrawLines(RenderArgs ra, ArrayList points, int startIndex, int length, Pen pen, bool antiAliasing)
        {
            if (points.Count == 0)
            {
                return;
            }
            else
                if (points.Count == 1)
            {
                Point p = (Point)points[0];

                if (Utility.IsPointInRectangle(p, ra.Surface.Bounds))
                {
                    ra.Surface[p.X, p.Y] = new BinaryPixelOps.AlphaBlend().Apply(ra.Surface[p.X, p.Y], ColorBgra.FromColor(pen.Color));
                }
            }
            else
            {
                if (antiAliasing)
                {
                    ra.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    ra.Graphics.CompositingMode = CompositingMode.SourceOver;

                    Point[] points2 = new Point[length];
                    points.CopyTo(startIndex, points2, 0, length);
                    ra.Graphics.DrawLines(pen, points2);
                }
                else
                {
                    BinaryPixelOp blender = new BinaryPixelOps.AlphaBlend();
                    ColorBgra color = ColorBgra.FromColor(pen.Color);

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

                            if (Utility.IsPointInRectangle(p, ra.Surface.Bounds))
                            {
                                ra.Surface[p.X, p.Y] = blender.Apply(ra.Surface[p.X, p.Y], color);
                            }
                        }
                    }
                }
            }
        }

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

            if (mouseDown && ((e.Button & mouseButton) != MouseButtons.None))
            {
                if (pen == null)
                {
                    PenInfo pi = Workspace.Environment.PenInfo;
                    pi.Width = 1.0f;

                    if ((mouseButton & MouseButtons.Left) == MouseButtons.Left)
                    {
                        pen = pi.CreatePen(new BrushInfo(BrushType.Solid, HatchStyle.BackwardDiagonal),
                            Workspace.Environment.ForeColor.ToColor(), Workspace.Environment.BackColor.ToColor());
                    }
                    else if ((mouseButton & MouseButtons.Right) == MouseButtons.Right)
                    {   // right mouse button = swap foreground/background
                        pen = pi.CreatePen(new BrushInfo(BrushType.Solid, HatchStyle.BackwardDiagonal),
                            Workspace.Environment.BackColor.ToColor(), Workspace.Environment.ForeColor.ToColor());
                    }
                }

                Point mouseXY = new Point(e.X, e.Y);

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
                    // >1 points
                    {
                        saveRect = Utility.PointsToRectangle((Point)tracePoints[tracePoints.Count - 1], (Point)tracePoints[tracePoints.Count - 2]);
                    }

                    saveRect.Inflate((int)Math.Ceiling(pen.Width), (int)Math.Ceiling(pen.Width));

                    if (renderArgs.Graphics.SmoothingMode == SmoothingMode.AntiAlias)
                    {
                        saveRect.Inflate(1, 1);
                    }

                    saveRect.Intersect(Workspace.ActiveLayer.Bounds);

					// drawing outside of the canvas is a no-op, so don't do anything in that case!
                    // also make sure it's within the clipping bounds
                    if (saveRect.Width > 0 && saveRect.Height > 0 && renderArgs.Graphics.Clip.IsVisible(saveRect))
                    {
						pen.LineJoin = LineJoin.Round;
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

                        DrawLines(this.renderArgs, tracePoints, startIndex, length, pen, Workspace.Environment.AntiAliasing);

                        bitmapLayer.Invalidate(saveRect);
						Workspace.Update();
                    }
                }
                else
                {
                    // TODO throw exception?
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

				Region clipRegion;

				if (!Workspace.Environment.IsSelectionEmpty)
				{
					clipRegion = Workspace.Environment.CreateSelectedRegion();
				}
				else
				{
					clipRegion = new Region();
					clipRegion.MakeInfinite();
				}

				renderArgs.Graphics.SetClip(clipRegion, CombineMode.Replace);
				clipRegion.Dispose();
				OnMouseMove(e);
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
                    Region saveMeRegion = new Region();
                    saveMeRegion.MakeEmpty();

                    foreach (PlacedSurface pi1 in savedSurfaces)
                    {
                        saveMeRegion.Union(pi1.Bounds);
                    }

                    Region simplifiedRegion = Utility.SimplifyAndInflateRegion(saveMeRegion);

                    //using (IrregularSurface weDrewThis = new IrregularSurface(renderArgs.Surface, simplifiedRegion))
                    {
                        for (int i = savedSurfaces.Count - 1; i >= 0; --i)
                        {
                            PlacedSurface pi = (PlacedSurface)savedSurfaces[i];
                            pi.Draw(renderArgs.Surface);
                            pi.Dispose();
                        }

                        savedSurfaces.Clear();

                        HistoryAction ha = bitmapLayer.CreateHistoryAction(Name, Image, simplifiedRegion);
                        //weDrewThis.Draw(renderArgs.Surface);
                        DrawLines(renderArgs, tracePoints, 0, tracePoints.Count, pen, Workspace.Environment.AntiAliasing);
                        bitmapLayer.Invalidate(simplifiedRegion);
                        Workspace.History.PushNewAction(ha);
                    }

                    simplifiedRegion.Dispose();
                    saveMeRegion.Dispose();
                }

                tracePoints = null;
                Utility.Dispose(pen);
                pen = null;
            }
        }

		public PencilTool(DocumentWorkspace parent)
			: base(parent)
		{
            toolBarImage = Utility.GetImageResource("Icons.PencilToolIcon.bmp");
            cursor = new Cursor(Utility.GetResourceStream("Cursors.PencilToolCursor.cur"));
            name = "Pencil";
            description = "Draws a freeform, one-pixel wide line.";

			// initialize any state information you need
            mouseDown = false;
		}
	}
}
