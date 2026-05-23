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
    [Guid("3154E367-6B4D-4960-B4D8-F6D06E1C9C24")]
    class TileEffect : Effect
    {
        public static Image StaticImage
        {
            get
            {
                return (Image)MyResources.MyResourceManager.GetObject("TileEffect");
            }
        }

        public static string StaticName
        {
            get
            {
                return MyResources.MyResourceManager.GetString("TileEffect.Name");
            }
        }

        public static string StaticSubMenuName
        {
            get
            {
                return MyResources.MyResourceManager.GetString("DistortSubmenu.Name");
            }
        }

        public TileEffect()
            :
            base(StaticName, StaticImage, System.Windows.Forms.Keys.None, StaticSubMenuName, true)
        {
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            ThreeAmountsConfigDialog tacd = new ThreeAmountsConfigDialog();

            tacd.Text = StaticName;
            tacd.Amount1Label = MyResources.MyResourceManager.GetString("TileEffect.Rotation.Text");
            tacd.Amount1Default = 30;
            tacd.Amount1Minimum = -45;
            tacd.Amount1Maximum = 45;
            tacd.Amount2Label = MyResources.MyResourceManager.GetString("TileEffect.SquareSize.Text");
            tacd.Amount2Default = 40;
            tacd.Amount2Maximum = 200;
            tacd.Amount2Minimum = 2;
            tacd.Amount3Label = MyResources.MyResourceManager.GetString("TileEffect.Intensity.Text");
            tacd.Amount3Default = 8;
            tacd.Amount3Maximum = 20;
            tacd.Amount3Minimum = -20;

            return tacd;
        }

        public unsafe override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, System.Drawing.Rectangle[] rois, int startIndex, int length)
        {
            ThreeAmountsConfigToken token = (ThreeAmountsConfigToken)parameters;

            Surface dst = dstArgs.Surface;
            Surface src = srcArgs.Surface;
            int width = dst.Width;
            int height = dst.Height;
            float hw = width / 2.0f;
            float hh = height / 2.0f;
            float sin = (float)Math.Sin(token.Amount1 * Math.PI / 180.0);
            float cos = (float)Math.Cos(token.Amount1 * Math.PI / 180.0);
            float scale = (float)Math.PI / token.Amount2;
            float intensity = token.Amount3;

            intensity = intensity * intensity / 10 * Math.Sign(intensity);

            int aaLevel = 4;
            int aaSamples = aaLevel * aaLevel + 1;
            PointF[] aaPoints = new PointF[aaSamples];
            PointF[] aaSingle = new PointF[] { new PointF(0, 0) };

            for (int i = 0; i < aaSamples; ++i)
            {
                double x = (i * aaLevel) / (double)aaSamples;
                double y = i / (double)aaSamples;

                x -= (int)x;

                //RGSS + rotation to maximize AA quality
                aaPoints[i] = new PointF((float)(cos * x + sin * y), (float)(cos * y - sin * x));
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

                            float s =  cos * u + sin * v;
                            float t = -sin * u + cos * v;

                            s += intensity * (float)Math.Tan(s * scale);
                            t += intensity * (float)Math.Tan(t * scale);
                            u = cos * s - sin * t;
                            v = sin * s + cos * t;

                            int xSample = (int)(hw + u);
                            int ySample = (int)(hh + v);

                            xSample = (xSample + width) % width;
                            if (xSample < 0)//This makes it a little faster
                            {
                                xSample = (xSample + width) % width;
                            }

                            ySample = (ySample + height) % height;
                            if (ySample < 0)//This makes it a little faster
                            {
                                ySample = (ySample + height) % height;
                            }

                            ColorBgra sample = *src.GetPointAddressUnchecked(xSample, ySample);

                            b += sample.B;
                            g += sample.G;
                            r += sample.R;
                            a += sample.A;
                        }

                        *(dstPtr++) = ColorBgra.FromBgra(
                            (byte)(b / aaSamples),
                            (byte)(g / aaSamples),
                            (byte)(r / aaSamples),
                            (byte)(a / aaSamples));
                    }
                }
            }
        }
    }
}
