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
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    [EffectCategory(EffectCategory.Adjustment)]
    [EffectTypeHint(EffectTypeHint.Unary | EffectTypeHint.Fast)]
    public class InvertColorsEffect
        : Effect
    {
        public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, 
            Rectangle[] rois, int startIndex, int length)
        {
            new UnaryPixelOps.Invert().Apply(dstArgs.Surface, srcArgs.Surface, rois, startIndex, length);
        }

        public InvertColorsEffect()
            : base(PdnResources.GetString("InvertColorsEffect.Name"),
                   PdnResources.GetImage("Icons.InvertColorsEffect.png"), 
                   Keys.Control | Keys.Shift | Keys.I)
        {
        }
    }
}
