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
                return PdnResources.GetString("BrightnessAndContrastAdjustment.Name");
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
            int multiply = token.Multiply;
            int divide = token.Divide;
            byte[] rgbTable = token.RgbTable;

            unsafe
            {
                foreach (Rectangle rect in roi.GetRegionScansReadOnlyInt())
                {
                    for (int y = rect.Top; y < rect.Bottom; ++y)
                    {
                        ColorBgra *srcRowPtr = srcArgs.Surface.GetPointAddress(rect.Left, y);
                        ColorBgra *dstRowPtr = dstArgs.Surface.GetPointAddress(rect.Left, y);
                        ColorBgra *dstRowEndPtr = dstRowPtr + rect.Width;

                        if (divide == 0)
                        {
                            while (dstRowPtr < dstRowEndPtr)
                            {
                                ColorBgra col = *srcRowPtr;
                                int i = col.GetIntensityByte();
                                uint c = rgbTable[i];
                                dstRowPtr->Bgra = (col.Bgra & 0xff000000) | c | (c << 8) | (c << 16);

                                ++dstRowPtr;
                                ++srcRowPtr;
                            }
                        }
                        else
                        {
                            while (dstRowPtr < dstRowEndPtr)
                            {
                                ColorBgra col = *srcRowPtr;
                                int i = col.GetIntensityByte();
                                int shiftIndex = i * 256;

                                col.R = rgbTable[shiftIndex + col.R];
                                col.G = rgbTable[shiftIndex + col.G]; 
                                col.B = rgbTable[shiftIndex + col.B];

                                *dstRowPtr = col;
                                ++dstRowPtr;
                                ++srcRowPtr;
                            }
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
                   PdnResources.GetImage("Icons.BrightnessAndContrastAdjustment.bmp"),
                   System.Windows.Forms.Shortcut.CtrlShiftC)
        {
        }
    }
}
