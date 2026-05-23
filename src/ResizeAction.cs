/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using PaintDotNet.Threading;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for ResizeAction.
    /// </summary>
    public class ResizeAction 
        : DocumentAction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("ResizeAction.Name");
            }
        }

        public static Image StaticImage
        {
            get
            {
                return PdnResources.GetImage("Icons.MenuImageResizeIcon.bmp");
            }
        }

        private sealed class FitSurfaceContext
        {
            private Surface dstSurface;
            private Surface srcSurface;
            private Rectangle[] dstRois;
            private ResamplingAlgorithm algorithm;

            public Surface DstSurface
            {
                get
                {
                    return dstSurface;
                }
            }

            public Surface SrcSurface
            {
                get
                {
                    return srcSurface;
                }
            }

            public Rectangle[] DstRois
            {
                get
                {
                    return dstRois;
                }
            }

            public ResamplingAlgorithm Algorithm
            {
                get
                {
                    return algorithm;
                }
            }

            public event VoidVoidDelegate RenderedRect;
            private void OnRenderedRect()
            {
                if (RenderedRect != null)
                {
                    RenderedRect();
                }
            }

            public void FitSurface(object context)
            {
                int index = (int)context;
                dstSurface.FitSurface(algorithm, srcSurface, dstRois[index]);
                OnRenderedRect();
            }

            public FitSurfaceContext(Surface dstSurface, Surface srcSurface, Rectangle[] dstRois, ResamplingAlgorithm algorithm)
            {
                this.dstSurface = dstSurface;
                this.srcSurface = srcSurface;
                this.dstRois = dstRois;
                this.algorithm = algorithm;
            }
        }

        private static BitmapLayer ResizeLayer(BitmapLayer layer, int width, int height, ResamplingAlgorithm algorithm,
            int tileCount, VoidVoidDelegate progressCallback, ref bool pleaseStopMonitor)
        {
            Surface surface = new Surface(width, height);
            surface.Clear(ColorBgra.FromBgra(255, 255, 255, 0));

            PaintDotNet.Threading.ThreadPool threadPool = new PaintDotNet.Threading.ThreadPool();
            int rectCount;
            
            if (tileCount == 0)
            {
                rectCount = Processor.LogicalCpuCount;
            }
            else
            {
                rectCount = tileCount;
            }

            Rectangle[] rects = new Rectangle[rectCount];
            Utility.SplitRectangle(surface.Bounds, rects);

            FitSurfaceContext fsc = new FitSurfaceContext(surface, layer.Surface, rects, algorithm);

            if (progressCallback != null)
            {
                fsc.RenderedRect += progressCallback;
            }

            WaitCallback callback = new WaitCallback(fsc.FitSurface);

            for (int i = 0; i < rects.Length; ++i)
            {
                if (pleaseStopMonitor)
                {
                    break;
                }
                else
                {
                    threadPool.QueueUserWorkItem(callback, BoxedConstants.GetInt32(i));
                }
            }

            threadPool.Drain();
            threadPool.DrainExceptions();

            if (pleaseStopMonitor)
            {
                surface.Dispose();
                surface = null;
            }

            BitmapLayer newLayer;

            if (surface == null)
            {
                newLayer = null;
            }
            else
            {
                newLayer = new BitmapLayer(surface, true);
                newLayer.LoadProperties(layer.SaveProperties());
            }

            if (progressCallback != null)
            {
                fsc.RenderedRect -= progressCallback;
            }

            return newLayer;
        }

        public static BitmapLayer ResizeLayer(BitmapLayer layer, int width, int height, ResamplingAlgorithm algorithm)
        {
            bool pleaseStop = false;
            return ResizeLayer(layer, width, height, algorithm, 0, null, ref pleaseStop);
        }

        private class ResizeProgressDialog
            : CallbackWithProgressDialog
        {
            private int maxTiles;
            private int tilesCompleted = 0;
            private int tilesPerLayer;
            private Document dst;
            private Document src;
            private Size newSize;
            private ResamplingAlgorithm algorithm;
            private bool returnVal;
            private bool pleaseStop = false;

            public ResizeProgressDialog(Control owner, Document dst, Document src, Size newSize, ResamplingAlgorithm algorithm)
                : base (owner, PdnInfo.GetProductName(), PdnResources.GetString("ResizeAction.ProgressDialog.Description"))
            {
                this.dst = dst;
                this.src = src;
                this.newSize = newSize;
                this.algorithm = algorithm;
                this.tilesPerLayer = 50 * Processor.LogicalCpuCount;
                this.maxTiles = tilesPerLayer * src.Layers.Count;
                this.Icon = Utility.ImageToIcon(StaticImage);
            }

            protected override void OnCancelClick()
            {
                this.pleaseStop = true;
                base.OnCancelClick ();
            }

            private void RenderedRectHandler()
            {
                this.Owner.BeginInvoke(new VoidVoidDelegate(MarshaledProgressUpdate));
            }

            private void MarshaledProgressUpdate()
            {
                ++tilesCompleted;
                double progress = 100.0 * ((double)tilesCompleted / (double)maxTiles);
                this.Progress = (int)Math.Round(progress);
            }

            public bool DoResize()
            {
                DialogResult result = this.ShowDialog(true, false, new ThreadStart(ResizeDocument));

                if (!this.returnVal && !this.Cancelled)
                {
                    Utility.ErrorBox(this.Owner, PdnResources.GetString("ResizeAction.PerformAction.UnspecifiedError"));
                }

                return this.returnVal;
            }

            private void ResizeDocument()
            {
                this.pleaseStop = false;

                // This is only sort of a hack: we must try and allocate enough for 2 extra layer-sized buffers
                // Then we free them immediately. This is just so that if we don't have enough memory that we'll
                // fail sooner rather than later.
                Surface s1 = new Surface(this.newSize);
                Surface s2 = new Surface(this.newSize);

                try
                {
                    foreach (Layer layer in src.Layers)
                    {
                        if (this.pleaseStop)
                        {
                            this.returnVal = false;
                            return;
                        }

                        if (layer is BitmapLayer)
                        {
                            Layer newLayer = ResizeLayer((BitmapLayer)layer, this.newSize.Width, this.newSize.Height, this.algorithm, 
                                this.tilesPerLayer, new VoidVoidDelegate(RenderedRectHandler), ref this.pleaseStop);

                            if (newLayer == null)
                            {
                                this.returnVal = false;
                                return;
                            }

                            dst.Layers.Add(newLayer);
                        }
                        else
                        {
                            throw new InvalidOperationException("Resize does not support Layers that are not BitmapLayers");
                        }
                    }
                }

                finally
                {
                    s1.Dispose();
                    s2.Dispose();
                }

                this.returnVal = true;
            }
        }

        public override HistoryAction PerformAction()
        {
            int newWidth;
            int newHeight;
            double newDpu;
            MeasurementUnit newDpuUnit;

            string resamplingAlgorithm = Settings.CurrentUser.GetString(PdnSettings.LastResamplingMethod, 
                ResamplingAlgorithm.SuperSampling.ToString());

            ResamplingAlgorithm alg;
            
            try
            {
                alg = (ResamplingAlgorithm)Enum.Parse(typeof(ResamplingAlgorithm), resamplingAlgorithm, true);
            }

            catch
            {
                alg = ResamplingAlgorithm.SuperSampling;
            }

            bool maintainAspect = Settings.CurrentUser.GetBoolean(PdnSettings.LastMaintainAspectRatio, true);

            using (ResizeDialog rd = new ResizeDialog())
            {
                rd.OriginalSize = Workspace.Document.Size;
                rd.OriginalDpuUnit = Workspace.Document.DpuUnit;
                rd.OriginalDpu = Workspace.Document.DpuX;
                rd.ImageHeight = Workspace.Document.Height;
                rd.ImageWidth = Workspace.Document.Width;
                rd.ResamplingAlgorithm = alg;
                rd.LayerCount = Workspace.Document.Layers.Count;
                rd.Units = rd.OriginalDpuUnit;
                rd.Resolution = Workspace.Document.DpuX;
                rd.Units = PdnSettings.GetLastNonPixelUnits();
                rd.ConstrainToAspect = maintainAspect;
            
                DialogResult result = Utility.ShowDialog(rd, Workspace.FindForm());

                if (result == DialogResult.Cancel)
                {
                    return null;
                }

                Settings.CurrentUser.SetString(PdnSettings.LastResamplingMethod, rd.ResamplingAlgorithm.ToString());
                Settings.CurrentUser.SetBoolean(PdnSettings.LastMaintainAspectRatio, rd.ConstrainToAspect);
                newDpuUnit = rd.Units;
                newWidth = rd.ImageWidth;
                newHeight = rd.ImageHeight;
                newDpu = rd.Resolution;
                alg = rd.ResamplingAlgorithm;

                if (newDpuUnit != MeasurementUnit.Pixel)
                {
                    Settings.CurrentUser.SetString(PdnSettings.LastNonPixelUnits, newDpuUnit.ToString());

                    if (Workspace.Environment.Units != MeasurementUnit.Pixel)
                    {
                        Workspace.Environment.Units = newDpuUnit;
                    }
                }

                // if the new size equals the old size, there's really no point in doing anything
                if (Workspace.Document.Size == new Size(rd.ImageWidth, rd.ImageHeight) &&
                    Workspace.Document.DpuX == newDpu &&
                    Workspace.Document.DpuUnit == newDpuUnit)
                {
                    return null;
                }
            }

            HistoryAction ha;

            if (newWidth == Workspace.Document.Width &&
                newHeight == Workspace.Document.Height)
            {
                // Only adjusting Dpu or DpuUnit
                ha = new DocumentMetaDataHistoryAction(Name, StaticImage, Workspace);
                Workspace.Document.DpuUnit = newDpuUnit;
                Workspace.Document.DpuX = newDpu;
                Workspace.Document.DpuY = newDpu;
            }
            else
            {
                try
                {
                    using (new WaitCursorChanger(Workspace))
                    {
                        ha = new ReplaceDocumentHistoryAction(Name, StaticImage, Workspace);
                    }

                    Document newDocument = new Document(newWidth, newHeight);
                    newDocument.ReplaceMetaDataFrom(Workspace.Document);
                    newDocument.DpuUnit = newDpuUnit;
                    newDocument.DpuX = newDpu;
                    newDocument.DpuY = newDpu;
                    ResizeProgressDialog rpd = new ResizeProgressDialog(this.Workspace, newDocument, Workspace.Document, new Size(newWidth, newHeight), alg);
                    Utility.GCFullCollect();
                    bool result = rpd.DoResize();

                    if (!result)
                    {
                        return null;
                    }

                    Workspace.SetDocument(newDocument);
                }

                catch (WorkerThreadException ex)
                {
                    if (ex.InnerException is OutOfMemoryException)
                    {
                        Utility.ErrorBox(Workspace, PdnResources.GetString("ResizeAction.PerformAction.OutOfMemory"));
                        return null;
                    }
                    else
                    {
                        throw;
                    }
                }

                catch (OutOfMemoryException)
                {
                    Utility.ErrorBox(Workspace, PdnResources.GetString("ResizeAction.PerformAction.OutOfMemory"));
                    return null;
                }
            }

            return ha;
        }

        public ResizeAction(DocumentWorkspace workspace) 
            : base(workspace, StaticName)
        {
        }
    }
}
