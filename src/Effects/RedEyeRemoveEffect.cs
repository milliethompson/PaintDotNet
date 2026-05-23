/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.Effects
{
	/// <summary>
	/// Summary description for DesaturateEffect.
	/// </summary>
    [EffectTypeHint(EffectTypeHint.Unary | EffectTypeHint.Fast)]
    public class RedEyeRemoveEffect
		: Effect,
		IConfigurableEffect
	{
		public EffectConfigDialog CreateConfigDialog()
		{   
			RedEyeRemoveEffectDialog tacd = new RedEyeRemoveEffectDialog();

			tacd.Text = "Red Eye Removal";

			tacd.Amount1Minimum = 0;
			tacd.Amount1Maximum = 100;
			tacd.Amount1Default = 70;
			tacd.Amount1Label = "Tolerence";
            
			tacd.Amount2Minimum = 0;
			tacd.Amount2Maximum = 100;
			tacd.Amount2Default = 90;
			tacd.Amount2Label = "Percent Saturation";

			tacd.Icon = Utility.GetIconResource("Icons.RedEyeRemoveEffect.bmp");

			return tacd;
		}

		void IConfigurableEffect.Render(EffectConfigToken configToken, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
		{
			TwoAmountsConfigToken tact = (TwoAmountsConfigToken)configToken;
		
			PixelOp redEyeRemove = new UnaryPixelOps.RedEyeRemove(tact.Amount1,tact.Amount2);
			
			System.Drawing.Rectangle[] rects = roi.GetRegionScansInt();		
			foreach (System.Drawing.Rectangle rect in rects)
			{
				redEyeRemove.Apply(dstArgs.Surface, rect.Location, srcArgs.Surface, rect.Location, rect.Size);
			}
		}

		public RedEyeRemoveEffect()
			: base("Red Eye Removal", 
                   "Removes all instances of red eye detected", 
                   Utility.GetImageResource("Icons.RedEyeRemoveEffect.bmp"))
		{
		}
	}
}