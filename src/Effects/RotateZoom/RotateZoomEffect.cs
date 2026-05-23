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
    //[EffectSubMenu("Rick's Plugins")]
    [EffectCategory(EffectCategory.Adjustment)]
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

        public void Render(EffectConfigToken properties, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
            RotateZoomEffectConfigToken token = (RotateZoomEffectConfigToken)properties;
            RotateZoomEffectConfigToken.RzInfo rzInfo = token.ComputedOnce;
            Rectangle bounds = Rectangle.Round(this.Selection.GetBounds());
			Point center = new Point((bounds.Left + bounds.Right) / 2, (bounds.Top + bounds.Bottom) / 2);
            Rectangle[] rects = roi.GetRegionScansReadOnlyInt();

            foreach (Rectangle rect in rects)
            {
                double sxul = (double)center.X + ((((double)(rect.Left - center.X) * rzInfo.angleCos) - ((double)(rect.Top - center.Y) * rzInfo.angleSin)) * token.Zoom);
                double syul = (double)center.Y + ((((double)(rect.Left - center.X) * rzInfo.angleSin) + ((double)(rect.Top - center.Y) * rzInfo.angleCos)) * token.Zoom);

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    double xp = sxul;
                    double yp = syul;

                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        double xLerp = xp - Math.Floor(xp);
                        double yLerp = yp - Math.Floor(yp);

                        // compute weights
                        double ulw = (1 - xLerp) * (1 - yLerp);
                        double urw = xLerp * (1 - yLerp);
                        double llw = (1 - xLerp) * yLerp;
                        double lrw = xLerp * yLerp;

                        // compute source Points
                        Point ulp = new Point((int)xp, (int)yp);
                        Point urp = new Point(ulp.X + 1, ulp.Y);
                        Point llp = new Point(ulp.X, ulp.Y + 1);
                        Point lrp = new Point(ulp.X + 1, ulp.Y + 1);

						ColorBgra backColor = token.SourceAsBackground ? srcArgs.Surface[x, y] : seeThroughColor;

                        ColorBgra ulc;
                        if (Utility.IsPointInRectangle(ulp, bounds))
                        {
                            ulc = srcArgs.Surface[ulp];
                        }
                        else
                        {
                            ulc = backColor;
                        }

                        ColorBgra urc;
                        if (Utility.IsPointInRectangle(urp, bounds))
                        {
                            urc = srcArgs.Surface[urp];
                        }
                        else
                        {
                            urc = backColor;
                        }

                        ColorBgra llc;
                        if (Utility.IsPointInRectangle(llp, bounds))
                        {
                            llc = srcArgs.Surface[llp];
                        }
                        else
                        {
                            llc = backColor;
                        }

                        ColorBgra lrc;
                        if (Utility.IsPointInRectangle(lrp, bounds))
                        {
                            lrc = srcArgs.Surface[lrp];
                        }
                        else
                        {
                            lrc = backColor;
                        }

                        double b = Math.Min(255.0f, ((double)ulc.B * ulw) + ((double)urc.B * urw) + ((double)llc.B * llw) + ((double)lrc.B * lrw));
                        double g = Math.Min(255.0f, ((double)ulc.G * ulw) + ((double)urc.G * urw) + ((double)llc.G * llw) + ((double)lrc.G * lrw));
                        double r = Math.Min(255.0f, ((double)ulc.R * ulw) + ((double)urc.R * urw) + ((double)llc.R * llw) + ((double)lrc.R * lrw));
                        double a = Math.Min(255.0f, ((double)ulc.A * ulw) + ((double)urc.A * urw) + ((double)llc.A * llw) + ((double)lrc.A * lrw));
                        
                        dstArgs.Surface[x, y] = ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, (byte)a);

                        xp += rzInfo.dsxddx;
                        yp += rzInfo.dsyddx;
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
