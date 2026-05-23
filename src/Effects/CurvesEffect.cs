/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Curves Adjustment
    /// </summary>
    [EffectTypeHint(EffectTypeHint.Fast)]
    [EffectCategory(EffectCategory.Adjustment)]
    public class CurvesEffect
        : Effect
    {
        public CurvesEffect()
            : base(PdnResources.GetString("CurvesEffect.Name"),
            PdnResources.GetImage("Icons.CurvesEffect.png"),
            Keys.Control | Keys.Shift | Keys.M,
            null,
            EffectDirectives.None, true)
        {
        }

        public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
        {
            CurvesEffectConfigToken token = parameters as CurvesEffectConfigToken;

            if (token != null)
            {
                UnaryPixelOp uop = token.Uop;

                for (int i = startIndex; i < startIndex + length; ++i)
                {
                    uop.Apply(dstArgs.Surface, srcArgs.Surface, rois[i]);
                }
            }
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            return new CurvesEffectConfigDialog();
        }
    }
}
