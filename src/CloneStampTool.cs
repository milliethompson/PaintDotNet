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
	/// Summary description for CloneStampTool.
	/// </summary>
	public class CloneStampTool 
		: Tool
	{
		private bool mouseDown;
		private bool sourcePresent;
		private ArrayList savedSurfaces;
		private Point sourceMouseXY;
		private Point lastMouseXY;
		private Point firstMouseXY;
		private RenderArgs renderArgs;
		private BitmapLayer bitmapLayer;

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

		private void ConfigRenderArgs()
		{
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
		}

		protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
		{
			base.OnMouseDown (e);
		
			
			// CTRL is being held, get the stamp from the current location
			if(((ModifierKeys & Keys.Control) != 0) && ((e.Button & MouseButtons.Left) == MouseButtons.Left))
			{
				sourceMouseXY = Utility.GetPointFromMouseXY(e);
				sourcePresent = true;
			}
			else if (sourcePresent && (e.Button & MouseButtons.Left) == MouseButtons.Left) // Left Click
			{
				mouseDown = true;
				lastMouseXY = Utility.GetPointFromMouseXY(e);
				firstMouseXY = Utility.GetPointFromMouseXY(e);

				ConfigRenderArgs();
				OnMouseMove(e);				
			}
		}

		private Point GetPointDifference(Point p, MouseEventArgs e)
		{
			Point final = new Point();
			final.X = e.X - p.X;
			final.Y = e.Y - p.Y;
			return final;
		}

		private Point GetPointDifference(Point p, Point e)
		{
			Point final = new Point();
			final.X = e.X - p.X;
			final.Y = e.Y - p.Y;
			return final;
		}

		private Point FactorInPointDifference(Point p, Point diff)
		{
			Point final = new Point(p.X, p.Y);
			final.X += diff.X;
			final.Y += diff.Y;
			return final;
		}
	
		private Point AdjustedFromSrc(Point p)
		{
			Point diff = GetPointDifference(firstMouseXY, p);
			Point newp = new Point(sourceMouseXY.X + diff.X, sourceMouseXY.Y + diff.Y);

			return newp;
		}


		private RenderArgs AliasCloneToRenderArgs(RenderArgs r, Pen pen, Point dst, Point src)
		{
			Size size = new Size((int)pen.Width + 1, (int)pen.Width +1);
			Rectangle overImage = new Rectangle(src, size);
			Surface clone = r.Surface.CreateWindow(overImage);
			RenderArgs nr = new RenderArgs(clone);
			return nr;
		}

		private void BlendCloneIntoImage(RenderArgs clone, RenderArgs image, Point dst, Point src)
		{
			//image.Bitmap.
		}

		private void DrawCircleOverLineFromClone(RenderArgs r, Pen pen, Point a, Point b)
		{
			Point[] coords = Utility.GetLinePoints(a, b);
			Point[] srccoords = Utility.GetLinePoints(AdjustedFromSrc(a), AdjustedFromSrc(b));

			int penWidth = (int)pen.Width;
			int halfPenWidth = (int)(pen.Width / 2.0f);
            
			// Configure new RenderArgs to alias the bitmap
			Rectangle aliasedRect = new Rectangle(0, 0, penWidth + 1, penWidth + 1);
			Surface ns = new Surface((int)pen.Width + 1, (int)pen.Width + 1);
			RenderArgs nra = new RenderArgs(ns);
			//nra.Bounds = aliasedRect;
			nra.Graphics.Clear(Color.FromArgb(0, 255, 255, 255));
			nra.Graphics.DrawEllipse(pen, nra.Bounds);

			for(int i = 0; i < coords.Length; i++)
			{
				nra = AliasCloneToRenderArgs(r, pen, coords[i], srccoords[i]);
				BlendCloneIntoImage(nra, r, coords[i], srccoords[i]);
			}
		}

		private void DoCloningAndSave(MouseEventArgs e, Pen pen)
		{
			Point a = lastMouseXY;
			Point b = new Point(e.X, e.Y);

			if (Workspace.ActiveLayer is BitmapLayer)
			{
				Rectangle saveRect = Utility.PointsToRectangle(a, b);

				saveRect.Inflate((int)Math.Ceiling(pen.Width), (int)Math.Ceiling(pen.Width));

				if (renderArgs.Graphics.SmoothingMode == SmoothingMode.AntiAlias)
				{
					saveRect.Inflate(1, 1);
				}

				saveRect.Intersect(Workspace.ActiveLayer.Bounds);

				// drawing outside of the canvas is a no-op, so don't do anything in that case!
				// also make sure we're within the clip region
				if (saveRect.Width > 0 && saveRect.Height > 0 && renderArgs.Graphics.Clip.IsVisible(saveRect))
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

//					DrawCircleOverLine(renderArgs.Graphics, pen, a, b);
					DrawCircleOverLineFromClone(renderArgs, pen, a, b);

					//bitmapLayer.Invalidate(saveRect);
					bitmapLayer.Invalidate();
					Workspace.Update();
				}
			}
			else
			{
				// TODO throw exception?
			}

			
		}

		protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
		{
			base.OnMouseMove (e);

			if(mouseDown)
			{
				Pen pen = Workspace.Environment.CreatePen(false);

				DoCloningAndSave( e, pen);

				lastMouseXY.X = e.X; 
				lastMouseXY.Y = e.Y;
			}
		}

		protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
			base.OnMouseUp (e);

			if((ModifierKeys & Keys.Control) != 0)
			{
				return; // We have just let up from Cloning
			}
			else if ((e.Button & MouseButtons.Left) == MouseButtons.Left) // Create the History Action
			{
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
			mouseDown = false;
		}
        
		public CloneStampTool(DocumentWorkspace parent) 
			: base(parent)
		{
			//
			// TODO: Add constructor logic here
			//
			name = "Clone Stamp";
			toolBarImage = Utility.GetImageResource("Icons.CloneStampToolIcon.bmp");
			cursor = new Cursor(Utility.GetResourceStream("Cursors.CloneStampToolCursor.cur"));
			description = "Clone Stamp";
			lastMouseXY = new Point();
			firstMouseXY = new Point();
			sourceMouseXY = new Point();
			mouseDown = false;
		}
	}
}
