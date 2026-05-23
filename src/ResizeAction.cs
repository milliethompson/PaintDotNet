/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
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
        private class FitSurfaceContext
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

            public void FitSurface(object context)
            {
                int index = (int)context;
                dstSurface.FitSurface(algorithm, srcSurface, dstRois[index]);
            }

            public FitSurfaceContext(Surface dstSurface, Surface srcSurface, Rectangle[] dstRois, ResamplingAlgorithm algorithm)
            {
                this.dstSurface = dstSurface;
                this.srcSurface = srcSurface;
                this.dstRois = dstRois;
                this.algorithm = algorithm;
            }
        }

        public static BitmapLayer ResizeLayer(BitmapLayer layer, int width, int height, ResamplingAlgorithm algorithm)
        {
            Surface surface = new Surface(width, height);
            surface.Clear(ColorBgra.FromBgra(255, 255, 255, 0));

            PaintDotNet.Threading.ThreadPool threadPool = new PaintDotNet.Threading.ThreadPool();
            int rectCount = Processor.LogicalCpuCount;

            Rectangle[] rects = new Rectangle[rectCount];
            Utility.SplitRectangle(surface.Bounds, rects);

            FitSurfaceContext fsc = new FitSurfaceContext(surface, layer.Surface, rects, algorithm);
            WaitCallback callback = new WaitCallback(fsc.FitSurface);

            for (int i = 0; i < rects.Length; ++i)
            {
                threadPool.QueueUserWorkItem(callback, (object)i);
            }

            threadPool.Drain();

            BitmapLayer newLayer = new BitmapLayer(surface, true);
            newLayer.LoadProperties(layer.SaveProperties());            
            return newLayer;
        }

        public override HistoryAction PerformAction()
        {
            int newWidth = -1;
            int newHeight = -1;

            string resamplingAlgorithm = Settings.GetString(PdnSettings.LastResamplingMethod, ResamplingAlgorithm.SuperSampling.ToString());
            ResamplingAlgorithm alg;
            
            try
            {
                alg = (ResamplingAlgorithm)Enum.Parse(typeof(ResamplingAlgorithm), resamplingAlgorithm, true);
            }

            catch
            {
                alg = ResamplingAlgorithm.SuperSampling;
            }
    
            using (ResizeDialog rd = new ResizeDialog())
            {
                rd.AspectRatio = (double)Workspace.Document.Width / (double)Workspace.Document.Height;
                rd.OriginalSize = Workspace.Document.Size;
                rd.ImageHeight =  Workspace.Document.Height;
                rd.ImageWidth =   Workspace.Document.Width;
                rd.DocumentSize = rd.ImageHeight * rd.ImageWidth * Workspace.Document.Layers.Count * ColorBgra.SizeOf;
                rd.ResamplingAlgorithm = alg;
                rd.Layers = Workspace.Document.Layers.Count;
            
                DialogResult result = Utility.ShowDialog(rd, Workspace.FindForm());

                if (result == DialogResult.Cancel)
                {
                    return null;
                }

                // if the new size equals the old size, there's really no point in doing anything
                if (Workspace.Document.Size == new Size(rd.ImageWidth, rd.ImageHeight))
                {
                    return null;
                }

                newWidth = rd.ImageWidth;
                newHeight = rd.ImageHeight;
                alg = rd.ResamplingAlgorithm;

                Settings.SetString(PdnSettings.LastResamplingMethod, alg.ToString());
            }

            using (new WaitCursorChanger(Workspace))
            {
                ReplaceDocumentHistoryAction rdha;
                Document nd;

                try
                {
                    rdha = new ReplaceDocumentHistoryAction(Name, Utility.GetImageResource("Icons.MenuImageResizeIcon.bmp"), Workspace);
                    nd = new Document(newWidth, newHeight);

					nd.CopyPropertiesFrom(Workspace.Document);

                    foreach (Layer layer in Workspace.Document.Layers)
                    {
                        if (layer is BitmapLayer)
                        {
                            Layer nl = ResizeLayer((BitmapLayer)layer, newWidth, newHeight, alg);

							if (nl == null)
							{
								Utility.ErrorBox(Workspace, "There was an unspecified error while trying to resize the image.");
								return null;
							}
                            
                            nd.Layers.Add(nl);
                        }
                        else
                        {
                            throw new InvalidOperationException("Resize does not support Layers that are not BitmapLayers");
                        }
                    }
                }

                catch (OutOfMemoryException)
                {
                    Utility.GCFullCollect();
                    Utility.ErrorBox(Workspace, "Not enough memory to resize the image.");
                    return null;
                }

                Workspace.SetDocument(nd);
                return rdha;
            }
        }

        public ResizeAction(DocumentWorkspace workspace) 
            : base(workspace, "Resize")
        {
        }
    }
}
