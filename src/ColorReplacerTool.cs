using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet
{
	public class ColorReplacerTool : Tool
	{
		private bool mouseDown;

		// these were ripped off from the v1.0 of the PaintBrushTool.cs document
		private Point lastMouseXY;
		private MouseButtons mouseButton;
		private RenderArgs renderArgs;
		private BitmapLayer bitmapLayer;
		private ArrayList savedSurfaces;

		// these were added by MK
		private float     penWidth;
		private int       ceilingPenWidth;
		private int       halfPenWidth;
		private ColorBgra colorToReplace;
		private ColorBgra colorReplacing;
		private Cursor    cursorMouseDown, cursorMouseUp, cursorMouseDownPickColor, cursorMouseDownAdjustColor;

		// private ColorBgra replacementDiff;
		private static ColorBgra colorToleranceBasis = ColorBgra.FromBgra(0x20, 0x20, 0x20, 0x00);
		private PdnRegion clipRegion;
		private UserBlendOps.NormalBlendOp blendOp = new UserBlendOps.NormalBlendOp();
		private bool      hasDrawn;

		// AA stuff
		private SortedList aaPoints;
	
		// RenderArgs specifically for a brush mask
		private RenderArgs  brushRenderArgs;

		# region hotkey assignment
		public override char HotKey
		{
			get
			{
				return 'r';
			}
		}
		# endregion

		# region tolerance property and method
		private int myTolerance;
		private bool ColorInTolerance(ColorBgra colorA, ColorBgra colorB)
		{
			return (Utility.ColorDifference(colorA, colorB) <= myTolerance);
		}

		private void RestrictTolerance()
		{
			int difference = Utility.ColorDifference(colorReplacing, colorToReplace);
			if (myTolerance > difference)
				myTolerance = difference;
		}
		# endregion

		# region Activate/Deactivate stuff copied from v1.0 PaintBrush code


		// ripped off...er copied from v1.1 PaintBrushTool.cs code
		protected override void OnActivate()
		{
			base.OnActivate();

			aaPoints = new SortedList();

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
			aaPoints = null;

			renderArgs = null;
			bitmapLayer = null;
		}
		# endregion

		# region color picker code
		private ColorBgra LiftColor( int x, int y )
		{
			return ((BitmapLayer)this.Workspace.ActiveLayer).Surface[x, y];
		}

		/// <summary>
		/// Picks up the color under the mouse and assigns to the forecolor (or backcolor).
		/// If assigning to the forecolor, the backcolor will be adjusted respective to the
		/// difference of the old forecolor versus the new forecolor.
		/// </summary>
		/// <param name="e"></param>
		private void AdjustDrawingColor(MouseEventArgs e)
		{
			ColorBgra oldColor;

			if (bdMouseLeft(e))
			{
				oldColor = this.Workspace.Environment.ForeColor;
				PickColor(e);
				this.Workspace.Environment.BackColor = 
					AdjustColorDifference(oldColor, 
					this.Workspace.Environment.ForeColor,
					this.Workspace.Environment.BackColor);
			}

			if (bdMouseRight(e))
			{
				oldColor = this.Workspace.Environment.BackColor;
				PickColor(e);
				this.Workspace.Environment.ForeColor = 
						AdjustColorDifference(oldColor, 
						this.Workspace.Environment.BackColor,
						this.Workspace.Environment.ForeColor);
			}
		}

		/// <summary>
		/// Returns a ColorBgra shift by the difference between oldcolor and newcolor but using 
		/// basisColor as the basis.
		/// </summary>
		/// <param name="oldcolor"></param>
		/// <param name="newcolor"></param>
		/// <param name="shiftColor"></param>
		/// <returns></returns>
		private ColorBgra AdjustColorDifference(ColorBgra oldColor, ColorBgra newColor, ColorBgra basisColor) 
		{
			ColorBgra returnColor;

			// eliminate testing for the "equal to" case
			returnColor = basisColor;

			for(int chan = 0; chan < 3; chan++)
			{
				if (oldColor[chan] > newColor[chan])
				{
					returnColor[chan] = Utility.ClampToByte(basisColor[chan] - (oldColor[chan] - newColor[chan]));
				}
				else
				{
					returnColor[chan] = Utility.ClampToByte(basisColor[chan] + (newColor[chan] - oldColor[chan]));
				}
			}
			return(returnColor);
		}

		private void PickColor(MouseEventArgs e)
		{
			if (!Utility.IsPointInRectangle(new Point(e.X, e.Y), new Rectangle(new Point(0,0), this.Workspace.Document.Size)))
			{
				return;
			}
			// if we managed to get here without any mouse buttons down
			// we return promptly.
			if (bdMouseLeft(e) || bdMouseRight(e))
			{
				// since the above statement exits if one or the other
				if (bdMouseLeft(e))
				{
					colorReplacing   = LiftColor(e.X, e.Y);
					colorReplacing.A = this.Workspace.Environment.ForeColor.A;
					this.Workspace.Environment.ForeColor = colorReplacing;
				}
				else
				{
					colorToReplace   = LiftColor(e.X, e.Y);
					colorToReplace.A = this.Workspace.Environment.BackColor.A;
					this.Workspace.Environment.BackColor = colorToReplace;
				}
			}
			else
			{
				return;
			}

			// before assigned the newly lifted color, we preserve the
			// alpha from the colorWheelForm.

		}
		# endregion color picker code

		# region rendering methods
		private void RenderCircleBrush()
		{
			// create pen mask surface
			// marker
			brushRenderArgs = new RenderArgs(new Surface(ceilingPenWidth, ceilingPenWidth));

			if (Workspace.Environment.AntiAliasing)
			{
				brushRenderArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			}
			else
			{
				brushRenderArgs.Graphics.SmoothingMode = SmoothingMode.None;
			}

			if (Workspace.Environment.AntiAliasing)
			{
				if (penWidth > 2)
				{
					penWidth = penWidth - 1.0f;
				}
				else
				{
					penWidth = penWidth / 2;
				}
			}
			else
			{
				if (penWidth <= 1.0f)
				{
					brushRenderArgs.Bitmap.SetPixel(1,1,System.Drawing.Color.Black);
				}
				else
				{
					penWidth = (float)Math.Round(penWidth + 1.0f);
				}
			}

			using (Brush testBrush = new SolidBrush(System.Drawing.Color.Black))
			{
				brushRenderArgs.Graphics.FillEllipse(testBrush, 
					0.0f, 
					0.0f, 
					penWidth,
					penWidth);
			}
		}

		private bool PointAlreadyAntiAliased(string str)
		{
			bool paaaReturnValue;

			try 
			{
				paaaReturnValue = aaPoints.ContainsKey(str);
			}
			catch
			{
				paaaReturnValue = false;
			}
			return(paaaReturnValue);
		}

		private unsafe void DrawOverPoints(Point start, Point finish, ColorBgra colorReplacing, ColorBgra colorToReplace)
		{
			ColorBgra   colorAdjusted = ColorBgra.FromColor(Color.Empty);
			byte		dstAlpha;
			ColorBgra	colorLifted;
			Rectangle[] rectSelRegions;
			Rectangle   rectBrushArea;
			Rectangle	rectBrushRelativeOffset = new Rectangle(0,0,0,0);
			
			// special condition for a canvas with no active selection
			// create an array of rectangles with a single rectangle 
			// specifying the size of the canvas
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

			// code ripped off from clone stamp tool
			Point direction = new Point(finish.X - start.X, finish.Y - start.Y);
			float length = Utility.Magnitude(direction);
			float bw = Workspace.Environment.PenInfo.Width / 2;

			// iterate through all points in the linear stroke
			for (float f = 0; f < 1; f += (float)Math.Sqrt(bw) / length) 
			{
				PointF q = new PointF(finish.X * (1 - f) + f * start.X,
					finish.Y * (1 - f) + f * start.Y);

				Point p = Point.Round(q);

				// iterate through all rectangles
				foreach (Rectangle rectSel in rectSelRegions)
				{
					// set the perimeter values for the rectBrushRegion rectangle
					// so the area can be intersected with the active
					// selection individual recSelRegion rectangle.
					rectBrushArea =   new Rectangle(p.X - halfPenWidth,
						p.Y - halfPenWidth,
						ceilingPenWidth,
						ceilingPenWidth);

					// test the intersection...
					// the perimeter values of rectBrushRegion (above)
					// may calculate negative but
					// *should* always be clipped to acceptable values by
					// by the following intersection.
					if (rectBrushArea.IntersectsWith(rectSel))
					{
						// a valid intersection was found.
						// prune the brush rectangle to fit the intersection.
						rectBrushArea.Intersect(rectSel);
						for (int y = rectBrushArea.Top; y < rectBrushArea.Bottom; y++)
						{
							// create a new rectangle for an offset relative to the 
							// the brush mask
							rectBrushRelativeOffset.X    = Math.Max(rectSel.X - (p.X - halfPenWidth), 0); 
							rectBrushRelativeOffset.Y    = Math.Max(rectSel.Y - (p.Y - halfPenWidth), 0);
							rectBrushRelativeOffset.Size = rectBrushArea.Size;
							
							// get the source address of the first pixel from the brush mask.
							ColorBgra *srcBgra = brushRenderArgs.Surface.GetPointAddress(rectBrushRelativeOffset.Left,
								rectBrushRelativeOffset.Y + y - rectBrushArea.Y);
							
							// get the address of the pixel we want to change on the canvas.
							ColorBgra *dstBgra = renderArgs.Surface.GetPointAddress(rectBrushArea.Left, y);

							for (int x = rectBrushArea.Left; x < rectBrushArea.Right; x++)
							{
								if ((*srcBgra).A != 0)
								{
									colorLifted      = *dstBgra;
									// hasDrawn is set if a pixel endures color replacement so that 
									// the placed surface will be left alone, otherwise, the placed
									// surface will be discarded
									// adjust the channel color up and down based on the difference calculated
									// from the source.  These values are clamped to a byte.  It's possible
									// that the new color is too dark or too bright to take the whole range 

									// float diff = Utility.ColorDifferenceSquared(colorLifted, colorToReplace) / (1.0f + myTolerance + myTolerance);
									// if (diff <= 1) 

									bool boolCIT = this.ColorInTolerance(colorLifted, colorToReplace);
									bool boolPAAA = false;
									if (Workspace.Environment.AntiAliasing)
									{
										boolPAAA = this.PointAlreadyAntiAliased(x.ToString() + "," + y.ToString());
									}

									if (boolCIT || boolPAAA)
									{
										if (boolPAAA)
										{
											colorAdjusted = (ColorBgra)aaPoints[x.ToString() + "," + y.ToString()];
											if (penWidth < 2.0f)
											{
												for(int chan = 0; chan < 3; chan++)
												{
													colorAdjusted[chan] = Utility.ClampToByte(colorReplacing[chan] + colorAdjusted[chan] - colorToReplace[chan]);
													//colorAdjusted[chan] = (byte)Utility.Lerp(colorReplacing[chan], colorLifted[chan], diff);
												}
											}
										}
										else
										{
											for(int chan = 0; chan < 3; chan++)
											{
												colorAdjusted[chan] = Utility.ClampToByte(colorLifted[chan] + colorReplacing[chan] - colorToReplace[chan]);
												//colorAdjusted[chan] = (byte)Utility.Lerp(colorReplacing[chan], colorLifted[chan], diff);
											}
										}

										if (((*srcBgra).A != 255) && Workspace.Environment.AntiAliasing)
										{
											colorAdjusted.A = (*srcBgra).A;
											dstAlpha = (*dstBgra).A;
											*dstBgra = blendOp.Apply(*dstBgra, colorAdjusted);
											(*dstBgra).A = dstAlpha;
											if (! aaPoints.ContainsKey(x.ToString() + "," + y.ToString()))
											{
												aaPoints.Add(x.ToString() + "," + y.ToString(),colorAdjusted);
											}
										}
										else
										{
											colorAdjusted.A = (*dstBgra).A;
											*dstBgra = colorAdjusted;
											if (boolPAAA)
											{
												aaPoints.Remove(x.ToString() + "," + y.ToString());
											}
										}
										
										hasDrawn = true;
									}
								}
								srcBgra++;
								dstBgra++;
							}
						}
					}
				}
			}
		}

		# endregion

		# region keyboard methods

		private bool kdShift()
		{
			return(ModifierKeys == Keys.Shift);
		}

		private bool kdCtrl()
		{
			return(ModifierKeys == Keys.Control);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (!mouseDown)
			{
				if (kdCtrl())
				{
					Cursor = cursorMouseDownPickColor;
				}

				if (kdShift())
				{
					Cursor = cursorMouseDownAdjustColor;
				}
			}
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			if ((!kdCtrl() || !kdShift()) && !mouseDown)
			{
				Cursor = cursorMouseUp;
			}
		}
		# endregion

		# region mouse methods
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

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (mouseDown)
			{
				return;
			}

			if (bdMouseLeft(e) || bdMouseRight(e))
			{
				mouseDown   = true;
				Cursor = cursorMouseDown;
				if ((!kdCtrl()) && (!kdShift()))
				{
					mouseButton = e.Button;

				
					lastMouseXY.X = e.X;
					lastMouseXY.Y = e.Y;

					// code copied (and slightly modified) from v1.1 PaintBrush
					// parses and establishes the active selection area
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

					// find the replacement color and the color to replace
					colorReplacing = Workspace.Environment.ForeColor;
					colorToReplace = Workspace.Environment.BackColor;
					penWidth       = Workspace.Environment.PenInfo.Width;

					// get the pen width find the ceiling integer of half of the pen width
					ceilingPenWidth = (int)Math.Max(Math.Ceiling(penWidth), 3);

					// used only for cursor positioning
					halfPenWidth    = (int)Math.Ceiling(penWidth / 2.0f);

					// set hasDrawn to false since nothing has been drawn		
					hasDrawn = false;

					// render the circle via GDI+ so the AA techniques can precisely
					// mimic GDI+.
					RenderCircleBrush(); 

					// establish tolerance
					myTolerance = Workspace.Environment.Tolerance;

					// restrict tolerance so no overlap is permitted
					RestrictTolerance();
					OnMouseMove(e);
				}
				else
				{
					OnMouseMove(e);
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (!kdShift() && !kdCtrl())
			{
				Cursor = cursorMouseUp;
			}

			if (mouseDown)
			{
				OnMouseMove(e);

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

						if (hasDrawn)
						{
							HistoryAction ha = bitmapLayer.CreateHistoryAction(Name, Image, simplifiedRegion);
							weDrewThis.Draw(bitmapLayer.Surface);
							Workspace.History.PushNewAction(ha);
						}
					}
				}
				mouseDown = false;
			}
			// dispose added by MK
			if (clipRegion != null)
			{
				clipRegion.Dispose();
			}

			if (brushRenderArgs != null)
			{
				if (brushRenderArgs.Surface != null)
				{
					brushRenderArgs.Surface.Dispose();
				}
			}
		}

		
		protected override void OnMouseMove(MouseEventArgs e)
		{
					
			base.OnMouseMove(e);

			if (mouseDown)
			{
				if  (bdMouseLeft(e) | bdMouseRight(e))
				{
					if (!kdShift() & !kdCtrl())
					{
						// if the foreground and background colors are identical,
						// return...there's no point in committing any action
						if (colorReplacing == colorToReplace)
						{
							return;
						}

						// get our start and end coordinates, since we need
						// to trace along an action line -- the user will expect this behavior
						// if we don't, it'll look like a tin can riddled with bullet holes
						Point pointStartCorner = lastMouseXY;          // start point
						Point pointEndCorner   = new Point(e.X, e.Y);  // end point

						// create the rectangle with the 'a' and 'b' points above
						Rectangle inspectionRect = 
							Utility.PointsToRectangle(pointStartCorner, pointEndCorner);

						// inflate the region to address account for the pen width
						// then intersect with the Workspace to "clip" the boundary
						// the total area of the clipped rectangle includes the
						// width of the pen surrounding the points limited by either
						// the canvas perimeter or the selection outline
						inspectionRect.Inflate(1 + ceilingPenWidth / 2, 1 + ceilingPenWidth / 2);
						inspectionRect.Intersect(Workspace.ActiveLayer.Bounds);

						// Enforce the selection area restrictions.
						// If within the selection area restrictions, build an image history
						bool gotWidth      = inspectionRect.Width  > 0;
						bool gotHeight     = inspectionRect.Height > 0;
						bool isInClip	   = renderArgs.Graphics.Clip.IsVisible(inspectionRect);

						if ((gotWidth) && (gotHeight) && (isInClip))
						{
							PlacedSurface savedPS = new PlacedSurface(renderArgs.Surface, inspectionRect);
							savedSurfaces.Add(savedPS);

							renderArgs.Graphics.CompositingMode = CompositingMode.SourceOver;
					
							// check the mouse buttons and if we've made it this far, at least
							// one of the mouse buttons (left|right) was depressed
							if (bdMouseLeft(e))
							{
								this.DrawOverPoints(pointStartCorner, pointEndCorner, colorReplacing, colorToReplace);
							}
							else
							{
								this.DrawOverPoints(pointStartCorner, pointEndCorner, colorToReplace, colorReplacing);
							}

							bitmapLayer.Invalidate(inspectionRect);
							Workspace.Update();
						}

						// update the lastMouseXY so we know how to "connect the dots"
						lastMouseXY = pointEndCorner;
					}
					else
					{
						switch (ModifierKeys & (Keys.Control | Keys.Shift))
						{
							case Keys.Control: PickColor(e);
								break;
							case Keys.Shift: AdjustDrawingColor(e);
								break;
							default: break;
						}
					}
				}
			}
		}

		# endregion

		public ColorReplacerTool(DocumentWorkspace parent)
			: base(parent)
		{
			toolBarImage = Utility.GetImageResource("Icons.ColorReplacerToolIcon.bmp");
			name = "Color Replacer";
			description = "Replaces the color under the brush with an alternate color";
			helpText = "Replaces palette colors complementarily.  See Help->Help Topics for performance and extra functions.";

			// initialize any state information you need
			cursorMouseUp = new Cursor(Utility.GetResourceStream("Cursors.ColorReplacerToolCursor.cur"));
			cursorMouseDown = new Cursor(Utility.GetResourceStream("Cursors.GenericToolCursorMouseDown.cur"));
			cursorMouseDownPickColor = new Cursor(Utility.GetResourceStream("Cursors.ColorReplacerToolCursorPickColor.cur"));
			cursorMouseDownAdjustColor = new Cursor(Utility.GetResourceStream("Cursors.ColorReplacerToolCursorAdjustColor.cur"));
			Cursor = cursorMouseUp;

			mouseDown = false;
			// fetch colors from workspace palette
			this.colorToReplace = this.Workspace.Environment.ForeColor;
			this.colorReplacing = this.Workspace.Environment.BackColor;
		}
	}
}