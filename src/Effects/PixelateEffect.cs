/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for PixelateEffect.
    /// </summary>
    [EffectTypeHint(EffectTypeHint.Fast)]
    public class PixelateEffect 
        : Effect, 
          IConfigurableEffect
    {
		public PixelateEffect() 
			: base("Pixelate", 
                   "Tiles a Picture", 
                   Utility.GetImageResource("Icons.PixelateEffect.bmp"))
		{
		}

        public override void Render(RenderArgs dstArgs, RenderArgs srcArgs, Rectangle roi)
        {
            throw new InvalidOperationException("PixelateEffect must be used via the other Render overload");
        }

        private ColorBgra RenderPixel(int x, int y, RenderArgs src, int cellSize)
        {
            Rectangle cell = GetCellBox(x,y,src, cellSize);
            
            int left = cell.Left;
            int right = cell.Right - 1;
            int bottom = cell.Bottom - 1;
            int top = cell.Top;

            Point topLeft = new Point(left, top);
            Point topRight = new Point(right, top);
            Point bottomLeft = new Point(left, bottom);
            Point bottomRight = new Point(right, bottom); 

            // Check for Overlapping Points
            if (!Utility.IsPointInRectangle(topLeft, src.Bounds))
            {
                topLeft = new Point(src.Bounds.Left, src.Bounds.Top);
            }

            if (!Utility.IsPointInRectangle(topRight, src.Bounds))
            {
                topRight = new Point(src.Bounds.Right - 1, src.Bounds.Top);
            }

            if (!Utility.IsPointInRectangle(bottomLeft, src.Bounds))
            {
                bottomLeft = new Point(src.Bounds.Left, src.Bounds.Bottom - 1);
            }

            if (!Utility.IsPointInRectangle(bottomRight, src.Bounds))
            {
                bottomRight = new Point(src.Bounds.Right - 1, src.Bounds.Bottom - 1);
            }

            ColorBgra colorTopLeft     = src.Surface[topLeft.X, topLeft.Y];
            ColorBgra colorTopRight    = src.Surface[topRight.X, topRight.Y];
            ColorBgra colorBottomLeft  = src.Surface[bottomLeft.X, bottomLeft.Y];
            ColorBgra colorBottomRight = src.Surface[bottomRight.X, bottomRight.Y];

            byte a = (byte)((colorTopLeft.A + colorTopRight.A + colorBottomLeft.A + colorBottomRight.A) / 4);
            byte r = (byte)((colorTopLeft.R + colorTopRight.R + colorBottomLeft.R + colorBottomRight.R) / 4);
            byte g = (byte)((colorTopLeft.G + colorTopRight.G + colorBottomLeft.G + colorBottomRight.G) / 4);
            byte b = (byte)((colorTopLeft.B + colorTopRight.B + colorBottomLeft.B + colorBottomRight.B) / 4);   
        
            return ColorBgra.FromBgra((byte)b,(byte)g,(byte)r,(byte)a);
        }

        private Rectangle GetCellBox(int x, int y, RenderArgs src, int cellSize)
        {
            int widthBoxNum  = x % cellSize;
            int heightBoxNum = y % cellSize;
            Point leftUpper = new Point(x - widthBoxNum, y - heightBoxNum);
            Rectangle returnMe = new Rectangle(leftUpper, new Size(cellSize, cellSize));
            return returnMe;
        }
        #region IConfigurableEffect Members

        public EffectConfigDialog CreateConfigDialog()
        {
            AmountEffectConfigDialog aecg = new AmountEffectConfigDialog();

            aecg.Effect = this;
            aecg.Text = "Pixelate";
            aecg.SliderMinimum = 1;
            aecg.SliderMaximum = 100;
            aecg.SliderLabel = "Cell Size";
            aecg.SliderUnitsName = "pixels";
            aecg.Icon = Utility.GetIconResource("Icons.PixelateEffect.bmp");

            return aecg;
        }

        void IConfigurableEffect.Render(EffectConfigToken properties, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
			AmountEffectConfigToken aecd = (AmountEffectConfigToken)properties;

            foreach (Rectangle rect in roi.GetRegionScansReadOnlyInt())
            {
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    for (int y = rect.Top; y < rect.Bottom; y++)
                    {
                        dstArgs.Surface[x,y] = RenderPixel(x,y,srcArgs, aecd.Amount);
                    }
                }
            }
        }

        #endregion
    }

}
