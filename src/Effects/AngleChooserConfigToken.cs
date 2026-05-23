// AngleChooserConfigToken.cs 
// effect parameter token passed from AngleChooserConfigDialog to 
// various effects.
//
// Chris Crosetto

using PaintDotNet;
using PaintDotNet.Effects;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet.Effects
{
	/// <summary>
	/// Summary description for AngleChooserConfigToken.
	/// </summary>
	public class AngleChooserConfigToken
        : EffectConfigToken
	{
        private double angle;
        public double Angle
        {
            get
            {
                return angle;
            }

            set
            {
                this.angle = value;
                OnAngleChanged();
            }
        }

        // Override to implement behavior for when the angle changes
        protected virtual void OnAngleChanged()
        {
        }

        public override object Clone()
        {
            return new AngleChooserConfigToken(this);
        }

        public AngleChooserConfigToken(double angle)
            : base()
        {
            this.angle = angle;
        }

        protected AngleChooserConfigToken(AngleChooserConfigToken copyMe)
            : base(copyMe)
        {
            this.angle = copyMe.angle;
        }
    }
}
