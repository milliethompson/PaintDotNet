using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.SystemLayer;
using System;
using System.Drawing;

namespace PdnBench
{
	/// <summary>
	/// Summary description for EffectBenchmark.
	/// </summary>
	public class EffectBenchmark
        : Benchmark
	{
        private Effect effect;
        private EffectConfigToken token;
        private Surface image;

        private Surface dst;
        private RenderArgs srcArgs;
        private RenderArgs dstArgs;
        private PdnRegion region;
        private BackgroundEffectRenderer ber;

        protected override void OnBeforeExecute()
        {
            srcArgs = new RenderArgs(image);
            dst = image.Clone();
            dstArgs = new RenderArgs(dst);
            region = new PdnRegion(dst.Bounds);
            ber = new BackgroundEffectRenderer(effect, token, dstArgs, srcArgs, region, 
                25 * Processor.LogicalCpuCount, Processor.LogicalCpuCount);
        }

        protected sealed override void OnExecute()
        {
            ber.Start();
            ber.Join();
        }

        protected override void OnAfterExecute()
        {
            ber.Dispose();
            region.Dispose();
            dstArgs.Dispose();
            dst.Dispose();
            srcArgs.Dispose();
        }

        public EffectBenchmark(string name, Effect effect, EffectConfigToken token, Surface image)
            : base(name)
		{
            this.effect = effect;
            this.token = token;
            this.image = image;
		}
	}
}
