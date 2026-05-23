using System;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for DesaturateEffect.
    /// </summary>
    [EffectCategory(EffectCategory.Adjustment)]
    public class DesaturateEffect
        : Effect
    {
        public override void Render(RenderArgs dstArgs, RenderArgs srcArgs, System.Drawing.Rectangle roi)
        {
            new UnaryPixelOps.Desaturate().Apply(dstArgs.Surface, roi.Location, srcArgs.Surface, roi.Location, roi.Size);
        }

        public DesaturateEffect()
            : base("Desaturate", "Desatures the image (converts it to black and white).", Utility.GetImageResource("Icons.DesaturateEffect.bmp"), System.Windows.Forms.Shortcut.CtrlShiftG)
        {
        }
    }
}
