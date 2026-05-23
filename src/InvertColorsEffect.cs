using System;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for InvertColorsEffect.
	/// </summary>
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
