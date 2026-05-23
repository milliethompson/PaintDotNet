using System;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for InvertColorsEffect.
    /// </summary>
    [EffectCategory(EffectCategory.Adjustment)]
    public class InvertColorsEffect
        : Effect
    {
        public override void Render(RenderArgs dstArgs, RenderArgs srcArgs, System.Drawing.Rectangle roi)
        {
            new UnaryPixelOps.Invert().Apply(dstArgs.Surface, roi.Location, srcArgs.Surface, roi.Location, roi.Size);
        }

        public InvertColorsEffect()
            : base("Invert Colors", "Inverts the colors. Alpha channel is not touched.", null)
        {
        }
    }
}
