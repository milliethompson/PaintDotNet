using PaintDotNet;
using PaintDotNet.Effects;
using System;
using System.Drawing;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for ReliefEffect.
    /// </summary>
    public class ReliefEffect
        : ColorDifferenceEffect, IConfigurableEffect
    {
        public ReliefEffect()
            : base("Relief", "Produce image relief.", Utility.GetImageResource("Icons.ReliefEffect.bmp"))
        {
        }

        #region IConfigurableEffect Members

        void IConfigurableEffect.Render(EffectConfigToken properties, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
            ReliefEffectConfigToken token = (ReliefEffectConfigToken)properties;
            base.RenderColorDifferenceEffect(token.Weights, dstArgs, srcArgs, roi);
        }

        public EffectConfigDialog CreateConfigDialog()
        {
            return new ReliefEffectConfigDialog();
        }

        #endregion
    }
}
