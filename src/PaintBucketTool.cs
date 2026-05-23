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
	public class PaintBucketTool : Tool
	{
		private ColorBgra currentColor;
		private ColorBgra colorToDraw;
		private Region currentRegion;
		private Surface currentSurface;
		private int minx, maxx, miny, maxy;
		private static int tolerance = 10;

		protected override void OnActivate()
		{
			base.OnActivate();
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
            currentRegion = null;
            currentSurface = null;
		}

		#region Recursive Fill
		protected unsafe void FillRecursively(int x, int y)
		{
			// if current color is right and clip is right
			if((currentRegion.IsVisible(x, y)) &&
				(currentSurface[x,y] == currentColor)
				)
			{
				currentSurface[x,y] = colorToDraw;
				FillRecursively(x, y-1);
				FillRecursively(x-1, y);
				FillRecursively(x+1, y);
				FillRecursively(x, y+1);
			}

			// set color, call 8 others

		}
		#endregion

		#region Scan Line Fill
		private int FillLeft(int x, int y, bool[,] pixelsChecked, bool isDraw)
		{
			int lowx = x;
			
			while( (lowx >= 0) && (CheckColorBgra(currentSurface[lowx, y], currentColor)) && currentRegion.IsVisible(lowx,y) && !pixelsChecked[lowx, y] )
			{
				if(isDraw)
					currentSurface[lowx, y] = colorToDraw;
				pixelsChecked[lowx,y] = true;
				lowx--;
			}
			
			if( lowx < 0 )
			{
				//Debug.WriteLine("Invalid lowx");
			}

			if (lowx < minx)
			{
				minx = lowx;
			}

			return ++lowx;
		}

		private int FillRight(int x, int y, bool[,] pixelsChecked, bool isDraw)
		{
			int highx = x;
			while( (highx <= Workspace.Document.Width - 1) && 
			      currentRegion.IsVisible(highx,y) &&
				(CheckColorBgra(currentSurface[highx, y],currentColor)) && 
				!pixelsChecked[highx,y])
			{
				if(isDraw)
					currentSurface[highx,y] = colorToDraw;
				pixelsChecked[highx,y] = true;
				highx++;
			}
			
			if( highx < 0 )
			{
				//Debug.WriteLine("Invalid Highx");
			}

			if(highx >= maxx)
			{
				maxx = highx;
			}

			return --highx;
		}

		
		private bool IsWithinTolerance(byte check, byte src)
		{
			int a = (int)check;
			int b = (int)src;
			int diff = a - b;
			diff = Math.Abs(diff);

			if(diff <= tolerance)
			{
				return true;
			}
			else return false;
		}

		private bool CheckColorBgra(ColorBgra checkMe, ColorBgra source)
		{
			if(IsWithinTolerance(checkMe.r, source.r) &&
				IsWithinTolerance(checkMe.g, source.g) &&
				IsWithinTolerance(checkMe.b, source.b))
			{
				return true;
			}
			else return false;
		}

		private void FillScanLines(int x, int y, bool[,] pixelsChecked, bool isDraw)
		{
			int lowX = FillLeft(x,y,pixelsChecked, isDraw);
			int highX = FillRight(x + 1,y,pixelsChecked, isDraw);
			int i;
		
			// For Creating the Bounding Box///
			if(y > maxy)
			{
				maxy = y;
			}
			else if(y < miny)
			{
				miny = y;
			}

			// Vertical Scan
			for(i = lowX; i <= highX; i++)
			{
				if((y>0) && !pixelsChecked[i,y-1] && currentRegion.IsVisible(i,y-1) && CheckColorBgra(currentSurface[i, y-1], currentColor))
				{
					FillScanLines(i,y-1,pixelsChecked, isDraw);
				}

				if((y<Workspace.Document.Size.Height-1) && !pixelsChecked[i,y+1] && currentRegion.IsVisible(i,y+1) && CheckColorBgra(currentSurface[i,y+1], currentColor))
				{
					FillScanLines(i,y+1,pixelsChecked, isDraw);
				}
			}
		}
		#endregion
		
		#region DrawBoundingBox - Debug Purposes
		protected void DrawBoundingBox()
		{
			int i;
			int x, y;

			// Top
			y = miny;
			for(i = minx; i <= maxx; i++)
			{
				currentSurface[i,y] = colorToDraw;
			}

			// Bottom
			y = maxy;
			for(i = minx; i <= maxx; i++)
			{
				currentSurface[i,y] = colorToDraw;
			}

			// Left
			x = minx;
			for(i = miny; i <= maxy; i++)
			{
				currentSurface[x,i] = colorToDraw;
			}
			// Right
			x = maxx;
			for(i = miny; i <= maxy; i++)
			{
				currentSurface[x,i] = colorToDraw;
			}
		}

		#endregion

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if(Utility.IsPointInRectangle(new Point(e.X, e.Y), Workspace.Document.Bounds))
			{
				bool[,] pixelsChecked;
				base.OnMouseDown (e);
			
				// Create the Current Region
				if (!Workspace.Environment.IsSelectionEmpty)
				{
					currentRegion = Workspace.Environment.CreateSelectedRegion();
				}
				else
				{
					currentRegion = new Region();
					currentRegion.MakeInfinite();
				}

				// See if the Mouseclick is valid fo schizzle
				if(!currentRegion.IsVisible(new Point(e.X,e.Y)))
				{
					return;
				}
			
			
				// Set the current surface, color picked and color to draw
				currentSurface = ((BitmapLayer)Workspace.ActiveLayer).Surface;
				currentColor = currentSurface[e.X, e.Y];

				switch(e.Button)
				{
					case MouseButtons.Left:
						colorToDraw = Workspace.Environment.ForeColor; 
						break;

					case MouseButtons.Right: 
						colorToDraw = Workspace.Environment.BackColor; 
						break;

					default: 
						return;
				}

				pixelsChecked = new bool[Workspace.Document.Width + 1, Workspace.Document.Height + 1];
								
				minx = maxx = e.X; // These four variable help create the bounding box;
				miny = maxy = e.Y;

				// Bounding Box Pass
				FillScanLines(e.X, e.Y, pixelsChecked, false);
			
				Rectangle boundingBox = Rectangle.FromLTRB(minx, miny, maxx + 1, maxy + 1);
				HistoryAction ha = ((BitmapLayer)Workspace.ActiveLayer).CreateHistoryAction(this.name, this.Image, new Region(boundingBox));
				
				pixelsChecked = new bool[Workspace.Document.Width + 1, Workspace.Document.Height + 1];

				// Draw Pass
				FillScanLines(e.X, e.Y, pixelsChecked, true);

				Workspace.History.PushNewAction(ha);
				Workspace.ActiveLayer.Invalidate(boundingBox);
				Workspace.Update();
				pixelsChecked = null;
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp (e);
		}

		public PaintBucketTool(DocumentWorkspace parent) : base(parent)
		{
			//
			// TODO: Add constructor logic here
			//

			name = "Paint Bucket";
			toolBarImage = Utility.GetImageResource("Icons.PaintBucketIcon.bmp");
			cursor = new Cursor(Utility.GetResourceStream("Cursors.PaintBucketToolCursor.cur"));
			description = "Fills a Homogenous Color Region";
		}
	}
}
