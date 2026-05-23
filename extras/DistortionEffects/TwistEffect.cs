using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using PaintDotNet;
using PaintDotNet.Effects;

namespace DistortionEffects
{
    [Guid("9A1EB3D9-0A36-4d32-9BB2-707D6E5A9D2C")]
    class TwistEffect : Effect
    {
        public static Image StaticImage
        {
            get
            {
                return (Image)MyResources.MyResourceManager.GetObject("TwistEffect");
            }
        }

        public static string StaticName
        {
            get
            {
                return MyResources.MyResourceManager.GetString("TwistEffect.Name");
            }
        }

        public static string StaticSubMenuName
        {
            get
            {
                return MyResources.MyResourceManager.GetString("DistortSubmenu.Name");
            }
        }

        public TwistEffect()
            :
            base(StaticName, StaticImage, System.Windows.Forms.Keys.None, StaticSubMenuName, true)
        {
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            TwoAmountsConfigDialog tacd = new TwoAmountsConfigDialog();

            tacd.Text = StaticName;
            tacd.Amount1Default = 45;
            tacd.Amount1Label = MyResources.MyResourceManager.GetString("TwistEffect.TwistAmount.Text");
            tacd.Amount1Maximum = 100;
            tacd.Amount1Minimum = -100;
            tacd.Amount2Default = 2;
            tacd.Amount2Label = MyResources.MyResourceManager.GetString("TwistEffect.Antialias.Text");
            tacd.Amount2Maximum = 5;
            tacd.Amount2Minimum = 0;

            return tacd;
        }

        public unsafe override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, System.Drawing.Rectangle[] rois, int startIndex, int length)
        {
            TwoAmountsConfigToken token = (TwoAmountsConfigToken)parameters;

            float twist = token.Amount1;
            Surface dst = dstArgs.Surface;
            Surface src = srcArgs.Surface;

            float hw = dst.Width / 2.0f;
            float hh = dst.Height / 2.0f;
            float maxrad = Math.Min(hw, hh);

            twist = twist * twist * Math.Sign(twist);

            int aaLevel = token.Amount2;
            int aaSamples = aaLevel * aaLevel + 1;
            PointF[] aaPoints = new PointF[aaSamples];

            for (int i = 0; i < aaSamples; ++i)
            {
                PointF pt = new PointF(
                    ((i * aaLevel) / (float)aaSamples),
                    i / (float)aaSamples);

                pt.X -= (int)pt.X;
                aaPoints[i] = pt;
            }

            for (int n = startIndex; n < startIndex + length; ++n)
            {
                Rectangle rect = rois[n];
                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    float j = y - hh;
                    ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);
                    ColorBgra* srcPtr = src.GetPointAddressUnchecked(rect.Left, y);

                    for (int x = rect.Left; x < rect.Right; x++)
                    {
                        float i = x - hw;

                        if (i * i + j * j > (maxrad + 1) * (maxrad + 1))
                        {
                            *dstPtr = *srcPtr;
                        }
                        else
                        {
                            int b = 0, g = 0, r = 0, a = 0;

                            foreach (PointF pt in aaPoints)
                            {
                                float u = i + pt.X;
                                float v = j + pt.Y;
                                double rad = Math.Sqrt(u * u + v * v);
                                double theta = Math.Atan2(v, u);

                                double t = 1 - rad / maxrad;

                                t = t < 0 ? 0 : t * t * t;

                                theta += t * twist / 100;
                                /*
                                ColorBgra sample = src.GetBilinearSample(
                                    hw + (float)(rad * Math.Cos(theta)),
                                    hh + (float)(rad * Math.Sin(theta)), true);
                                */
                                ColorBgra sample = *src.GetPointAddressUnchecked(
                                    (int)(hw + (float)(rad * Math.Cos(theta))),
                                    (int)(hh + (float)(rad * Math.Sin(theta))));

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
                        }

                        ++dstPtr;
                        ++srcPtr;
                    }
                }
            }
        }
    }
}
