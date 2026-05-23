/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
	/// <summary>
	/// Summary description for HueAndSaturationAdjustment.
	/// </summary>
	[EffectCategory(EffectCategory.Adjustment)]
    [EffectTypeHint(EffectTypeHint.Unary | EffectTypeHint.Fast)]
	public class HueAndSaturationAdjustment
        : Effect,
          IConfigurableEffect  
	{
        public const string StaticName = "Hue / Saturation";

		public HueAndSaturationAdjustment()
            : base(HueAndSaturationAdjustment.StaticName,
                   "Allows you to adjust the hue, saturation, and luminance",
                   Utility.GetImageResource("Icons.HueAndSaturationAdjustment.bmp"), 
                   System.Windows.Forms.Shortcut.CtrlShiftU)
		{
		}

        public EffectConfigDialog CreateConfigDialog()
        {
            ThreeAmountsConfigDialog tacg = new ThreeAmountsConfigDialog();

            tacg.Text = HueAndSaturationAdjustment.StaticName;

            tacg.Amount1Default = 0;
            tacg.Amount1Label = "Hue";
            tacg.Amount1Maximum = 180;
            tacg.Amount1Minimum = -180;

            tacg.Amount2Default = 100;
            tacg.Amount2Label = "Saturation";
            tacg.Amount2Maximum = 400;
            tacg.Amount2Minimum = 0;

            tacg.Amount3Default = 0;
            tacg.Amount3Label = "Lightness";
            tacg.Amount3Maximum = 100;
			tacg.Amount3Minimum = -100;

			tacg.Icon = Utility.GetIconResource("Icons.HueAndSaturationAdjustment.bmp");

            return tacg;
        }

        public unsafe void Render(EffectConfigToken properties, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
            ThreeAmountsConfigToken token = (ThreeAmountsConfigToken)properties;
            int hueDelta = token.Amount1;
            int satDelta = token.Amount2;
            int lightness = token.Amount3;

            UnaryPixelOp op;

            Surface dst = dstArgs.Surface;
            Surface src = srcArgs.Surface;

            if (hueDelta == 0 && satDelta == 100 && lightness == 0)
            {
                op = new UnaryPixelOps.Identity();
            }
            else
            {
                op = new UnaryPixelOps.HueSaturationLightness(hueDelta, satDelta, lightness);
            }
            

            foreach (Rectangle rect in roi.GetRegionScansReadOnlyInt())
            {
                op.Apply(dst, src, rect);
            }
        }
    }
}
