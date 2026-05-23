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
    /// Summary description for BrightnessAndContrastAdjustment.
    /// </summary>
    [EffectCategory(EffectCategory.Adjustment)]
    [EffectTypeHint(EffectTypeHint.Unary | EffectTypeHint.Fast)]
    public class BrightnessAndContrastAdjustment
        : Effect,
          IConfigurableEffect
    {
        public static string StaticName
        {
            get
            {
                return "Brightness / Contrast";
            }
        }

        public EffectConfigDialog CreateConfigDialog()
        {
            return new BrightnessAndContrastAdjustmentConfigDialog();
        }

		public void Render(EffectConfigToken properties, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
		{
			BrightnessAndContrastAdjustmentConfigToken token = (BrightnessAndContrastAdjustmentConfigToken)properties;
			int contrast = token.Contrast;
			int brightness = token.Brightness;

			int multiply, divide;
			if (contrast < 0) 
			{
				multiply = contrast + 100;
				divide = 100;
			} 
			else if (contrast > 0) 
			{
				multiply = 100;
				divide = 100 - contrast;
			} 
			else 
			{
				multiply = 1;
				divide = 1;
			}

            unsafe
            {
                foreach (Rectangle rect in roi.GetRegionScansReadOnlyInt())
                {
                    for (int y = rect.Top; y < rect.Bottom; ++y)
                    {
                        ColorBgra *srcRowPtr = srcArgs.Surface.GetPointAddress(rect.Left, y);
                        ColorBgra *dstRowPtr = dstArgs.Surface.GetPointAddress(rect.Left, y);

                        for (int x = 0; x < rect.Width; ++x)
                        {
                            // read
                            ColorBgra col = *srcRowPtr;
                            ++srcRowPtr;

							for (int c = 0; c < 3; c++) 
							{
								if (divide != 0) 
								{
									col[c] = Utility.ClampToByte(127 + (col[c] - 127 + brightness) * multiply / divide);
								} 
								else 
								{
									col[c] = Utility.ClampToByte((col[c] + brightness > 127) ? 255 : 0);
								}
							}

                            // store
                            *dstRowPtr = col;
                            ++dstRowPtr;
                        }
                    }
                }
            }

            return;
        }

        public override void Render(RenderArgs dstArgs, RenderArgs srcArgs, System.Drawing.Rectangle roi)
        {
            throw new InvalidOperationException("BrightnessAndContrastEffect must be used via the other Render overload");
        }

        public BrightnessAndContrastAdjustment()
            : base(StaticName,
                   "Adjusts the brightness and contrast levels of an image", 
                   Utility.GetImageResource("Icons.BrightnessAndContrastAdjustment.bmp"),
                   System.Windows.Forms.Shortcut.CtrlShiftC)
        {
        }
    }
}
