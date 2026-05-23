using System;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for DesaturateEffect.
	/// </summary>
	public class DesaturateEffect
        : Effect
	{
        public override void Render(RenderArgs dstArgs, RenderArgs srcArgs, System.Drawing.Rectangle roi)
        {
            new UnaryPixelOps.Desaturate().Apply(dstArgs.Surface, roi.Location, srcArgs.Surface, roi.Location, roi.Size);
        }

		public DesaturateEffect()
            : base("Desaturate", "Desatures the image (converts it to black and white).", null)
		{
		}
	}
}
