/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PaintDotNet.Effects
{
    public sealed class PolarInversionEffect 
        : PropertyBasedEffect
    {
        public static Image StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.PolarInversionEffect.png").Reference;
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
                return SubmenuNames.Distort;
            }
        }

        public PolarInversionEffect()
            : base(StaticName, StaticImage, StaticSubMenuName, EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            Amount,
            Quality
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new DoubleProperty(PropertyNames.Amount, 1.0, -2.0, +2.0));
            props.Add(new Int32Property(PropertyNames.Quality, 2, 1, 5));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Amount, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("PolarInversionEffect.PolarInversionAmount.Text"));
            configUI.SetPropertyControlValue(PropertyNames.Amount, ControlInfoPropertyNames.UseExponentialScale, true);
            configUI.SetPropertyControlValue(PropertyNames.Amount, ControlInfoPropertyNames.SliderLargeChange, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.Amount, ControlInfoPropertyNames.SliderSmallChange, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.Amount, ControlInfoPropertyNames.UpDownIncrement, 0.01);

            configUI.SetPropertyControlValue(PropertyNames.Quality, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("PolarInversionEffect.Quality.Text"));

            return configUI;
        }

        private double amount;
        private int quality;

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.amount = newToken.GetProperty<DoubleProperty>(PropertyNames.Amount).Value;
            this.quality = newToken.GetProperty<Int32Property>(PropertyNames.Quality).Value;
            
            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected unsafe override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            Surface dst = DstArgs.Surface;
            Surface src = SrcArgs.Surface;

            float hw = dst.Width / 2.0f;
            float hh = dst.Height / 2.0f;
            float maxrad = Math.Min(hw, hh);
            float maxrad2 = maxrad * maxrad;
            float amt = (float)this.amount;

            int aaLevel = this.quality;
            int aaSamples = aaLevel * aaLevel;
            PointF* aaPoints = stackalloc PointF[aaSamples];
            Utility.GetRgssOffsets(aaPoints, aaSamples, aaLevel);

            ColorBgra* samples = stackalloc ColorBgra[aaSamples];

            for (int n = startIndex; n < startIndex + length; ++n)
            {
                Rectangle rect = rois[n];
                
                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    float j = y - hh;
                    ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);

                    for (int x = rect.Left; x < rect.Right; x++)
                    {
                        float i = x - hw;
                        int sampleCount = 0;

                        for (int z = 0; z < aaSamples; ++z)
                        {
                            float u = i + aaPoints[z].X;
                            float v = j - aaPoints[z].Y;
                            float scale = Utility.Lerp(1, maxrad2 / (u * u + v * v), amt);
                            float xp = u * scale;
                            float yp = v * scale;

                            samples[sampleCount] = src.GetBilinearSampleWrapped(xp + hw, yp + hh);
                            ++sampleCount;
                        }

                        *dstPtr = ColorBgra.Blend(samples, sampleCount);
                        ++dstPtr;
                    }
                }
            }
        }
    }
}
