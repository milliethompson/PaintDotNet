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
    [EffectCategory(EffectCategory.Adjustment)]
    [EffectTypeHint(EffectTypeHint.Unary | EffectTypeHint.Fast)]
    public class SepiaEffect
        : Effect
    {
        private UnaryPixelOp levels;
        private UnaryPixelOp desaturate;

        public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, 
            Rectangle[] rois, int startIndex, int length)
        {
            desaturate.Apply(dstArgs.Surface, srcArgs.Surface, rois, startIndex, length);
            levels.Apply(dstArgs.Surface, dstArgs.Surface, rois, startIndex, length);
        }

        public SepiaEffect()
            : base(PdnResources.GetString("SepiaEffect.Name"),
                   PdnResources.GetImage("Icons.SepiaEffect.png"), 
                   Keys.Control | Keys.Shift | Keys.P)
        {
            desaturate = new UnaryPixelOps.Desaturate();
            levels = new UnaryPixelOps.Level(
                ColorBgra.Black, 
                ColorBgra.White,
                new float[] { 1.2f, 1.0f, 0.8f },
                ColorBgra.Black,
                ColorBgra.White);
        }
    }
}
