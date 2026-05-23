/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
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
        : Effect
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("PixelateEffect.Name");
            }
        }

        public PixelateEffect() 
            : base(StaticName,
                   PdnResources.GetImage("Icons.PixelateEffect.png"),
                   true)
        {
        }

        private ColorBgra RenderPixel(int x, int y, RenderArgs src, int cellSize)
        {
            Rectangle cell = GetCellBox(x, y, cellSize);
            cell.Intersect(src.Bounds);
            
            int left = cell.Left;
            int right = cell.Right - 1;
            int bottom = cell.Bottom - 1;
            int top = cell.Top;
 
            ColorBgra colorTopLeft = src.Surface[left, top];
            ColorBgra colorTopRight = src.Surface[right, top];
            ColorBgra colorBottomLeft = src.Surface[left, bottom];
            ColorBgra colorBottomRight = src.Surface[right, bottom];

            byte a = (byte)((colorTopLeft.A + colorTopRight.A + colorBottomLeft.A + colorBottomRight.A) / 4);
            byte r = (byte)((colorTopLeft.R + colorTopRight.R + colorBottomLeft.R + colorBottomRight.R) / 4);
            byte g = (byte)((colorTopLeft.G + colorTopRight.G + colorBottomLeft.G + colorBottomRight.G) / 4);
            byte b = (byte)((colorTopLeft.B + colorTopRight.B + colorBottomLeft.B + colorBottomRight.B) / 4);   
        
            return ColorBgra.FromBgra((byte)b,(byte)g,(byte)r,(byte)a);
        }

        private Rectangle GetCellBox(int x, int y, int cellSize)
        {
            int widthBoxNum  = x % cellSize;
            int heightBoxNum = y % cellSize;
            Point leftUpper = new Point(x - widthBoxNum, y - heightBoxNum);
            Rectangle returnMe = new Rectangle(leftUpper, new Size(cellSize, cellSize));
            return returnMe;
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            AmountEffectConfigDialog aecg = new AmountEffectConfigDialog();

            aecg.Effect = this;
            aecg.Text = PdnResources.GetString("PixelateEffect.Name");
            aecg.SliderMinimum = 1;
            aecg.SliderMaximum = 100;
            aecg.SliderLabel = PdnResources.GetString("PixelateEffect.ConfigDialog.SliderLabel");
            aecg.SliderUnitsName = PdnResources.GetString("PixelateEffect.ConfigDialog.SliderUnitsName");
            aecg.Icon = PdnResources.GetIconFromImage("Icons.PixelateEffect.png");

            return aecg;
        }

        public unsafe override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
        {
            AmountEffectConfigToken aecd = (AmountEffectConfigToken)parameters;

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Rectangle rect = rois[i];

                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    int yEnd = y + 1;

                    for (int x = rect.Left; x < rect.Right; x++)
                    {
                        Rectangle cellRect = GetCellBox(x, y, aecd.Amount);
                        cellRect.Intersect(dstArgs.Bounds);
                        ColorBgra color = RenderPixel(x, y, srcArgs, aecd.Amount);

                        int xEnd = Math.Min(rect.Right, cellRect.Right);
                        yEnd = Math.Min(rect.Bottom, cellRect.Bottom);

                        for (int y2 = y; y2 < yEnd; ++y2)
                        {
                            ColorBgra *ptr = dstArgs.Surface.GetPointAddress(x, y2);

                            for (int x2 = x; x2 < xEnd; ++x2)
                            {
                                ptr->Bgra = color.Bgra;
                                ++ptr;
                            }
                        }

                        x = xEnd - 1;
                    }

                    y = yEnd - 1;
                }
            }
        }
    }
}
