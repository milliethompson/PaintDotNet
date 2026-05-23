/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System.Drawing;
using System;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for DesaturateEffect.
    /// </summary>
    [EffectCategory(EffectCategory.Adjustment)]
    [EffectTypeHint(EffectTypeHint.Unary | EffectTypeHint.Fast)]
    public class DesaturateEffect
        : Effect
    {
        public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, 
            Rectangle[] rois, int startIndex, int length)
        {
            new UnaryPixelOps.Desaturate().Apply(dstArgs.Surface, srcArgs.Surface, rois, startIndex, length);
        }

        public DesaturateEffect()
            : base(PdnResources.GetString("DesaturateEffect.Name"),
                   PdnResources.GetImage("Icons.DesaturateEffect.png"), 
                   Keys.Control | Keys.Shift | Keys.G)
        {
        }
    }
}
