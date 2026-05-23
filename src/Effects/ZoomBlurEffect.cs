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
    public unsafe class ZoomBlurEffect
        : Effect
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("ZoomBlurEffect.Name");
            }
        }

        public static Image StaticImage
        {
            get
            {
                return PdnResources.GetImage("Icons.ZoomBlurEffect.png");
            }
        }

        public ZoomBlurEffect()
            : base(StaticName,
                   StaticImage,
                   Keys.None,
                   PdnResources.GetString("Effects.Blurring.Submenu.Name"),
                   EffectDirectives.None,
                   true)
        {
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            AmountEffectConfigDialog oacd = new AmountEffectConfigDialog();

            oacd.Text = StaticName;
            oacd.SliderLabel = PdnResources.GetString("ZoomBlurEffect.ConfigDialog.AmountLabel");
            oacd.SliderMaximum = 100;
            oacd.SliderMinimum = 0;
            oacd.SliderInitialValue = 10;
            oacd.Icon = PdnResources.GetIconFromImage("Icons.ZoomBlurEffect.png");

            return oacd;
        }

        public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, 
            Rectangle[] rois, int startIndex, int length)
        {
            AmountEffectConfigToken token = (AmountEffectConfigToken)parameters;
            Surface dst = dstArgs.Surface;
            Surface src = srcArgs.Surface;
            long w = dst.Width;
            long h = dst.Height;
            long fcx = w << 15;
            long fcy = h << 15;
            long fz = token.Amount;
            
            for (int r = startIndex; r < startIndex + length; ++r)
            {
                Rectangle rect = rois[r];

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    ColorBgra *dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);
                    ColorBgra *srcPtr = src.GetPointAddressUnchecked(rect.Left, y);

                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        long fx = (x << 16) - fcx;
                        long fy = (y << 16) - fcy;
                        const int n = 64;

                        int sr = 0;
                        int sg = 0;
                        int sb = 0;
                        int sa = 0;
                        int sc = 0;

                        sr += srcPtr->R * srcPtr->A;
                        sg += srcPtr->G * srcPtr->A;
                        sb += srcPtr->B * srcPtr->A;
                        sa += srcPtr->A;
                        ++sc;

                        for (int i = 0; i < n; ++i)
                        {
                            fx -= ((fx >> 4) * fz) >> 10;
                            fy -= ((fy >> 4) * fz) >> 10;

                            int u = (int)(fx + fcx + 32768 >> 16);
                            int v = (int)(fy + fcy + 32768 >> 16);

                            ColorBgra *srcPtr2 = src.GetPointAddress(u, v);

                            sr += srcPtr2->R * srcPtr2->A;
                            sg += srcPtr2->G * srcPtr2->A;
                            sb += srcPtr2->B * srcPtr2->A;
                            sa += srcPtr2->A;
                            ++sc;
                        }
                 
                        if (sa != 0)
                        {
                            *dstPtr = ColorBgra.FromBgra(
                                Utility.ClampToByte(sb / sa),
                                Utility.ClampToByte(sg / sa),
                                Utility.ClampToByte(sr / sa),
                                Utility.ClampToByte(sa / sc));
                        }
                        else
                        {
                            dstPtr->Bgra = 0;
                        }

                        ++srcPtr;
                        ++dstPtr;
                    }
                }
            }                       
        }
    }
}
