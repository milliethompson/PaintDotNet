/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace PaintDotNet.Effects
{
    public sealed class PencilSketchEffect
        : Effect
    {
        private static string StaticName
        {
            get
            {
                return PdnResources.GetString("PencilSketchEffect.Name");
            }
        }

        private static ImageResource StaticIcon
        {
            get
            {
                return ImageResource.Get("Icons.PencilSketchEffectIcon.png");
            }
        }

        private BlurEffect blurEffect = new BlurEffect();
        private UnaryPixelOps.Desaturate desaturateOp = new UnaryPixelOps.Desaturate();
        private DesaturateEffect desaturateEffect = new DesaturateEffect();
        private InvertColorsEffect invertEffect = new InvertColorsEffect();
        private BrightnessAndContrastAdjustment bacAdjustment = new BrightnessAndContrastAdjustment();
        private UserBlendOps.ColorDodgeBlendOp colorDodgeOp = new UserBlendOps.ColorDodgeBlendOp();

        public override unsafe void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
        {
            TwoAmountsConfigToken tacd = (TwoAmountsConfigToken)parameters;

            AmountEffectConfigToken blurToken = new AmountEffectConfigToken(tacd.Amount1);
            BrightnessAndContrastAdjustmentConfigToken bacToken = new BrightnessAndContrastAdjustmentConfigToken(tacd.Amount2, -tacd.Amount2);

            this.blurEffect.Render(blurToken, dstArgs, srcArgs, rois, startIndex, length);
            this.bacAdjustment.Render(bacToken, dstArgs, dstArgs, rois, startIndex, length);
            this.invertEffect.Render(null, dstArgs, dstArgs, rois, startIndex, length);
            this.desaturateEffect.Render(null, dstArgs, dstArgs, rois, startIndex, length);

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Rectangle roi = rois[i];

                for (int y = roi.Top; y < roi.Bottom; ++y)
                {
                    ColorBgra* srcPtr = srcArgs.Surface.GetPointAddress(roi.X, roi.Y);
                    ColorBgra* dstPtr = dstArgs.Surface.GetPointAddress(roi.X, roi.Y);

                    for (int x = roi.Left; x < roi.Right; ++x)
                    {
                        ColorBgra srcGrey = this.desaturateOp.Apply(*srcPtr);
                        ColorBgra sketched = this.colorDodgeOp.Apply(srcGrey, *dstPtr);
                        *dstPtr = sketched;

                        ++srcPtr;
                        ++dstPtr;
                    }
                }
            }
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            TwoAmountsConfigDialog tacd = new TwoAmountsConfigDialog();
            tacd.Text = StaticName;

            tacd.Amount1Label = PdnResources.GetString("PencilSketchEffect.ConfigDialog.PencilTipSizeLabel");
            tacd.Amount1Minimum = 1;
            tacd.Amount1Maximum = 20;
            tacd.Amount1Default = 2;

            tacd.Amount2Label = PdnResources.GetString("PencilSketchEffect.ConfigDialog.RangeLabel");
            tacd.Amount2Minimum = -20;
            tacd.Amount2Maximum = 20;
            tacd.Amount2Default = 0;

            return tacd;
        }

        public PencilSketchEffect()
            : base(StaticName, StaticIcon.Reference, true)
        {

        }
    }
}
