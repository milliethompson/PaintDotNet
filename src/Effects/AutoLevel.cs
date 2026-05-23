/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using PaintDotNet;

namespace PaintDotNet.Effects
{
	/// <summary>
	/// Summary description for DesaturateEffect.
	/// </summary>
	[EffectCategory(EffectCategory.Adjustment)]
    [EffectTypeHint(EffectTypeHint.Fast)]
	public class AutoLevel
		: Effect
	{
		public Histogram histogram;
		private UnaryPixelOps.Level levels = null;

		public override void Render(RenderArgs dstArgs, RenderArgs srcArgs, System.Drawing.Rectangle roi)
		{
			if (levels == null) 
			{
                histogram.UpdateHistogram(srcArgs.Surface, this.EnvironmentParameters.GetSelection(dstArgs.Bounds));
                levels = histogram.MakeLevelsAuto();
			}

            if (levels.isValid)
            {
                levels.Apply(dstArgs.Surface, roi.Location, srcArgs.Surface, roi.Location, roi.Size);
            }
		}

		public AutoLevel()
			: base("Auto-Level", "Automatically levels the image.", Utility.GetImageResource("Icons.AutoLevel.bmp"), System.Windows.Forms.Shortcut.CtrlShiftL)
		{
			histogram = new Histogram();
		}
	}
}
