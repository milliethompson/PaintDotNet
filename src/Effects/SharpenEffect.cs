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
    /// Summary description for SharpenEffect.
    /// </summary>
    public unsafe class SharpenEffect
        : ConvolutionFilterEffect,
          IConfigurableEffect
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("SharpenEffect.Name");
            }
        }

        private int[][] sharpenWeights = new int[3][] { new int[] { -1, -1, -1 },
                                                        new int[] { -1, 20, -1 },
                                                        new int[] { -1, -1, -1 } };

        public override void Render(RenderArgs dstArgs, RenderArgs srcArgs, Rectangle roi)
        {
            base.RenderConvolutionFilter (sharpenWeights, 0, dstArgs, srcArgs, roi);
        }

        public SharpenEffect()
            : base(StaticName,
                   PdnResources.GetImage("Icons.SharpenEffect.bmp"))
        {
        }

        #region IConfigurableEffect Members

        public EffectConfigDialog CreateConfigDialog()
        {
            AmountEffectConfigDialog aecg = new AmountEffectConfigDialog();
            aecg.Effect = this;
            aecg.Text = StaticName;
            aecg.SliderLabel = PdnResources.GetString("SharpenEffect.ConfigDialog.SliderLabel");
            aecg.SliderUnitsName = string.Empty;
            aecg.SliderMinimum = 1;
            aecg.SliderMaximum = 4;
            aecg.SliderInitialValue = 1;
            aecg.Icon = PdnResources.GetIconFromImage("Icons.SharpenEffect.bmp");
            return aecg;
        }

        // http://www.photo.net/bboard/q-and-a-fetch-msg.tcl?msg_id=000Qi5
        // This guy (Alan Gibson) stated that sharpening is best done as "2*original - blur"

        // So I create a gaussian blur matrix, then negate all the elements except the center one
        // for the center one I compute this as the sum of all the elements in the matrix minus the
        // value at the center. So:
        // blurMatrix = ...;
        // sharpenMatrix = -blurMatrix;
        // sharpenMatrix[center,center] = sum(blurMatrix) - blurMatrix[center,center]

        void IConfigurableEffect.Render(EffectConfigToken properties, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
            AmountEffectConfigToken token = (AmountEffectConfigToken)properties;

            int[][] weights = BlurEffect.CreateGaussianBlurMatrix(1 << (token.Amount - 1));
            int sum = Utility.Sum(weights);
            int center = weights.GetLength(0) / 2;

            for (int i = 0; i < weights.Length; ++i)
            {
                int[] row = weights[i];

                for (int j = 0; j < row.Length; ++j)
                {
                    if (i == center && j == center)
                    {
                        row[j] = (2 * sum) - row[j];
                    }
                    else
                    {
                        row[j] = -row[j];
                    }
                }
            }

            NormalizeWeightMatrix(weights);
            base.RenderConvolutionFilter (weights, 0, dstArgs, srcArgs, roi);
        }

        #endregion
    }
}
