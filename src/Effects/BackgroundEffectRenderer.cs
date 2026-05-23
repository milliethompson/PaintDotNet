/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
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
    public sealed class BackgroundEffectRenderer
        : IDisposable
    {
        private Effect effect;
        private EffectConfigToken effectToken; // this references the main token that is passed in to the constructor
        private EffectConfigToken effectTokenCopy; // this copy of the token is updated every time you call Start() to make sure it is up to date. This is then passed to the threads, not the original one.
        private PdnRegion renderRegion;
        private Rectangle[][] tileRegions;
        private PdnRegion[] tilePdnRegions;
        private int tileCount;
        private Threading.ThreadPool threadPool;
        private RenderArgs dstArgs;
        private RenderArgs srcArgs;
        private int workerThreads;
        private ArrayList exceptions = ArrayList.Synchronized(new ArrayList());

        public event RenderedTileEventHandler RenderedTile;
        private void OnRenderedTile(RenderedTileEventArgs e)
        {
            if (RenderedTile != null)
            {
                RenderedTile(this, e);
            }
        }

        public event EventHandler FinishedRendering;
        private void OnFinishedRendering()
        {
            if (FinishedRendering != null)
            {
                FinishedRendering(this, EventArgs.Empty);
            }
        }

        public event EventHandler StartingRendering;
        private void OnStartingRendering()
        {
            if (StartingRendering != null)
            {
                StartingRendering(this, EventArgs.Empty);
            }
        }

        private sealed class RendererContext
        {
            private BackgroundEffectRenderer ber;
            private EffectConfigToken token;
            private int threadNumber;
            private int startOffset;

            public RendererContext(BackgroundEffectRenderer ber, EffectConfigToken token, int threadNumber)
                : this(ber, token, threadNumber, 0)
            {
            }

            public RendererContext(BackgroundEffectRenderer ber, EffectConfigToken token, int threadNumber, int startOffset)
            {
                this.ber = ber;
                this.token = token;
                this.threadNumber = threadNumber;
                this.startOffset = startOffset;
            }

            public void Renderer2(object ignored)
            {
                Renderer();
            }

            public void Renderer()
            {
                ThreadPriority oldTP = Thread.CurrentThread.Priority;
                ThreadPriority newTP = ThreadPriority.BelowNormal;
                Thread.CurrentThread.Priority = newTP;

                int inc = ber.workerThreads;
                int start = this.threadNumber + (this.startOffset * inc);
                int max = ber.tileCount;

                try
                {
                    for (int tile = start; tile < max; tile += inc)
                    {
                        if (ber.threadShouldStop)
                        {
                            break;
                        }

                        Rectangle[] subRegion = ber.tileRegions[tile];
                        ber.effect.Render(this.token, ber.dstArgs, ber.srcArgs, subRegion);
                        PdnRegion subPdnRegion = ber.tilePdnRegions[tile];
                        ber.OnRenderedTile(new RenderedTileEventArgs(subPdnRegion, ber.tileCount, tile));
                    }
                }

                catch (Exception ex)
                {
                    ber.exceptions.Add(ex);
                }

                finally
                {
                    Thread.CurrentThread.Priority = oldTP;
                }
            }
        }

        public void ThreadFunction()
        {
            if (this.srcArgs.Surface.Scan0.MaySetAllowWrites)
            {
                this.srcArgs.Surface.Scan0.AllowWrites = false;
            }

            try
            {
                if (tileCount > 0)
                {
                    ThreadPriority oldTP = Thread.CurrentThread.Priority;
                    Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

                    try
                    {
                        Rectangle[] subRegion = this.tileRegions[0];
                        this.effect.Render(this.effectTokenCopy, this.dstArgs, this.srcArgs, subRegion);
                        PdnRegion subPdnRegion = this.tilePdnRegions[0];
                        OnRenderedTile(new RenderedTileEventArgs(subPdnRegion, this.tileCount, 0));
                    }

                    finally
                    {
                        Thread.CurrentThread.Priority = oldTP;
                    }
                }

                EffectConfigToken[] tokens = new EffectConfigToken[workerThreads];

                int i;
                for (i = 0; i < workerThreads; ++i)
                {
                    if (this.threadShouldStop)
                    {
                        break;
                    }

                    if (this.effectTokenCopy == null)
                    {
                        tokens[i] = null;
                    }
                    else
                    {
                        tokens[i] = (EffectConfigToken)this.effectTokenCopy.Clone();
                    }

                    RendererContext rc = new RendererContext(this, tokens[i], i, (i == 0) ? 1 : 0);
                    threadPool.QueueUserWorkItem(new WaitCallback(rc.Renderer2));
                }

                if (i == workerThreads)
                {
                    threadPool.Drain();
                    OnFinishedRendering();
                }
            }

            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            finally
            {
                threadPool.Drain();

                Exception[] exceptions = threadPool.Exceptions;

                if (exceptions.Length > 0)
                {
                    foreach (Exception exception in exceptions)
                    {
                        this.exceptions.Add(exception);
                    }
                }

                if (this.srcArgs.Surface.Scan0.MaySetAllowWrites)
                {
                    this.srcArgs.Surface.Scan0.AllowWrites = true;
                }
            }
        }

        private volatile bool threadShouldStop = false;
        private Thread thread = null;

        public void Start()
        {
            Abort();

            if (this.effectToken != null)
            {
                this.effectTokenCopy = (EffectConfigToken)this.effectToken.Clone();
            }

            threadShouldStop = false;
            OnStartingRendering();
            thread = new Thread(new ThreadStart(ThreadFunction));
            thread.Start();
        }

        public void Abort()
        {
            if (thread != null)
            {
                threadShouldStop = true;
                Join();
                threadPool.Drain();
            }
        }

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

            if (exceptions.Count > 0)
            {
                throw new WorkerThreadException("Worker thread threw an exception", (Exception)exceptions[0]);
            }

            exceptions.Clear();
        }

        private Rectangle[][] SliceUpRegion(PdnRegion region, int sliceCount, Rectangle layerBounds)
        {
            Rectangle[][] slices = new Rectangle[sliceCount][];
            Rectangle[] regionRects = region.GetRegionScansReadOnlyInt();
            Scanline[] regionScans = Utility.GetRegionScans(regionRects);

            for (int i = 0; i < sliceCount; ++i)
            {
                int beginScan = (regionScans.Length * i) / sliceCount;
                int endScan = Math.Min(regionScans.Length, (regionScans.Length * (i + 1)) / sliceCount);

                // Try to arrange it such that the maximum size of the first region
                // is 1-pixel tall
                if (i == 0)
                {
                    endScan = Math.Min(endScan, beginScan + 1);
                }
                else if (i == 1)
                {
                    beginScan = Math.Min(beginScan, 1);
                }

                Rectangle[] newRects = Utility.ScanlinesToRectangles(regionScans, beginScan, endScan - beginScan);

                for (int j = 0; j < newRects.Length; ++j)
                {
                    newRects[j].Intersect(layerBounds);
                }

                slices[i] = newRects;
            }

            return slices;
        }

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
            this.tileRegions = SliceUpRegion(renderRegion, tileCount, dstArgs.Bounds);

            this.tilePdnRegions = new PdnRegion[this.tileRegions.Length];
            for (int i = 0; i < this.tileRegions.Length; ++i)
            {
                PdnRegion pdnRegion = Utility.RectanglesToRegion(this.tileRegions[i]);
                this.tilePdnRegions[i] = pdnRegion;
            }

            this.tileCount = tileCount;
            this.workerThreads = workerThreads;

            if ((effect.EffectDirectives & EffectDirectives.SingleThreaded) != 0)
            {
                this.workerThreads = 1;
            }

            this.threadPool = new Threading.ThreadPool(this.workerThreads, false);
        }

        ~BackgroundEffectRenderer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.srcArgs != null)
                {
                    this.srcArgs.Dispose();
                    this.srcArgs = null;
                }

                if (this.dstArgs != null)
                {
                    this.dstArgs.Dispose();
                    this.dstArgs = null;
                }
            }
        }
    }
}
