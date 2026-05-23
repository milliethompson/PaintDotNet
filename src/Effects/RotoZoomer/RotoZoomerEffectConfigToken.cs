using PaintDotNet;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for RotoZoomerEffectConfigToken.
    /// </summary>
    public class RotoZoomerEffectConfigToken
        : EffectConfigToken
    {
        internal struct RzInfo
        {
            // gradients
            public double dsxddx;
            public double dsxddy;
            public double dsyddy;
            public double dsyddx;

            // degrees -> radians
            public double angleRadians;

            // cache cos() and sin() of angle
            public double angleCos;
            public double angleSin;
        }

        private void UpdateRzInfo()
        {
            lock (this)
            {
                computedOnce = new RzInfo();

                computedOnce.angleRadians = ((double)this.Angle * (double)Math.PI) / 180.0f;
                computedOnce.angleCos = (double)Math.Cos(computedOnce.angleRadians);
                computedOnce.angleSin = (double)Math.Sin(computedOnce.angleRadians);
            
                double sxul = ((1 * computedOnce.angleCos) - (1 * computedOnce.angleSin)) * this.Zoom;
                double syul = ((1 * computedOnce.angleSin) + (1 * computedOnce.angleCos)) * this.Zoom;
                double sxur = ((2 * computedOnce.angleCos) - (1 * computedOnce.angleSin)) * this.Zoom;
                double syur = ((2 * computedOnce.angleSin) + (1 * computedOnce.angleCos)) * this.Zoom;
                double sxll = ((1 * computedOnce.angleCos) - (2 * computedOnce.angleSin)) * this.Zoom;
                double syll = ((1 * computedOnce.angleSin) + (2 * computedOnce.angleCos)) * this.Zoom;

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

        private int angle;
        public int Angle
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

        private Point anchorOffset;
        public Point AnchorOffset
        {
            get
            {
                return anchorOffset;
            }

            set
            {
                anchorOffset = value;
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

        public RotoZoomerEffectConfigToken(int angle, float zoom, Point anchorOffset, bool sourceAsBackground)
        {
            this.angle = angle;
            this.zoom = zoom;
            this.anchorOffset = anchorOffset;
            this.sourceAsBackground = sourceAsBackground;
            UpdateRzInfo();
        }

        protected RotoZoomerEffectConfigToken(RotoZoomerEffectConfigToken copyMe)
        {
            this.angle = copyMe.angle;
            this.zoom = copyMe.zoom;
            this.anchorOffset = copyMe.anchorOffset;
            this.sourceAsBackground = copyMe.sourceAsBackground;
            UpdateRzInfo();
        }

        public override object Clone()
        {
            return new RotoZoomerEffectConfigToken(this);
        }

    }
}
