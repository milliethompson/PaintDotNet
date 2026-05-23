using System;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for BrightnessAndContrastAdjustmentConfigToken.
    /// </summary>
    public class BrightnessAndContrastAdjustmentConfigToken
        : EffectConfigToken
    {
        private int brightness;
        public int Brightness
        {
            get
            {
                return brightness;
            }

            set
            {
                if (value < -100 || value > +100)
                {
                    throw new ArgumentOutOfRangeException("brightness must be within the range [-100,+100]");
                }

                brightness = value;
            }
        }

        private int contrast;
        public int Contrast
        {
            get
            {
                return contrast;
            }

            set
            {
                if (value < -100 || value > +100)
                {
                    throw new ArgumentOutOfRangeException("contrast must be within the range [-100,+100]");
                }

                contrast = value;
            }
        }
        
        public override object Clone()
        {
            return new BrightnessAndContrastAdjustmentConfigToken(this);
        }

        public BrightnessAndContrastAdjustmentConfigToken(int brightness, int contrast)
            : base()
        {
            this.brightness = brightness;
            this.contrast = contrast;
        }

        public BrightnessAndContrastAdjustmentConfigToken(BrightnessAndContrastAdjustmentConfigToken copyMe)
            : base(copyMe)
        {
            this.brightness = copyMe.brightness;
            this.contrast = copyMe.contrast;
        }
    }
}
