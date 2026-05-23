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
        private static int tolerance = 10;

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
        
        private bool IsWithinTolerance(byte check, byte src)
        {
            int a = (int)check;
            int b = (int)src;
            int diff = a - b;
            diff = Math.Abs(diff);

            if (diff <= tolerance)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }

        private bool CheckColorBgra(ColorBgra checkMe, ColorBgra source)
        {
            if (IsWithinTolerance(checkMe.R, source.R) &&
                IsWithinTolerance(checkMe.G, source.G) &&
                IsWithinTolerance(checkMe.B, source.B))
            {
                return true;
            }
            else 
            {
                return false;
            }
        }

        private void FillScanLines(int x, int y, bool[,] pixelsChecked, bool isDraw)
        {
            int lowX = FillLeft(x, y, pixelsChecked, isDraw);
            int highX = FillRight(x + 1, y, pixelsChecked, isDraw);
            int i;
        
            // For Creating the Bounding Box///
            if (y > maxY)
            {
                maxY = y;
            }
            else if (y < minY)
            {
                minY = y;
            }

            // Vertical Scan
            for (i = lowX; i <= highX; i++)
            {
                if (y > 0 && 
                    !pixelsChecked[i, y - 1] && 
                    currentRegion.IsVisible(i, y - 1) && 
                    CheckColorBgra(surface[i, y - 1], currentColor))
                {
                    FillScanLines(i,y-1,pixelsChecked, isDraw);
                }

                if (y < (Workspace.Document.Size.Height - 1) &&
                    !pixelsChecked[i, y + 1] && 
                    currentRegion.IsVisible(i, y + 1) && 
                    CheckColorBgra(surface[i, y + 1], currentColor))
                {
                    FillScanLines(i, y + 1, pixelsChecked, isDraw);
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
            if (Utility.IsPointInRectangle(new Point(e.X, e.Y), Workspace.Document.Bounds))
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
                    currentRegion = new PdnRegion(Workspace.Document.Bounds);
                }

                // See if the Mouseclick is valid fo schizzle
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

                pixelsChecked = new bool[Workspace.Document.Width + 1, Workspace.Document.Height + 1];
                            
                // These four variable help create the bounding box;
                minX = e.X;
                maxX = e.X; 
                minY = e.Y;
                maxY = e.Y;

                // Bounding Box Pass
                FillScanLines(e.X, e.Y, pixelsChecked, false);
            
                Rectangle boundingBox = Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
                HistoryAction ha = ((BitmapLayer)Workspace.ActiveLayer).CreateHistoryAction(this.name, this.Image, new PdnRegion(boundingBox));
                
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
