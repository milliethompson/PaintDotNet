using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections;

namespace PaintDotNet
{
	/// <summary>
	/// Ctrl left-click to select an origin, left click to place it
	/// </summary>
	public class CloneStampTool
		: Tool
	{
		private static Point takeFrom = Point.Empty;
		private static Point lastMoved = Point.Empty;

		private static WeakReference wr;
		private BitmapLayer takeFromLayer;

		private bool switchedTo = false;
		private EventHandler documentChangedDelegate;
		private Rectangle undoRegion = Rectangle.Empty;
		private PlacedSurface savedSurface;
		private RenderArgs ra;
		private bool mouseUp = true;
		private ArrayList historySections;
		private bool antialiasing;
		private PdnRegion clipRegion;

		// private bool added by MK for "clone source" cursor transition
		private bool mouseDownSettingCloneSource;

		private Cursor cursorMouseDown, cursorMouseUp, cursorMouseDownSetSource;

		public override char HotKey
		{
			get
			{
				return 'c';
			}
		}

		# region (MK) simplified keyboard modifier and mouse methods
		// Added by Michael Kelsey when adding cursor transitions.

		private bool kdShift()
		{
			return(ModifierKeys == Keys.Shift);
		}

		private bool kdCtrl()
		{
			return(ModifierKeys == Keys.Control);
		}

		/// <summary>
		/// Button down mouse left.  Returns true if only the left mouse button is depressed.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		private bool bdMouseLeft(MouseEventArgs e)
		{
			return(e.Button == MouseButtons.Left);
		}

		/// <summary>
		/// Button down mouse right.  Returns true if only the right mouse is depressed.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		private bool bdMouseRight(MouseEventArgs e)
		{
			return(e.Button == MouseButtons.Right);
		}
		# endregion

		// Not implemented yet. Most likely never will implement, and will be taken out soon
		#region ink support
//		protected override void OnStylusUp(StylusEventArgs e)
//		{
//			this.OnMouseUp(e);
//			base.OnStylusUp (e);
//		}
//
//		protected override void OnStylusDown(StylusEventArgs e)
//		{
//			this.OnMouseDown(e);
//			base.OnStylusUp (e);
//		}
//		protected override void OnStylusMove(StylusEventArgs e)
//		{
//			this.OnMouseMove(e);
//			base.OnStylusUp (e);
//		}
//
//		protected override bool SupportsInk
//		{
//			get
//			{
//				return true;
//			}
//		}
		#endregion

		public CloneStampTool(DocumentWorkspace parent) : base(parent)
		{
			name = "Clone Stamp";
			toolBarImage = Utility.GetImageResource("Icons.CloneStampToolIcon.bmp");
			description = "Copies a Section of the Picture";
			helpText = "Hold ctrl and left click to select an origin. Afterwards, left click to place";

			cursorMouseDown = new Cursor(Utility.GetResourceStream("Cursors.GenericToolCursorMouseDown.cur"));
			cursorMouseDownSetSource = new Cursor(Utility.GetResourceStream("Cursors.CloneStampToolCursorSetSource.cur"));
			cursorMouseUp   = new Cursor(Utility.GetResourceStream("Cursors.CloneStampToolCursor.cur"));
			Cursor = cursorMouseUp;

			documentChangedDelegate = new EventHandler(CloneStamp_DocumentChangedHandler);
			Workspace.DocumentChanged += documentChangedDelegate;
		}

		private void CloneStamp_DocumentChangedHandler(object sender, EventArgs e)
		{
			takeFrom = Point.Empty;
			lastMoved = Point.Empty;
			takeFromLayer = null;
		}

		protected override void OnActivate()
		{
			base.OnActivate();
			
			if (Workspace.ActiveLayer != null)
			{
				switchedTo = true;
				historySections = new ArrayList();
				if((wr != null) && (wr.IsAlive))
				{
					takeFromLayer = (BitmapLayer)(wr.Target);
				}
				else
				{
					takeFromLayer = null;
				}
			}
			
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (kdCtrl() && mouseUp)
			{
				Cursor = cursorMouseDownSetSource;
				mouseDownSettingCloneSource = true;
			}
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			// this isn't likely the best way to check to see if
			// the CTRL key has been let up.  If it's not, version
			// 2.1 can address the discrepancy.
			if (!kdCtrl() && mouseDownSettingCloneSource)
			{
				Cursor = cursorMouseUp;
				mouseDownSettingCloneSource = false;
			}
		}
				
		protected override void OnMouseUp(MouseEventArgs e)
		{
			mouseUp = true;

			if (!mouseDownSettingCloneSource)
			{
				Cursor = cursorMouseUp; 
			}

			// Slap down that undo action
			if(bdMouseLeft(e))
			{
				if(savedSurface != null)
				{
					savedSurface.Draw(ra.Surface);
					Workspace.ActiveLayer.Invalidate(savedSurface.Bounds);
					savedSurface.Dispose();
					savedSurface = null;
					Workspace.Update();
				}

				if(takeFrom == Point.Empty || lastMoved == Point.Empty)
					return;

				if(historySections.Count > 0)
				{
					PdnRegion saveMeRegion = new PdnRegion();
					saveMeRegion.MakeEmpty();

					foreach (PlacedSurface pi1 in historySections)
					{
						saveMeRegion.Union(pi1.Bounds);
					}

					PdnRegion simplifiedRegion = Utility.SimplifyAndInflateRegion(saveMeRegion);

					using (IrregularSurface weDrewThis = new IrregularSurface(ra.Surface, simplifiedRegion))
					{
						for (int i = historySections.Count - 1; i >= 0; --i)
						{
							PlacedSurface ps = (PlacedSurface)historySections[i];
							ps.Draw(ra.Surface);
							ps.Dispose();
						}

						historySections.Clear();
						historySections = null;
						historySections = new ArrayList();

						HistoryAction ha = ((BitmapLayer)Workspace.ActiveLayer).CreateHistoryAction(Name, Image, simplifiedRegion);
						weDrewThis.Draw(((BitmapLayer)Workspace.ActiveLayer).Surface);
						Workspace.History.PushNewAction(ha);
					}
				}
			}
		}

		private void DrawACircle(PointF pt, Surface srfSrc, Surface srfDst, Point difference, Rectangle rect) 
		{
			float bw = Workspace.Environment.PenInfo.Width / 2;
			float envAlpha = Workspace.Environment.ForeColor.A / 255.0f;
			rect.Intersect(new Rectangle(difference, srfSrc.Size));
			rect.Intersect(srfDst.Bounds);

			if(rect.Width == 0 || rect.Height == 0)
				return;

			envAlpha *= envAlpha;
			envAlpha *= envAlpha;
			for (int y = rect.Top; y < rect.Bottom; y++) 
			{
				unsafe 
				{
					ColorBgra *dst;
					ColorBgra *src;
					try
					{
						dst = srfDst.GetRowAddressUnchecked(y);
						src = &srfSrc.GetRowAddress(y - difference.Y)[-difference.X];
					}
					catch(ArgumentOutOfRangeException)
					{
						continue;
					}

					for (int x = rect.Left; x < rect.Right; x++) 
					{
						float distFromRing = 0.5f + bw - Utility.Distance(pt, new PointF(x, y));
						if (distFromRing > 0)
						{
							float alpha = antialiasing ? Utility.Clamp(distFromRing * envAlpha, 0, 1) : 1;
							alpha *= (src[x].A / 255.0f);
							dst[x].A = (byte)(255 - (255 - dst[x].A) * (1 - alpha));
							for (int c = 0; c < 3; c++) 
							{
								dst[x][c] = (byte)(((    alpha) + (1 - alpha) * dst[x].A / 255) == 0 ? (byte)0 :
									(byte)(
									(
									src[x][c] * (    alpha) + 
									dst[x][c] * (1 - alpha) * dst[x].A / 255
									)
									/
									(
									(    alpha) + 
									(1 - alpha) * dst[x].A / 255
									)
									));
							}
						}
					}
				}
			}
			rect.Inflate(1,1);
			Workspace.Document.Invalidate(rect);
		}

		private void DrawCloneLine(Point currentMouse, Point lastMoved, Point lastTakeFrom, Surface surfaceSource, Surface surfaceDest)
		{
			Rectangle[] rectSelRegions;
			Rectangle   rectBrushArea;
			int penWidth = (int)Workspace.Environment.PenInfo.Width;
			int ceilingPenWidth = (int)Math.Ceiling(penWidth);

			if(mouseUp == true || switchedTo == true)
			{
				lastMoved = currentMouse;
				lastTakeFrom = takeFrom;
				mouseUp = false;
				switchedTo = false;
			}

			Point difference = new Point(currentMouse.X - takeFrom.X, currentMouse.Y - takeFrom.Y);
			Point direction = new Point(currentMouse.X - lastMoved.X, currentMouse.Y - lastMoved.Y);
			float length = Utility.Magnitude(direction);
			float bw = 1 + Workspace.Environment.PenInfo.Width / 2;
			
			
			if (Workspace.Environment.IsSelectionEmpty)
			{
				rectSelRegions = 
					new Rectangle [] {
										 new Rectangle(0,0,
										 this.Workspace.Document.Width,
										 this.Workspace.Document.Height)
									 };
			}
			else
			{
				rectSelRegions = clipRegion.GetRegionScansInt();
			}
	
			// Create a history action
			Rectangle rect = Utility.PointsToRectangle(lastMoved,currentMouse);
			rect.Inflate(penWidth/2 + 1,penWidth/2 + 1);
			rect.Intersect(new Rectangle(difference, surfaceSource.Size));
			rect.Intersect(surfaceDest.Bounds);
			
			if(rect.Width == 0 || rect.Height == 0)
				return;

			PlacedSurface savedPS = new PlacedSurface(ra.Surface, rect);
			historySections.Add(savedPS);

			// Follow the line to draw the clone... line
			for (float f = 0; f < 1; f += (float)Math.Sqrt(bw) / length) 
			{
				// Do intersects with each of the rectangles in a selection
				foreach (Rectangle rectSel in rectSelRegions)
				{
					PointF p = new PointF(currentMouse.X * (1 - f) + f * lastMoved.X,
						currentMouse.Y * (1 - f) + f * lastMoved.Y);

					rectBrushArea =   new Rectangle((int)(p.X - bw),
						(int)(p.Y - bw),
						(int)(bw * 2 + 1),
						(int)(bw * 2 + 1));

					if (rectBrushArea.IntersectsWith(rectSel))
					{
						rectBrushArea.Intersect(rectSel);

						// Draw a circle
						DrawACircle(p,surfaceSource,surfaceDest,difference, rectBrushArea);
					}
				}
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{			
			base.OnMouseMove(e);
			if (!(Workspace.ActiveLayer is BitmapLayer) || (takeFromLayer == null))
			{
				return;
			}

//			if(historySections == null)
//				historySections = new ArrayList();

			if( (bdMouseLeft(e)) && (takeFrom != Point.Empty) && !kdCtrl() )
			{
				Point currentMouse = new Point(e.X,e.Y);

				Point lastTakeFrom = Point.Empty;
				if(lastMoved != Point.Empty)
				{
					Point difference = new Point(currentMouse.X - lastMoved.X, currentMouse.Y - lastMoved.Y);
					lastTakeFrom = takeFrom;
					takeFrom = new Point(takeFrom.X + difference.X,takeFrom.Y + difference.Y);
				}
				else
				{
					lastTakeFrom = takeFrom;
					lastMoved = currentMouse;
				}

				int penWidth = (int)Workspace.Environment.PenInfo.Width;
				Rectangle rect;
				if(penWidth != 1)
					rect = new Rectangle(new Point(takeFrom.X - penWidth/2,takeFrom.Y - penWidth/2),new Size(penWidth+1, penWidth+1));
				else
					rect = new Rectangle(new Point(takeFrom.X - penWidth,takeFrom.Y - penWidth),new Size(2 * penWidth + 1, 2*penWidth+1));

				Rectangle boundRect = new Rectangle(takeFrom,new Size(1,1));

				// If the takeFrom area escapes the boundary
				if(!Workspace.ActiveLayer.Bounds.Contains(boundRect))
				{
					lastMoved = currentMouse;
					lastTakeFrom = takeFrom;
				}

				if(savedSurface != null)
				{
					savedSurface.Draw(ra.Surface);
					Workspace.ActiveLayer.Invalidate(savedSurface.Bounds);
					savedSurface.Dispose();
					savedSurface = null;
				}

				
				rect.Intersect(takeFromLayer.Surface.Bounds);
				if(rect.Width == 0 || rect.Height == 0)
					return;

				savedSurface = new PlacedSurface(ra.Surface,rect);

				// Draw that clone line
				DrawCloneLine(currentMouse,lastMoved, lastTakeFrom, takeFromLayer.Surface, ((BitmapLayer)Workspace.ActiveLayer).Surface);

				// Draw the "source" ellipse
				Pen blackPen = new Pen(Color.Black,1);

				if(penWidth != 1)
					penWidth /= 2;

				ra.Graphics.DrawEllipse(blackPen,takeFrom.X - penWidth,takeFrom.Y - penWidth,2*penWidth,2 * penWidth);
				Workspace.ActiveLayer.Invalidate(savedSurface.Bounds);
				Workspace.Update();
				
				lastMoved = currentMouse;

			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (!(Workspace.ActiveLayer is BitmapLayer))
			{
				return;
			}

			Cursor = cursorMouseDown;

			if(bdMouseLeft(e) && kdCtrl())
			{
				takeFrom = new Point(e.X,e.Y);
				wr = new WeakReference(((BitmapLayer)Workspace.ActiveLayer));
				takeFromLayer = (BitmapLayer)(wr.Target);
				//takeFromLayer = ((BitmapLayer)Workspace.ActiveLayer);
				lastMoved = Point.Empty;
				ra = new RenderArgs(((BitmapLayer)Workspace.ActiveLayer).Surface);
			}
			else
				if(bdMouseLeft(e) && !kdCtrl())
			{
				// Determine if there is something to work if, if there isn't return
				if(takeFrom == Point.Empty)
				{
					return;
				}

				if(!wr.IsAlive || takeFromLayer == null)
				{
					takeFrom = Point.Empty;
					lastMoved = Point.Empty;
					return;
					
				}

				// Make sure the layer is still there!
				if(takeFromLayer != null && !Workspace.Document.Layers.Contains(takeFromLayer))
				{	
					takeFrom = Point.Empty;
					lastMoved = Point.Empty;
					return;
				}

				if (!Workspace.Environment.IsSelectionEmpty)
				{
					clipRegion = Workspace.Environment.CreateSelectedRegion();
				}
				else
				{
					clipRegion = new PdnRegion();
					clipRegion.MakeInfinite();
				}

				antialiasing = Workspace.Environment.AntiAliasing;
				ra = new RenderArgs(((BitmapLayer)Workspace.ActiveLayer).Surface);
				ra.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				OnMouseMove(e);
			}
		}
	}
}
