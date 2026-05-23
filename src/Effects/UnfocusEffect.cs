/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace PaintDotNet.Effects
{
    public sealed class UnfocusEffect
        : LocalHistogramEffect
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("UnfocusEffect.Name");
            }
        }

        public static ImageResource StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.UnfocusEffectIcon.png");
            }
        }

        public UnfocusEffect() 
            : base(StaticName, 
                   StaticImage.Reference,
                   SubmenuNames.Blurs,
                   EffectFlags.Configurable)
        { 
        }

        public enum PropertyNames
        {
            Radius
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Radius, 4, 1, 200));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Radius, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("UnfocusEffect.ConfigDialog.AmountLabel"));

            // TODO: units label
            //acd.SliderUnitsName = PdnResources.GetString("UnfocusEffect.ConfigDialog.UnitsLabel");

            return configUI;
        }

        private int radius;

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.radius = newToken.GetProperty<Int32Property>(PropertyNames.Radius).Value;
            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        public unsafe override ColorBgra Apply(ColorBgra src, int area, int* hb, int* hg, int* hr, int* ha)
        {
            // TODO: do this calculation proper based on alpha values

            int b = 0;
            int g = 0;
            int r = 0;
            int a = 0;

            for (int i = 1; i < 256; ++i)
            {
                b += i * hb[i];
                g += i * hg[i];
                r += i * hr[i];
                a += i * ha[i];
            }

            ColorBgra c = ColorBgra.FromBgraClamped(b / area, g / area, r / area, a / area);
            return c;
        }

        protected unsafe override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            foreach (Rectangle rect in rois)
            {
                RenderRect(this.radius, SrcArgs.Surface, DstArgs.Surface, rect);
            }
        }
    }
}
