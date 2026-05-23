using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;

namespace PaintDotNet
{
    /// <summary>
    /// This class can be used to apply an effect using background worker threads
    /// which raise an event when a certain amount of the effect has been processed.
    /// You can use that event to update a status bar, display a preview of the
    /// rendering so far, or whatever.
    /// 
    /// Since two threads are used for rendering, this will improve performance on
    /// dual processor systems, and possibly on systems that have HyperThreading.
    /// 
    /// This class is NOT SAFE for multithreaded access. Note that the events will 
    /// be raised from arbitrary threads.
    /// </summary>
    public class BackgroundEffectRenderer
    {
        private Effect effect;
        private EffectConfigToken effectToken;
        private Region renderRegion;
        private Scanline[] renderScans;
        private int tileCount;
        private Threading.ThreadPool threadPool = new Threading.ThreadPool();
        private ISynchronizeInvoke marshal;
        private RenderArgs dstArgs;
        private RenderArgs srcArgs;

        private static readonly int workers = Utility.PhysicalCpuCount; // how many worker threads to use

        /// <summary>
        /// Used to determine whether the rendering fully completed or not, and was not
        /// aborted in any way. You can use this method to sleep until the rendering
        /// finishes. Once this is set to the signaled state you should check the IsDone
        /// property to make sure that the rendering was actually finished, and not
        /// aborted.
        /// </summary>
        public void Join()
        {
            thread.Join();
        }

        public event RenderedTileEventHandler RenderedTile;
        protected void OnRenderedTile(RenderedTileEventArgs e)
        {
            if (RenderedTile != null)
            {
                RenderedTile(this, e);
            }
        }

        public event EventHandler FinishedRendering;
        protected void OnFinishedRendering()
        {
            if (FinishedRendering != null)
            {
                FinishedRendering(this, EventArgs.Empty);
            }
        }

        private class RendererContext
        {
            private int tileNumber;
            private BackgroundEffectRenderer ber;

            public RendererContext(BackgroundEffectRenderer ber, int tileNumber)
            {
                this.ber = ber;
                this.tileNumber = tileNumber;
            }

            public void Renderer2(object ignored)
            {
                Renderer();
            }

            public void Renderer()
            {
                int begin = (ber.renderScans.Length * tileNumber) / ber.tileCount;
                int end = Math.Min(ber.renderScans.Length, (ber.renderScans.Length * (tileNumber + 1)) / ber.tileCount);

                using (Region subRegion = Utility.ScanlinesToRegion(ber.renderScans, begin, end - begin))
                {
                    if (ber.effect is IConfigurableEffect)
                    {
                        ((IConfigurableEffect)ber.effect).Render(ber.effectToken, ber.dstArgs, ber.srcArgs, subRegion);
                    }
                    else
                    {
                        ber.effect.Render(ber.dstArgs, ber.srcArgs, subRegion);
                    }

                    ber.OnRenderedTile(new RenderedTileEventArgs(subRegion, ber.tileCount, tileNumber));
                }
            }
        }

        public void ThreadFunction ()
        {
            bool finished = true;

            try
            {
                for (int i = 0; i < tileCount; ++i)
                {
                    RendererContext rc = new RendererContext(this, i);
                    threadPool.QueueUserWorkItem(new WaitCallback(rc.Renderer2));

                    if ((i % workers) == (workers - 1))
                    {
                        threadPool.Drain();
                    }

                    if (threadShouldStop)
                    {
                        finished = false;
                        break;
                    }
                }
            }

            catch
            {
                finished = false;
            }

            threadPool.Drain();

            if (finished)
            {
                this.OnFinishedRendering();
            }
        }

        private volatile bool threadShouldStop = false;
        private Thread thread = null;

        private void MarshaledStart()
        {
            Abort();
            threadShouldStop = false;
            thread = new Thread(new ThreadStart(ThreadFunction));
            thread.Start();
        }

        public void Start()
        {
            this.marshal.Invoke(new VoidVoidDelegate(MarshaledStart), null);
        }

        public void MarshaledAbort()
        {
            if (thread != null)
            {
                try
                {
                    threadShouldStop = true;
                    thread.Join();
                }

                catch
                {
                }

                threadPool.Drain();
            }
        }

        public void Abort()
        {
            this.marshal.Invoke(new VoidVoidDelegate(MarshaledAbort), null);
        }

        public BackgroundEffectRenderer(ISynchronizeInvoke marshal,
                                        Effect effect, 
                                        EffectConfigToken effectToken, 
                                        RenderArgs dstArgs, 
                                        RenderArgs srcArgs, 
                                        Region renderRegion, 
                                        int tileCount)
        {
            this.marshal = marshal;
            this.effect = effect;
            this.effectToken = effectToken;
            this.dstArgs = dstArgs;
            this.srcArgs = srcArgs;
            this.renderRegion = renderRegion;
            this.renderRegion.Intersect(dstArgs.Bounds);
            this.renderScans = Utility.GetRegionScans(renderRegion.GetRegionScans(Utility.IdentityMatrix));
            this.tileCount = tileCount;
        }
    }
}
