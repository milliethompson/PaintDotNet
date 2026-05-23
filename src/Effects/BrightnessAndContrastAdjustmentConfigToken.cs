using System;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Provided for compatibility with v1.1.
    /// </summary>
    public class BrightnessAndContrastAdjustmentConfigToken
        : TwoAmountsConfigToken
    {
        public int Brightness
        {
            get
            {
                return Amount1;
            }

            set
            {
                this.Amount1 = value;
            }
        }

        public int Contrast
        {
            get
            {
                return Amount2;
            }

            set
            {
                this.Amount2 = value;
            }
        }

        public BrightnessAndContrastAdjustmentConfigToken(int brightness, int contrast)
            : base(brightness, contrast)
        {
        }

        public override object Clone()
        {
            return new BrightnessAndContrastAdjustmentConfigToken(this);
        }

        public BrightnessAndContrastAdjustmentConfigToken(BrightnessAndContrastAdjustmentConfigToken copyMe)
            : base(copyMe)
        {
        }
    }
}
