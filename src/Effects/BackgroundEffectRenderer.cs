using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;

namespace PaintDotNet.Effects
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
        private PdnRegion renderRegion;
        private PdnRegion[] tileRegions;
        private int tileCount;
        private Threading.ThreadPool threadPool = new Threading.ThreadPool();
        private RenderArgs dstArgs;
        private RenderArgs srcArgs;
        private int workerThreads;

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

        public event EventHandler StartingRendering;
        protected void OnStartingRendering()
        {
            if (StartingRendering != null)
            {
                StartingRendering(this, EventArgs.Empty);
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
                PdnRegion subRegion = ber.tileRegions[tileNumber];

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

        public void ThreadFunction ()
        {
            bool finished = true;

            try
            {
                for (int i = 0; i < tileCount; ++i)
                {
                    RendererContext rc = new RendererContext(this, i);
                    threadPool.QueueUserWorkItem(new WaitCallback(rc.Renderer2));

                    if ((i % workerThreads) == (workerThreads - 1))
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

        public void Start()
        {
            Abort();
            OnStartingRendering();
            threadShouldStop = false;
            thread = new Thread(new ThreadStart(ThreadFunction));
            thread.Start();
        }

        public void Abort()
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

#if false
        private int CountScans(Rectangle[] rects)
        {
            int scans = 0;

            foreach (Rectangle rect in rects)
            {
                scans += rect.Height;
            }

            return scans;
        }

        private void CopyScansToRegion(PdnRegion dst, int scanCount, Rectangle[] srcRects, ref int cursorRectIndex, ref int cursorRectScan)
        {
            Rectangle rect = srcRects[cursorRectIndex];
            int scansLeftInRect = rect.Height - cursorRectScan;

            if (scansLeftInRect < scanCount)
            {
                Rectangle copyMe = Rectangle.FromLTRB(rect.Left, rect.Top + cursorRectScan, rect.Right, rect.Bottom);

                dst.Union(copyMe);
                scanCount -= scansLeftInRect;
                ++cursorRectIndex;
                cursorRectScan = 0;

                CopyScansToRegion(dst, scanCount, srcRects, ref cursorRectIndex, ref cursorRectScan);
            }
            else if (scansLeftInRect >= scanCount)
            {
                int scansNotToCopy = scansLeftInRect - scanCount;
                Rectangle copyMe = Rectangle.FromLTRB(rect.Left, rect.Top + cursorRectScan, rect.Right, rect.Top + cursorRectScan + scanCount);
                dst.Union(copyMe);

                if (scansNotToCopy == 0)
                {
                    ++cursorRectIndex;
                    cursorRectScan = 0;
                }
                else
                {
                    cursorRectScan += scanCount;
                }
            }
        }

        private PdnRegion[] SliceUpRegion(PdnRegion region, int sliceCount)
        {
            PdnRegion[] slices = new PdnRegion[sliceCount];
            Rectangle[] regionScans = region.GetRegionScansReadOnlyInt();
            int regionScansCount = CountScans(regionScans);

            // cursors
            int currentRectIndex = 0;
            int currentRectScan = 0;

            for (int i = 0; i < sliceCount; ++i)
            {
                int beginScan = (regionScansCount * i) / sliceCount;
                int endScan = Math.Min(regionScansCount, (regionScansCount * (i + 1)) / sliceCount);
                int scanCount = endScan - beginScan;

                PdnRegion newRegion = PdnRegion.CreateEmpty();
                CopyScansToRegion(newRegion, scanCount, regionScans, ref currentRectIndex, ref currentRectScan);
                slices[i] = newRegion;
            }

            return slices;
        }
#else
        private PdnRegion[] SliceUpRegion(PdnRegion region, int sliceCount)
        {
            PdnRegion[] slices = new PdnRegion[sliceCount];
            Rectangle[] regionRects = region.GetRegionScansReadOnlyInt();
            Scanline[] regionScans = Utility.GetRegionScans(regionRects);

            for (int i = 0; i < sliceCount; ++i)
            {
                int beginScan = (regionScans.Length * i) / sliceCount;
                int endScan = Math.Min(regionScans.Length, (regionScans.Length * (i + 1)) / sliceCount);

                Rectangle[] newRects = Utility.ScanlinesToRectangles(regionScans, beginScan, endScan - beginScan);
                PdnRegion newRegion = Utility.RectanglesToRegion(newRects);
                slices[i] = newRegion;
            }

            return slices;
        }
#endif  

        public BackgroundEffectRenderer(Effect effect, 
                                        EffectConfigToken effectToken, 
                                        RenderArgs dstArgs, 
                                        RenderArgs srcArgs, 
                                        PdnRegion renderRegion, 
                                        int tileCount,
                                        int workerThreads)
        {
            this.effect = effect;
            this.effectToken = effectToken;
            this.dstArgs = dstArgs;
            this.srcArgs = srcArgs;
            this.renderRegion = renderRegion;
            this.renderRegion.Intersect(dstArgs.Bounds);
            this.tileRegions = SliceUpRegion(renderRegion, tileCount);
            this.tileCount = tileCount;
            this.workerThreads = workerThreads;
        }
    }
}
