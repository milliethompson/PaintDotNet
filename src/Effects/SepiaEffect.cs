/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for SepiaEffect.
    /// </summary>
    [EffectCategory(EffectCategory.Adjustment)]
    [EffectTypeHint(EffectTypeHint.Unary | EffectTypeHint.Fast)]
    public class SepiaEffect
        : Effect
    {
        private UnaryPixelOp levels;
        private UnaryPixelOp desaturate;
        
        public override void Render(RenderArgs dstArgs, RenderArgs srcArgs, System.Drawing.Rectangle roi)
        {
            desaturate.Apply(dstArgs.Surface, roi.Location, srcArgs.Surface, roi.Location, roi.Size);
            levels.Apply(dstArgs.Surface, roi.Location, dstArgs.Surface, roi.Location, roi.Size);
        }

        public SepiaEffect()
            : base(PdnResources.GetString("SepiaEffect.Name"),
                   PdnResources.GetImage("Icons.SepiaEffect.bmp"), 
                   System.Windows.Forms.Shortcut.CtrlShiftP)
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
