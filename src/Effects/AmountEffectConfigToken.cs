/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Used for configuring effects that just need one variable,
    /// an integer that specifies a range that describes "how much"
    /// to apply the effect.
    /// </summary>
    public class AmountEffectConfigToken
        : EffectConfigToken
    {
        private int amount;
        public int Amount
        {
            get
            {
                return amount;
            }

            set
            {
                amount = value;
            }
        }

        public override object Clone()
        {
            return new AmountEffectConfigToken(this);
        }

        public AmountEffectConfigToken(int amount)
            : base()
        {
            this.amount = amount;
        }

        protected AmountEffectConfigToken(AmountEffectConfigToken copyMe)
            : base(copyMe)
        {
            this.amount = copyMe.amount;
        }
    }
}
