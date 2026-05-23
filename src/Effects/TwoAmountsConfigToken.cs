using System;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for BrightnessAndContrastAdjustmentConfigToken.
    /// </summary>
    public class TwoAmountsConfigToken
        : EffectConfigToken
    {
        private int amount1;
        public int Amount1
        {
            get
            {
                return amount1;
            }

            set
            {
                amount1 = value;
            }
        }

        private int amount2;
        public int Amount2
        {
            get
            {
                return amount2;
            }

            set
            {
                amount2 = value;
            }
        }
        
        public override object Clone()
        {
            return new TwoAmountsConfigToken(this);
        }

        public TwoAmountsConfigToken(int amount1, int amount2)
            : base()
        {
            this.amount1 = amount1;
            this.amount2 = amount2;
        }

        public TwoAmountsConfigToken(TwoAmountsConfigToken copyMe)
            : base(copyMe)
        {
            this.amount1 = copyMe.amount1;
            this.amount2 = copyMe.amount2;
        }
    }
}
