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
        private PdnRegion currentRegion;
        private Surface surface;
        private int minX, maxX, minY, maxY;
		private Cursor cursorMouseUp, cursorMouseDown;

		public override char HotKey
		{
			get
			{
				return 'f';
			}
		}

        protected override void OnActivate()
        {
            base.OnActivate();

        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();

            if (currentRegion != null)
            {
                currentRegion.Dispose();
                currentRegion = null;
            }

            surface = null;
        }

        #region Recursive Fill
        protected void FillRecursively(int x, int y)
        {
            // if current color is right and clip is right
            if ((currentRegion.IsVisible(x, y)) &&
                (surface[x, y] == currentColor))
            {
                surface[x,y] = colorToDraw;
                FillRecursively(x, y - 1);
                FillRecursively(x - 1, y);
                FillRecursively(x + 1, y);
                FillRecursively(x, y + 1);
            }
        }
        #endregion

        #region Scan Line Fill
        private int FillLeft(int x, int y, bool[,] pixelsChecked, bool isDraw)
        {
            int lowX = x;
            
            while (lowX >= 0 && 
                   CheckColorBgra(surface[lowX, y], currentColor) && 
                   currentRegion.IsVisible(lowX,y) && 
                   !pixelsChecked[lowX, y])
            {
                if (isDraw)
                {
                    surface[lowX, y] = colorToDraw;
                }

                pixelsChecked[lowX, y] = true;
                lowX--;
            }
            
            if (lowX < minX)
            {
                minX = lowX;
            }

            return lowX + 1;
        }

        private int FillRight(int x, int y, bool[,] pixelsChecked, bool isDraw)
        {
            int highX = x;

            while ((highX <= Workspace.Document.Width - 1) && 
                   currentRegion.IsVisible(highX, y) &&
                   CheckColorBgra(surface[highX, y], currentColor) && 
                   !pixelsChecked[highX, y])
            {
                if (isDraw)
                {
                    surface[highX, y] = colorToDraw;
                }

                pixelsChecked[highX, y] = true;
                highX++;
            }
            
            if (highX >= maxX)
            {
                maxX = highX;
            }

            return highX - 1;
        }
        
		#region Tolerence Checking
		private bool CheckColorBgra(ColorBgra checkMe, ColorBgra source)
		{
			int ds = 0, t;
			t = checkMe.R - source.R;
			ds += t * t;
			t = checkMe.G - source.G;
			ds += t * t;
			t = checkMe.B - source.B;
			ds += t * t;
			return (ds/3 <= Workspace.Environment.Tolerance * Workspace.Environment.Tolerance);
		}
		#endregion

		private class FillScanLinesInfo
		{
			public int X;
			public int Y;
			public bool[,] PixelsChecked;
			public bool IsDraw;

			public FillScanLinesInfo(int x, int y, bool[,] pixelsChecked, bool isDraw)
			{
				this.X = x;
				this.Y = y;
				this.PixelsChecked = pixelsChecked;
				this.IsDraw = isDraw;
			}
		}

		private void FillScanLines(FillScanLinesInfo info, Queue infoQueue, int maxRecursionDepth)
        {
			if (maxRecursionDepth <= 0)
			{
				infoQueue.Enqueue(info);
				return;
			}

            int lowX = FillLeft(info.X, info.Y, info.PixelsChecked, info.IsDraw);
			int highX = FillRight(info.X + 1, info.Y, info.PixelsChecked, info.IsDraw);
			int i;
        
            // For Creating the Bounding Box///
            if (info.Y > maxY)
            {
                maxY = info.Y;
            }
            else if (info.Y < minY)
            {
                minY = info.Y;
            }

            // Vertical Scan
            for (i = lowX; i <= highX; i++)
            {
                if (info.Y > 0 && 
                    !info.PixelsChecked[i, info.Y - 1] && 
                    currentRegion.IsVisible(i, info.Y - 1) && 
                    CheckColorBgra(surface[i, info.Y - 1], currentColor))
                {
                    FillScanLines(new FillScanLinesInfo(i, info.Y - 1, info.PixelsChecked, info.IsDraw), infoQueue, maxRecursionDepth - 1);
                }

                if (info.Y < (Workspace.Document.Size.Height - 1) &&
                    !info.PixelsChecked[i, info.Y + 1] && 
                    currentRegion.IsVisible(i, info.Y + 1) && 
                    CheckColorBgra(surface[i, info.Y + 1], currentColor))
                {
                    FillScanLines(new FillScanLinesInfo(i, info.Y + 1, info.PixelsChecked, info.IsDraw), infoQueue, maxRecursionDepth - 1);
                }
            }
        }

		private void FillScanLines(FillScanLinesInfo info)
		{
			Queue infoQueue = new Queue();

			FillScanLines(info, infoQueue, 4);

			while (infoQueue.Count > 0)
			{
				FillScanLinesInfo fsli = (FillScanLinesInfo)infoQueue.Dequeue();
				FillScanLines(fsli, infoQueue, 4);
			}
		}
        #endregion
        
        #region DrawBoundingBox - Debug Purposes
        protected void DrawBoundingBox()
        {
            int i;
            int x, y;

            // Top
            y = minY;
            for (i = minX; i <= maxX; i++)
            {
                surface[i, y] = colorToDraw;
            }

            // Bottom
            y = maxY;
            for (i = minX; i <= maxX; i++)
            {
                surface[i, y] = colorToDraw;
            }

            // Left
            x = minX;
            for (i = minY; i <= maxY; i++)
            {
                surface[x, i] = colorToDraw;
            }

            // Right
            x = maxX;
            for (i = minY; i <= maxY; i++)
            {
                surface[x, i] = colorToDraw;
            }
        }

        #endregion

        protected override void OnMouseDown(MouseEventArgs e)
        {
			try
			{
				Cursor = cursorMouseDown;

				if (Utility.IsPointInRectangle(new Point(e.X, e.Y), Workspace.Document.Bounds))
				{
					base.OnMouseDown (e);

					bool[,] pixelsChecked;

					try
					{
						pixelsChecked = new bool[Workspace.Document.Width + 1, Workspace.Document.Height + 1];
					}

					catch (OutOfMemoryException)
					{
						Utility.ErrorBox(this.Workspace, "Not enough memory to perform this operation.");
						return;
					}

					// Create the Current Region
					if (!Workspace.Environment.IsSelectionEmpty)
					{
						currentRegion = Workspace.Environment.CreateSelectedRegion();
					}
					else
					{
						currentRegion = new PdnRegion(Workspace.Document.Bounds);
					}

					// See if the mouse click is valid
					if (!currentRegion.IsVisible(new Point(e.X,e.Y)))
					{
						return;
					}           
            
					// Set the current surface, color picked and color to draw
					surface = ((BitmapLayer)Workspace.ActiveLayer).Surface;
					currentColor = surface[e.X, e.Y];

					switch (e.Button)
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

					// These four variable help create the bounding box;
					minX = e.X;
					maxX = e.X; 
					minY = e.Y;
					maxY = e.Y;

					// Bounding Box Pass
					FillScanLines(new FillScanLinesInfo(e.X, e.Y, pixelsChecked, false));
            
					Rectangle boundingBox = Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
					HistoryAction ha = ((BitmapLayer)Workspace.ActiveLayer).CreateHistoryAction(this.name, this.Image, new PdnRegion(boundingBox));
                
					for (int x = 0; x < Workspace.Document.Width + 1; ++x)
					{
						for (int y = 0; y < Workspace.Document.Height + 1; ++y)
						{
							pixelsChecked[x, y] = false;
						}
					}

					// Draw Pass
					FillScanLines(new FillScanLinesInfo(e.X, e.Y, pixelsChecked, true));

					Workspace.History.PushNewAction(ha);
					Workspace.ActiveLayer.Invalidate(boundingBox);
					Workspace.Update();
					pixelsChecked = null;

					Utility.GCFullCollect();
				}
			}

			finally
			{
				Cursor = cursorMouseUp;
			}
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp (e);
			Cursor = cursorMouseUp;
        }

        public PaintBucketTool(DocumentWorkspace parent) : base(parent)
        {
            name = "Paint Bucket";
            toolBarImage = Utility.GetImageResource("Icons.PaintBucketIcon.bmp");

            description = "Fills a Homogenous Color Region";
			helpText = "Left click to fill a region with the foreground color, right click to fill with the background color";

			// cursor-transitions
			cursorMouseUp = new Cursor(Utility.GetResourceStream("Cursors.PaintBucketToolCursor.cur"));
			cursorMouseDown = new Cursor(Utility.GetResourceStream("Cursors.PaintBucketToolCursorMouseDown.cur"));
			Cursor = cursorMouseUp;
		}
	}
}
