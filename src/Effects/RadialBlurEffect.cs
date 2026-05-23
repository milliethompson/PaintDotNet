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
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for RadialBlurEffect.
    /// </summary>
    public unsafe class RadialBlurEffect
        : Effect,
          IConfigurableEffect
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("RadialBlurEffect.Name");
            }
        }

        public override void Render(RenderArgs dstArgs, RenderArgs srcArgs, Rectangle roi)
        {
            throw new NotImplementedException("Use the other render method");
        }

        public RadialBlurEffect()
            : base(StaticName,
                   PdnResources.GetImage("Icons.RadialBlurEffect.bmp"),
                   Shortcut.None,
                   PdnResources.GetString("Effects.Blurring.Submenu.Name"),
                   EffectDirectives.None)
        {
        }

        #region IConfigurableEffect Members

        public EffectConfigDialog CreateConfigDialog()
        {
            AmountEffectConfigDialog oacd = new AmountEffectConfigDialog();

            oacd.Text = StaticName;
            oacd.SliderLabel = PdnResources.GetString("RadialBlurEffect.ConfigDialog.RadialLabel");
            oacd.SliderMaximum = 360;
            oacd.SliderMinimum = 0;
            oacd.Icon = PdnResources.GetIconFromImage("Icons.RadialBlurEffect.bmp");

            return oacd;
        }

        private static void Rotate(ref int fx, ref int fy, int fr)
        {
            int cx = fx;
            int cy = fy;

            //sin(x) ~~ x
            //cos(x)~~ 1 - x^2/2
            fx = cx - ((cy >> 8) * fr >> 8) - ((cx >> 14) * (fr * fr >> 11) >> 8);
            fy = cy + ((cx >> 8) * fr >> 8) - ((cy >> 14) * (fr * fr >> 11) >> 8);
        }

        void IConfigurableEffect.Render(EffectConfigToken properties, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
            AmountEffectConfigToken token = (AmountEffectConfigToken)properties;
            int w = dstArgs.Bounds.Width;
            int h = dstArgs.Bounds.Height;
            int fcx = w << 15;
            int fcy = h << 15;
            int fr = (int)((double)token.Amount * Math.PI * 65536.0 / 181.0);
            int strideSrc = srcArgs.Surface.Stride;
            int strideDst = dstArgs.Surface.Stride;
            ColorBgra* srcPtr = srcArgs.Surface.GetRowAddressUnchecked(0);
            ColorBgra* dstPtr = dstArgs.Surface.GetRowAddressUnchecked(0);
            
            foreach (Rectangle rect in roi.GetRegionScansReadOnlyInt())
            {
                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    ColorBgra *dstRow = (ColorBgra *)(strideDst * y + (byte *)dstPtr);

                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        int fx = (x << 16) - fcx;
                        int fy = (y << 16) - fcy;
                        const int n = 64;

                        int fsr = fr / n;

                        int sr = 0;
                        int sg = 0;
                        int sb = 0;
                        int sa = 0;
                        int sc = 0;

                        ColorBgra *src = x + (ColorBgra *)((byte *)srcPtr + strideSrc * y);

                        sr += src->R * src->A;
                        sg += src->G * src->A;
                        sb += src->B * src->A;
                        sa += src->A;
                        ++sc;

                        int ox1 = fx;
                        int ox2 = fx;
                        int oy1 = fy;
                        int oy2 = fy;

                        for (int i = 0; i < n; ++i)
                        {
                            int u;
                            int v;

                            Rotate(ref ox1, ref oy1, fsr);
                            Rotate(ref ox2, ref oy2, -fsr);
                            
                            u = ox1 + fcx + 32768 >> 16;
                            v = oy1 + fcy + 32768 >> 16;

                            if (u > 0 && v > 0 && u < w && v < h)
                            {
                                src = u + (ColorBgra *)((byte *)srcPtr + strideSrc * v);

                                sr += src->R * src->A;
                                sg += src->G * src->A;
                                sb += src->B * src->A;
                                sa += src->A;
                                ++sc;
                            }

                            u = ox2 + fcx + 32768 >> 16;
                            v = oy2 + fcy + 32768 >> 16;

                            if (u > 0 && v > 0 && u < w&& v < h)
                            {
                                src = u + (ColorBgra *)((byte *)srcPtr + strideSrc * v);

                                sr += src->R * src->A;
                                sg += src->G * src->A;
                                sb += src->B * src->A;
                                sa += src->A;
                                ++sc;
                            }
                        }
                 
                        if (sa > 0)
                        {
                            dstRow[x] = ColorBgra.FromBgra(
                                Utility.ClampToByte(sb / sa),
                                Utility.ClampToByte(sg / sa),
                                Utility.ClampToByte(sr / sa),
                                Utility.ClampToByte(sa / sc)
                                );
                        }
                        else
                        {
                            dstRow[x].Bgra = 0;
                        }
                    }
                }
            }                       
        }

        #endregion
    }
}
