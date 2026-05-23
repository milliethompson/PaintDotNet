/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for RotateZoomEffectConfigToken.
    /// </summary>
    public class RotateZoomEffectConfigToken
        : EffectConfigToken
    {
        internal struct RzInfo
        {
            // gradients
            public float dsxddx;
            public float dsxddy;
            public float dsyddy;
            public float dsyddx;

            // degrees -> radians
            public float angleRadians;

            // cache cos() and sin() of angle
            public float angleCos;
            public float angleSin;
        }

        private void UpdateRzInfo()
        {
            lock (this)
            {
                computedOnce = new RzInfo();

                computedOnce.angleRadians = ((float)this.Angle * (float)Math.PI) / 180.0f;
                computedOnce.angleCos = (float)Math.Cos(computedOnce.angleRadians);
                computedOnce.angleSin = (float)Math.Sin(computedOnce.angleRadians);
            
                float sxul = ((1 * computedOnce.angleCos) - (1 * computedOnce.angleSin)) * this.Zoom;
                float syul = ((1 * computedOnce.angleSin) + (1 * computedOnce.angleCos)) * this.Zoom;
                float sxur = ((2 * computedOnce.angleCos) - (1 * computedOnce.angleSin)) * this.Zoom;
                float syur = ((2 * computedOnce.angleSin) + (1 * computedOnce.angleCos)) * this.Zoom;
                float sxll = ((1 * computedOnce.angleCos) - (2 * computedOnce.angleSin)) * this.Zoom;
                float syll = ((1 * computedOnce.angleSin) + (2 * computedOnce.angleCos)) * this.Zoom;

                computedOnce.dsxddx = sxur - sxul;
                computedOnce.dsxddy = sxll - sxul;
                computedOnce.dsyddy = syll - syul;
                computedOnce.dsyddx = syur - syul;
            }
        }

        private RzInfo computedOnce;
        internal RzInfo ComputedOnce
        {
            get
            {
                return computedOnce;
            }
        }

        private float angle;
        public float Angle
        {
            get
            {
                return angle;
            }

            set
            {
                angle = value;
                UpdateRzInfo();
            }
        }

        private float zoom;
        public float Zoom
        {
            get
            {
                return zoom;
            }

            set
            {
                zoom = value;
                UpdateRzInfo();
            }
        }

        private bool sourceAsBackground;
        public bool SourceAsBackground
        {
            get
            {
                return sourceAsBackground;
            }

            set
            {
                sourceAsBackground = value;
                UpdateRzInfo();
            }
        }

		public RotateZoomEffectConfigToken(float angle, float zoom, bool sourceAsBackground)
		{
            this.angle = angle;
            this.zoom = zoom;
            this.sourceAsBackground = sourceAsBackground;
            UpdateRzInfo();
        }

        protected RotateZoomEffectConfigToken(RotateZoomEffectConfigToken copyMe)
        {
            this.angle = copyMe.angle;
            this.zoom = copyMe.zoom;
            this.sourceAsBackground = copyMe.sourceAsBackground;
            UpdateRzInfo();
        }

        public override object Clone()
        {
            return new RotateZoomEffectConfigToken(this);
        }
    }
}
