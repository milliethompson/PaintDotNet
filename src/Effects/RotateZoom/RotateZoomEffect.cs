/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.Effects;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for Rotate / Zoom Effect.
    /// </summary>
    [EffectCategory(EffectCategory.Adjustment)]
    [EffectTypeHint(EffectTypeHint.Fast)]
    public class RotateZoomEffect
        : Effect,
          IConfigurableEffect
    {
        private static readonly ColorBgra seeThroughColor = ColorBgra.FromBgra(255, 255, 255, 0);

        public static string StaticName
        {
            get
            {
                return "Rotate / Zoom";
            }
        }

        public EffectConfigDialog CreateConfigDialog()
        {
            return new RotateZoomEffectConfigDialog();
        }

        public unsafe void Render(EffectConfigToken properties, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
            RotateZoomEffectConfigToken token = (RotateZoomEffectConfigToken)properties;
            RotateZoomEffectConfigToken.RzInfo rzInfo = token.ComputedOnce;
            Rectangle bounds = this.EnvironmentParameters.GetSelection(dstArgs.Bounds).GetBoundsInt();
            bounds.Intersect(dstArgs.Bounds);
			Point center = new Point((bounds.Left + bounds.Right) / 2, (bounds.Top + bounds.Bottom) / 2);
            Rectangle[] rects = roi.GetRegionScansReadOnlyInt();

            uint srcMask;

            if (token.SourceAsBackground)
            {
                srcMask = 0xffffffff;
            }
            else
            {
                srcMask = 0;
            }

            foreach (Rectangle rect in rects)
            {
                float sxul = (float)center.X + ((((float)(rect.Left - center.X) * rzInfo.angleCos) - ((float)(rect.Top - center.Y) * rzInfo.angleSin)) * token.Zoom);
                float syul = (float)center.Y + ((((float)(rect.Left - center.X) * rzInfo.angleSin) + ((float)(rect.Top - center.Y) * rzInfo.angleCos)) * token.Zoom);

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    float xp = sxul;
                    float yp = syul;

                    int xpInt = (int)xp;
                    int ypInt = (int)yp;

                    ColorBgra *dstPtr = dstArgs.Surface.GetPointAddressUnchecked(rect.Left, y);

                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        float xLerp = xp - (float)Math.Floor(xp);
                        float yLerp = yp - (float)Math.Floor(yp);

                        // compute weights
                        float ulw = (1 - xLerp) * (1 - yLerp);
                        float urw = xLerp * (1 - yLerp);
                        float llw = (1 - xLerp) * yLerp;
                        float lrw = xLerp * yLerp;

						ColorBgra backColor = ColorBgra.FromUInt32((srcArgs.Surface.GetPointUnchecked(x, y).Bgra & srcMask) | (seeThroughColor.Bgra & ~srcMask));

                        ColorBgra ulc;
                        ColorBgra lrc;
                        ColorBgra urc;
                        ColorBgra llc;

                        if (Utility.IsPointInRectangle(xpInt, ypInt, bounds) &&
                            Utility.IsPointInRectangle(xpInt + 1, ypInt + 1, bounds))
                        {
                            ulc = srcArgs.Surface.GetPointUnchecked(xpInt, ypInt);
                            urc = srcArgs.Surface.GetPointUnchecked(xpInt + 1, ypInt);
                            lrc = srcArgs.Surface.GetPointUnchecked(xpInt + 1, ypInt + 1);
                            llc = srcArgs.Surface.GetPointUnchecked(xpInt, ypInt + 1);
                        }
                        else
                        {
                            if (Utility.IsPointInRectangle(xpInt, ypInt, bounds))
                            {
                                ulc = srcArgs.Surface.GetPointUnchecked(xpInt, ypInt);
                            }
                            else
                            {
                                ulc = backColor;
                            }

                            if (Utility.IsPointInRectangle(xpInt + 1, ypInt + 1, bounds))
                            {
                                lrc = srcArgs.Surface.GetPointUnchecked(xpInt + 1, ypInt + 1);
                            }
                            else
                            {
                                lrc = backColor;
                            }

                            if (Utility.IsPointInRectangle(xpInt + 1, ypInt, bounds))
                            {
                                urc = srcArgs.Surface.GetPointUnchecked(xpInt + 1, ypInt);
                            }
                            else
                            {
                                urc = backColor;
                            }

                            if (Utility.IsPointInRectangle(xpInt, ypInt + 1, bounds))
                            {
                                llc = srcArgs.Surface.GetPointUnchecked(xpInt, ypInt + 1);
                            }
                            else
                            {
                                llc = backColor;
                            }
                        }

                        float b = (float)Math.Min(255.0f, ((float)ulc.B * ulw) + ((float)urc.B * urw) + ((float)llc.B * llw) + ((float)lrc.B * lrw));
                        float g = (float)Math.Min(255.0f, ((float)ulc.G * ulw) + ((float)urc.G * urw) + ((float)llc.G * llw) + ((float)lrc.G * lrw));
                        float r = (float)Math.Min(255.0f, ((float)ulc.R * ulw) + ((float)urc.R * urw) + ((float)llc.R * llw) + ((float)lrc.R * lrw));
                        float a = (float)Math.Min(255.0f, ((float)ulc.A * ulw) + ((float)urc.A * urw) + ((float)llc.A * llw) + ((float)lrc.A * lrw));
                        
                        *dstPtr = ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, (byte)a);
                        ++dstPtr;

                        xp += rzInfo.dsxddx;
                        yp += rzInfo.dsyddx;

                        xpInt = (int)xp;
                        ypInt = (int)yp;
                    }

                    sxul += rzInfo.dsxddy;
                    syul += rzInfo.dsyddy;
                }
            }
        }

        public RotateZoomEffect()
            : base(StaticName, 
                   "Rotates and zooms an image", 
			       Utility.GetImageResource("Icons.RotateZoomIcon.bmp"), 
			       System.Windows.Forms.Shortcut.CtrlShiftZ)
        {
        }
    }
}
