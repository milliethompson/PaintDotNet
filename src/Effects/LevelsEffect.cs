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
    /// <summary>
    /// Summary description for LevelsEffect.
    /// </summary>
    [EffectCategory(EffectCategory.Adjustment)]
    [EffectTypeHint(EffectTypeHint.Unary | EffectTypeHint.Fast)]
    public class LevelsEffect 
        : Effect
    {
        public LevelsEffect() :
            base(PdnResources.GetString("LevelsEffect.Name"),
                 PdnResources.GetImage("Icons.LevelsEffect.png"),
                 Keys.Control | Keys.L,
                 true)
        {
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            return new LevelsEffectConfigDialog();
        }

        public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
        {
            UnaryPixelOps.Level levels = (parameters as LevelsEffectConfigToken).Levels;
            levels.Apply(dstArgs.Surface, srcArgs.Surface, rois, startIndex, length);
        }
    }
}
