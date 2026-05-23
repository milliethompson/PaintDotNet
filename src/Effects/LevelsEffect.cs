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

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for LevelsEffect.
    /// </summary>
    [EffectCategory(EffectCategory.Adjustment)]
    [EffectTypeHint(EffectTypeHint.Unary | EffectTypeHint.Fast)]
    public class LevelsEffect 
        : Effect, 
          IConfigurableEffect
    {
        public LevelsEffect() :
            base(PdnResources.GetString("LevelsEffect.Name"),
                 PdnResources.GetImage("Icons.LevelsEffect.bmp"),
                 System.Windows.Forms.Shortcut.CtrlL)
        {
        }
        #region IConfigurableEffect Members

        public EffectConfigDialog CreateConfigDialog()
        {
            return new LevelsEffectConfigDialog();
        }

        public void Render(EffectConfigToken properties, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
            UnaryPixelOps.Level levels = (properties as LevelsEffectConfigToken).Levels;

            foreach (Rectangle r in roi.GetRegionScansReadOnlyInt())
            {
                levels.Apply(dstArgs.Surface, srcArgs.Surface, r);
            }
        }

        #endregion
    }
}
