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
    /// Summary description for EdgeDetectEffect.
    /// </summary>
    [EffectTypeHint(EffectTypeHint.Fast)]
    public class EdgeDetectEffect
        : ColorDifferenceEffect, 
          IConfigurableEffect
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("EdgeDetectEffect.Name");
            }
        }

        public EdgeDetectEffect()
            : base(StaticName,
                   PdnResources.GetImage("Icons.EdgeDetectEffect.bmp"))
        {
        }

        #region IConfigurableEffect Members

        void IConfigurableEffect.Render(EffectConfigToken properties, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
            EdgeDetectConfigToken token = (EdgeDetectConfigToken)properties;
            base.RenderColorDifferenceEffect(token.Weights, dstArgs, srcArgs, roi);
        }

        public EffectConfigDialog CreateConfigDialog()
        {
            return new EdgeDetectConfigDialog();
        }

        #endregion
    }
}
