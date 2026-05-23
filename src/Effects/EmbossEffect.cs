/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.Effects;
using System;
using System.Drawing;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for EmbossEffect.
    /// </summary>
    [EffectTypeHint(EffectTypeHint.Fast)]
    public class EmbossEffect
        : Effect, 
          IConfigurableEffect
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("EmbossEffect.Name");
            }
        }

        public EmbossEffect()
            : base(StaticName,
                   PdnResources.GetImage("Icons.EmbossEffect.bmp"))
        {
        }

        #region IConfigurableEffect Members

        public EffectConfigDialog CreateConfigDialog()
        {
            return new EmbossEffectConfigDialog();
        }

        public unsafe void Render(EffectConfigToken configToken, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
            EmbossEffectConfigToken eect = (EmbossEffectConfigToken)configToken;

            double[,] weights = eect.Weights;

            Surface dst = dstArgs.Surface;
            Surface src = srcArgs.Surface;

            foreach (Rectangle rect in roi.GetRegionScansReadOnlyInt())
            {
                // loop through each line of target rectangle
                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    int fyStart = 0;
                    int fyEnd = 3;

                    if (y == src.Bounds.Top) fyStart = 1;
                    if (y == src.Bounds.Bottom - 1) fyEnd = 2;

                    // loop through each point in the line 
                    ColorBgra *dstPtr = dst.GetPointAddress(rect.Left, y);
                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        int fxStart = 0;
                        int fxEnd = 3;

                        if (x == src.Bounds.Left) fxStart = 1;
                        if (x == src.Bounds.Right - 1) fxEnd = 2;

                        // loop through each weight
                        double sum = 0.0;

                        for (int fy = fyStart; fy < fyEnd; ++fy)
                        {
                            for (int fx = fxStart; fx < fxEnd; ++fx)
                            {
                                double weight = weights[fy, fx];
                                ColorBgra c = src[x - 1 + fx, y - 1 + fy];
                                double intensity = 255.0 * c.GetIntensity();
                                sum += weight * intensity;
                            }
                        }

                        int iSum = (int)sum;
                        iSum += 128;
                        if (iSum > 255) iSum = 255;
                        if (iSum < 0) iSum = 0;
                        *dstPtr = ColorBgra.FromBgra((byte)iSum, (byte)iSum, (byte)iSum, 255);

                        ++dstPtr;
                    }
                }
            }
        }

        #endregion
    }
}
