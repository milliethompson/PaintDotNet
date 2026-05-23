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
	/// Summary description for PaintBrushTool.
	/// </summary>
	public class PaintBrushTool
		: Tool 
	{
		private bool mouseDown;
		private Brush brush = Brushes.Pink;
		private MouseButtons mouseButton;
		private ArrayList savedSurfaces;
		private PointF lastMouseXY;
		private PointF lastNorm;
		private PointF lastDir;
        private RenderArgs renderArgs;
		private BitmapLayer bitmapLayer;
		private Cursor cursorMouseDown, cursorMouseUp;

		public override char HotKey
		{
			get
			{
				return 'b';
			}
		}

		protected override bool SupportsInk
		{
			get
			{
				return true;
			}
		}

		protected override void OnActivate()
		{
			base.OnActivate();

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
			base.OnDeactivate();

			if (mouseDown)
			{
				OnStylusUp(new StylusEventArgs(mouseButton, 0, lastMouseXY.X, lastMouseXY.Y, 0));
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

            if (renderArgs != null)
            {
                renderArgs.Dispose();
                renderArgs = null;
            }

			bitmapLayer = null;
		}

		private float GetWidth(float Pressure) 
		{
			return Pressure * Pressure * Workspace.Environment.PenInfo.Width * 0.5f;
		}

		protected override void OnStylusDown(StylusEventArgs e)
		{
			base.OnStylusDown(e);

			if (mouseDown)
			{
				return;
			}

			Cursor = cursorMouseDown;

			if (((e.Button & MouseButtons.Left) == MouseButtons.Left) ||
                ((e.Button & MouseButtons.Right) == MouseButtons.Right))
            {
				mouseButton = e.Button;
				if ((mouseButton & MouseButtons.Left) == MouseButtons.Left)
				{
					brush = Workspace.Environment.CreateBrush(false);
				}
				else// if ((mouseButton & MouseButtons.Right) == MouseButtons.Right)
				{
					brush = Workspace.Environment.CreateBrush(true);
				}
				mouseDown = true;
				mouseButton = e.Button;

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
				this.OnStylusMove(new StylusEventArgs(e.Button, e.Clicks, e.fX + 0.01f, e.fY, e.Delta, e.Pressure));
			}
		}

		private PointF [] MakePolygon(PointF a, PointF b, PointF c, PointF d) 
		{
			PointF dirA = new PointF(a.X - b.X, a.Y - b.Y);
			PointF dirB = new PointF(c.X - d.X, c.Y - d.Y);
			//Swap points as necessary to keep the polygon winding one direction
			if (dirA.X * dirB.X + dirA.Y * dirB.Y > 0)
			{
				return new PointF [] {a, b, d, c};
			} 
			else
			{	
				return new PointF [] {a, b, c, d};
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (e.Button != MouseButtons.None) 
			{
				/*This is done so that if drawing falls behind due to a
				 * large queue of stylus inputs, it won't do any updates
				 * until it's done. This is accomplished by only updating
				 * when a MouseMove is caught */
				Workspace.Update();
			}
			base.OnMouseMove (e);
		}

        protected override void OnStylusMove(StylusEventArgs e)
        {
            base.OnStylusMove(e);
			PointF currMouseXY = new PointF(e.fX, e.fY);

			if (mouseDown && ((e.Button & mouseButton) != MouseButtons.None))
			{
				if (lastMouseXY == currMouseXY)
				{
					return;
				}

				float pressure = GetWidth(e.Pressure), length;
                PointF a = lastMouseXY;
                PointF b = currMouseXY;
				PointF dir = new PointF(b.X - a.X, b.Y - a.Y);
				PointF norm;
				PointF [] poly = new PointF[4];

				//save direction before normalizing
				lastDir = dir;

				//normalize
				length = Utility.Magnitude(dir);
				dir.X /= length;
				dir.Y /= length;
				
				//compute normal vector, calculate perpendicular offest from stroke for width
				norm = new PointF(dir.Y, -dir.X);
				norm.X *= pressure;
				norm.Y *= pressure;

				a.X -= dir.X * 0.1666f;
				a.Y -= dir.Y * 0.1666f;

				poly = MakePolygon(
					new PointF(a.X - lastNorm.X, a.Y - lastNorm.Y),
					new PointF(a.X + lastNorm.X, a.Y + lastNorm.Y),
					new PointF(b.X + norm.X, b.Y + norm.Y),
					new PointF(b.X - norm.X, b.Y - norm.Y));

				lastNorm = norm;
				lastMouseXY = currMouseXY;				

                if (Workspace.ActiveLayer is BitmapLayer)
                {
					RectangleF dotRect = Utility.RectFromCenter(currMouseXY, Math.Max((pressure - 0.5f), 0.0f));
					RectangleF saveRect = RectangleF.Union(
						dotRect,
						RectangleF.Union(
							Utility.PointsToRectangle(poly[0], poly[1]),
							Utility.PointsToRectangle(poly[2], poly[3])));

                    if (renderArgs.Graphics.SmoothingMode == SmoothingMode.AntiAlias)
                    {
                        saveRect.Inflate(1.0f, 1.0f);
                    }

                    saveRect.Intersect(Workspace.ActiveLayer.Bounds);

                    // drawing outside of the canvas is a no-op, so don't do anything in that case!
                    // also make sure we're within the clip region
                    if (saveRect.Width > 0 && saveRect.Height > 0 && renderArgs.Graphics.Clip.IsVisible(saveRect))
                    {
						Rectangle saveRectRounded = new Rectangle(
							(int)saveRect.Left, (int)saveRect.Top,
							(int)Math.Ceiling(saveRect.Right - (int)saveRect.Left),
							(int)Math.Ceiling(saveRect.Bottom - (int)saveRect.Top));
                        PlacedSurface savedPS = new PlacedSurface(renderArgs.Surface, saveRectRounded);
                        savedSurfaces.Add(savedPS);

                        if (Workspace.Environment.AntiAliasing)
                        {
                            renderArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        }
                        else
                        {
                            renderArgs.Graphics.SmoothingMode = SmoothingMode.None;
                        }

                        renderArgs.Graphics.CompositingMode = CompositingMode.SourceOver;

						renderArgs.Graphics.FillPolygon(brush, poly, FillMode.Winding);
						renderArgs.Graphics.FillEllipse(brush, dotRect);

                        bitmapLayer.Invalidate(saveRectRounded);
                        //Workspace.Update();
                    }
                }
			}
            else
            {
				lastMouseXY = currMouseXY;
				lastNorm = PointF.Empty;
				lastDir = PointF.Empty;
            }
        }

        protected override void OnStylusUp(StylusEventArgs e)
		{
			base.OnStylusUp(e);

			Cursor = cursorMouseUp;

            if (mouseDown)
            {
				mouseDown = false;

                if (savedSurfaces.Count > 0)
                {
                    PdnRegion saveMeRegion = new PdnRegion();
                    saveMeRegion.MakeEmpty();

                    foreach (PlacedSurface pi1 in savedSurfaces)
                    {
                        saveMeRegion.Union(pi1.Bounds);
                    }

                    using (PdnRegion simplifiedRegion = Utility.SimplifyAndInflateRegion(saveMeRegion))
                    {
                        using (IrregularSurface weDrewThis = new IrregularSurface(renderArgs.Surface, simplifiedRegion))
                        {
                            for (int i = savedSurfaces.Count - 1; i >= 0; --i)
                            {
                                PlacedSurface ps = (PlacedSurface)savedSurfaces[i];
                                ps.Draw(renderArgs.Surface);
                                ps.Dispose();
                            }

                            savedSurfaces.Clear();

                            HistoryAction ha = bitmapLayer.CreateHistoryAction(Name, Image, simplifiedRegion);
                            weDrewThis.Draw(bitmapLayer.Surface);
                            Workspace.History.PushNewAction(ha);
                        }
                    }
                }
            }
		}

		public PaintBrushTool(DocumentWorkspace parent)
			: base(parent)
		{
			toolBarImage = Utility.GetImageResource("Icons.PaintBrushToolIcon.bmp");
			name = "Paintbrush";
			description = "Draws a freeform, arbitrarily wide line in a variety of styles.";
			helpText = "Left click to draw with foreground color, right click to draw with background color";

			autoScroll = false;

			// separate cursor assignements
			cursorMouseUp   = new Cursor(Utility.GetResourceStream("Cursors.PaintBrushToolCursor.cur"));
			cursorMouseDown = new Cursor(Utility.GetResourceStream("Cursors.PaintBrushToolCursorMouseDown.cur"));
			Cursor = cursorMouseUp;

			// initialize any state information you need
			mouseDown = false;
		}
	}
}
