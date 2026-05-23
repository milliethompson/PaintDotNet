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
using PaintDotNet;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for AutoLevel.
    /// </summary>
    [EffectCategory(EffectCategory.Adjustment)]
    [EffectTypeHint(EffectTypeHint.Fast)]
    public class AutoLevel
        : Effect
    {
        private UnaryPixelOps.Level levels = null;

        public override void Render(RenderArgs dstArgs, RenderArgs srcArgs, System.Drawing.Rectangle roi)
        {
            if (levels == null) 
            {
                HistogramRgb histogram = new HistogramRgb();
                histogram.UpdateHistogram(srcArgs.Surface, this.EnvironmentParameters.GetSelection(dstArgs.Bounds));
                levels = histogram.MakeLevelsAuto();
            }

            if (levels.isValid)
            {
                levels.Apply(dstArgs.Surface, roi.Location, srcArgs.Surface, roi.Location, roi.Size);
            }
        }

        public AutoLevel()
            : base(PdnResources.GetString("AutoLevel.Name"),
                   PdnResources.GetImage("Icons.AutoLevel.bmp"), 
                   System.Windows.Forms.Shortcut.CtrlShiftL)
        {
        }
    }
}
