/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.Effects;
using System;
using System.Drawing;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for ReliefEffect.
    /// </summary>
    [EffectTypeHint(EffectTypeHint.Fast)]
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
