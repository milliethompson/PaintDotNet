/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
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
        : ColorDifferenceEffect
    {
        public ReliefEffect()
            : base(PdnResources.GetString("ReliefEffect.Name"),
                   PdnResources.GetImage("Icons.ReliefEffect.png"),
                   true)
        {
        }

        public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
        {
            ReliefEffectConfigToken token = (ReliefEffectConfigToken)parameters;
            base.RenderColorDifferenceEffect(token.Weights, dstArgs, srcArgs, rois, startIndex, length);
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            return new ReliefEffectConfigDialog();
        }
    }
}
