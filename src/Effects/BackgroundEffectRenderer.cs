/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
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
        private PdnRegion[] tileRegions;
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
                    IConfigurableEffect ice = ber.effect as IConfigurableEffect;

                    if (ice != null)
                    {
                        for (int tile = start; tile < max; tile += inc)
                        {
                            if (ber.threadShouldStop)
                            {
                                break;
                            }

                            PdnRegion subRegion = ber.tileRegions[tile];
                            ice.Render(this.token, ber.dstArgs, ber.srcArgs, subRegion);
                            ber.OnRenderedTile(new RenderedTileEventArgs(subRegion, ber.tileCount, tile));
                        }
                    }
                    else
                    {
                        for (int tile = start; tile < max; tile += inc)
                        {
                            if (ber.threadShouldStop)
                            {
                                break;
                            }

                            PdnRegion subRegion = ber.tileRegions[tile];
                            ber.effect.Render(ber.dstArgs, ber.srcArgs, subRegion);
                            ber.OnRenderedTile(new RenderedTileEventArgs(subRegion, ber.tileCount, tile));
                        }
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
            bool finished = true;

            try
            {
                if (tileCount > 0)
                {
                    IConfigurableEffect ice = this.effect as IConfigurableEffect;
                    PdnRegion subRegion = this.tileRegions[0];

                    if (ice != null)
                    {
                        ice.Render(this.effectTokenCopy, this.dstArgs, this.srcArgs, subRegion);
                    }
                    else
                    {
                        this.effect.Render(this.dstArgs, this.srcArgs, subRegion);
                    }

                    OnRenderedTile(new RenderedTileEventArgs(subRegion, this.tileCount, 0));
                }

                EffectConfigToken[] tokens = new EffectConfigToken[workerThreads];

                for (int i = 0; i < workerThreads; ++i)
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
            }

            catch (Exception ex)
            {
                exceptions.Add(ex);
                finished = false;
            }

            finally
            {
                threadPool.Drain();

                if (finished)
                {
                    this.OnFinishedRendering();
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
                throw new ApplicationException("Worker thread threw an exception", (Exception)exceptions[0]);
            }

            exceptions.Clear();
        }

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

			if (effect.SingleThreaded)
			{
				this.workerThreads = 1;
			}

            this.threadPool = new Threading.ThreadPool(this.workerThreads);
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
