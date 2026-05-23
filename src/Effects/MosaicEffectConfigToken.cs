using System;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Provided for compatibility with v1.1
    /// </summary>
    public class MosaicEffectConfigToken
        : AmountEffectConfigToken
    {
        public int CellSize
        {
            get
            {
                return this.Amount;
            }
            set
            {
                this.Amount = value;
            }
        }

        public MosaicEffectConfigToken(int newCellSize)
            : base(newCellSize)
        {
        }

        protected MosaicEffectConfigToken(MosaicEffectConfigToken copyThis)
            : base(copyThis)
        {
        }
    }
}
