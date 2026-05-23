/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using PaintDotNet;
using PaintDotNet.Effects;

namespace PaintDotNet.Effects.Distortion
{
    [Guid("1445F876-356D-4a7c-B726-50457F6E7AEF")]
    public class PolarInversionEffect 
        : Effect
    {
        public static Image StaticImage
        {
            get
            {
                return PdnResources.GetImage("Icons.PolarInversionEffect.png");
            }
        }

        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("PolarInversionEffect.Name");
            }
        }

        public static string StaticSubMenuName
        {
            get
            {
                return PdnResources.GetString("DistortSubmenu.Name");
            }
        }

        public PolarInversionEffect()
            : base(StaticName, StaticImage, System.Windows.Forms.Keys.None, StaticSubMenuName, true)
        {
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            TwoAmountsConfigDialog tacd = new TwoAmountsConfigDialog();

            tacd.Text = StaticName;
            tacd.Amount1Label = PdnResources.GetString("PolarInversionEffect.PolarInversionAmount.Text");
            tacd.Amount1Default = 100;
            tacd.Amount1Minimum = -200;
            tacd.Amount1Maximum = 200;
            tacd.Amount2Label = PdnResources.GetString("PolarInversionEffect.Quality.Text");
            tacd.Amount2Default = 2;
            tacd.Amount2Maximum = 7;
            tacd.Amount2Minimum = 0;

            return tacd;
        }

        public unsafe override void Render(
            EffectConfigToken parameters, 
            RenderArgs dstArgs, 
            RenderArgs srcArgs, 
            System.Drawing.Rectangle[] rois, 
            int startIndex, 
            int length)
        {
            TwoAmountsConfigToken token = (TwoAmountsConfigToken)parameters;

            Surface dst = dstArgs.Surface;
            Surface src = srcArgs.Surface;

            float hw = dst.Width / 2.0f;
            float hh = dst.Height / 2.0f;
            float maxrad = Math.Min(hw, hh);
            float maxrad2 = maxrad * maxrad;
            float amt = token.Amount1 / 100.0f;

            int aaLevel = token.Amount2;
            int aaSamples = aaLevel * aaLevel + 1;
            PointF[] aaPoints = new PointF[aaSamples];
            PointF[] aaSingle = new PointF[] { new PointF(0, 0) };

            for (int i = 0; i < aaSamples; ++i)
            {
                double x = (i * aaLevel) / (double)aaSamples;
                double y = i / (double)aaSamples;

                x -= (int)x;

                // RGSS + rotation to maximize AA quality
                aaPoints[i] = new PointF((float)x, (float)y);
            }

            for (int n = startIndex; n < startIndex + length; ++n)
            {
                Rectangle rect = rois[n];
                
                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    float j = y - hh;
                    ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);

                    for (int x = rect.Left; x < rect.Right; x++)
                    {
                        int b = 0, g = 0, r = 0, a = 0;
                        float i = x - hw;

                        foreach (PointF pt in aaPoints)
                        {
                            float u = i + pt.X;
                            float v = j - pt.Y;
                            float scale = Utility.Lerp(1, maxrad2 / (u * u + v * v), amt);
                            float xp = u * scale;
                            float yp = v * scale;

                            ColorBgra sample = src.GetBilinearSample(xp + hw, yp + hh, true);

                            b += sample.B;
                            g += sample.G;
                            r += sample.R;
                            a += sample.A;
                        }

                        *dstPtr = ColorBgra.FromBgra(
                            (byte)(b / aaSamples),
                            (byte)(g / aaSamples),
                            (byte)(r / aaSamples),
                            (byte)(a / aaSamples));

                        ++dstPtr;
                    }
                }
            }
        }
    }
}
