/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// There are two ways to use this action:
    /// 1. Through the normal "PerformAction" interface provided through DoucmentAction
    /// 2. Through the ResizeCanvas static method
    /// </summary>
    public class CanvasSizeAction
        : DocumentAction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("CanvasSizeAction.Name");
            }
        }

        public static Image StaticImage
        {
            get
            {
                return PdnResources.GetImage("Icons.MenuImageCanvasSizeIcon.png");
            }
        }

        public static BitmapLayer ResizeLayer(BitmapLayer layer, Size newSize, AnchorEdge anchor, ColorBgra background)
        {
            BitmapLayer newLayer = new BitmapLayer(newSize.Width, newSize.Height);

            // Background
            new UnaryPixelOps.Constant(background).Apply(newLayer.Surface, newLayer.Surface.Bounds);

            // non-background = clear the alpha channel (see-through)
            if (!layer.IsBackground)
            {
                new UnaryPixelOps.SetAlphaChannel(0).Apply(newLayer.Surface, newLayer.Surface.Bounds);
            }

            int topY = 0;
            int leftX = 0;
            int rightX = newSize.Width - layer.Width;
            int bottomY = newSize.Height - layer.Height;
            int middleX = (newSize.Width - layer.Width) / 2;
            int middleY = (newSize.Height - layer.Height) / 2;

            int x = 0;
            int y = 0;

            #region choose x,y from AnchorEdge
            switch (anchor)
            {
                case AnchorEdge.TopLeft:
                    x = leftX;
                    y = topY;
                    break;

                case AnchorEdge.Top:
                    x = middleX;
                    y = topY;
                    break;

                case AnchorEdge.TopRight:
                    x = rightX;
                    y = topY;
                    break;

                case AnchorEdge.Left:
                    x = leftX;
                    y = middleY;
                    break;

                case AnchorEdge.Middle:
                    x = middleX;
                    y = middleY;
                    break;

                case AnchorEdge.Right:
                    x = rightX;
                    y = middleY;
                    break;

                case AnchorEdge.BottomLeft:
                    x = leftX;
                    y = bottomY;
                    break;

                case AnchorEdge.Bottom:
                    x = middleX;
                    y = bottomY;
                    break;

                case AnchorEdge.BottomRight:
                    x = rightX;
                    y = bottomY;
                    break;
            }
            #endregion

            newLayer.Surface.CopySurface(layer.Surface, new Point(x, y));
            newLayer.LoadProperties(layer.SaveProperties());
            return newLayer;
        }

        public static Document ResizeDocument(Document document, Size newSize, AnchorEdge edge, ColorBgra background)
        {
            Document newDoc = new Document(newSize.Width, newSize.Height);
            newDoc.ReplaceMetaDataFrom(document);

            for (int i = 0; i < document.Layers.Count; ++i)
            {
                Layer layer = (Layer)document.Layers[i];

                if (layer is BitmapLayer)
                {
                    Layer newLayer;

                    try
                    {
                        newLayer = ResizeLayer((BitmapLayer)layer, newSize, edge, background);
                    }

                    catch (OutOfMemoryException)
                    {
                        newDoc.Dispose();
                        throw;
                    }

                    newDoc.Layers.Add(newLayer);
                }
                else
                {
                    throw new InvalidOperationException("Canvas Size does not support Layers that are not BitmapLayers");
                }
            }
                    
            return newDoc;
        }

        // returns null to indicate user cancelled, or if initialNewSize = newSize that the user requested, 
        // or if there was an error (out of memory)
        public static Document ResizeDocument(IWin32Window parent, 
                                              Document document, 
                                              Size initialNewSize, 
                                              AnchorEdge initialAnchor, 
                                              ColorBgra background,
                                              bool loadAndSaveMaintainAspect,
                                              bool saveAnchor)
        {
            using (CanvasSizeDialog csd = new CanvasSizeDialog())
            {
                bool maintainAspect;
                
                if (loadAndSaveMaintainAspect)
                {
                    maintainAspect = Settings.CurrentUser.GetBoolean(PdnSettings.LastMaintainAspectRatioCS, false);
                }
                else
                {
                    maintainAspect = false;
                }

                csd.OriginalSize = document.Size;
                csd.OriginalDpuUnit = document.DpuUnit;
                csd.OriginalDpu = document.DpuX;
                csd.ImageWidth = initialNewSize.Width;
                csd.ImageHeight = initialNewSize.Height;
                csd.LayerCount = document.Layers.Count;
                csd.AnchorEdge = initialAnchor;
                csd.Units = csd.OriginalDpuUnit;
                csd.Resolution = document.DpuX;
                csd.Units = PdnSettings.GetLastNonPixelUnits();
                csd.ConstrainToAspect = maintainAspect;

                DialogResult result = Utility.ShowDialog(csd, parent);
                Size newSize = new Size(csd.ImageWidth, csd.ImageHeight);
                MeasurementUnit newDpuUnit = csd.Units;
                double newDpu = csd.Resolution;

                // If they cancelled, get out
                if (result == DialogResult.Cancel)
                {
                    return null;
                }

                // If they clicked OK, then we save the aspect checkbox, and maybe the anchor
                if (loadAndSaveMaintainAspect)
                {
                    Settings.CurrentUser.SetBoolean(PdnSettings.LastMaintainAspectRatioCS, csd.ConstrainToAspect);
                }

                if (saveAnchor)
                {
                    Settings.CurrentUser.SetString(PdnSettings.LastCanvasSizeAnchorEdge, csd.AnchorEdge.ToString());
                }

                if (newSize == document.Size && newDpuUnit == document.DpuUnit && newDpu == document.DpuX)
                {
                    return null;
                }

                // PERF BUG: Would like to not replace the entire Document instance if they only change
                //           the dpu/dpuUnit, but that would require an interface change.
                //           Will consider for later.

                try
                {
                    Utility.GCFullCollect();
                    Document newDoc = ResizeDocument(document, newSize, csd.AnchorEdge, background);
                    newDoc.DpuUnit = newDpuUnit;
                    newDoc.DpuX = newDpu;
                    newDoc.DpuY = newDpu;
                    return newDoc;
                }

                catch (OutOfMemoryException)
                {
                    Utility.ErrorBox(parent, PdnResources.GetString("CanvasSizeAction.ResizeDocument.OutOfMemory"));
                    return null;
                }

                catch
                {
                    return null;
                }
            }
        }

        public override HistoryAction PerformAction()
        {
            AnchorEdge initialEdge = PdnSettings.GetLastCanvasSizeAnchorEdge();

            Document newDoc = ResizeDocument(Workspace.FindForm(), 
                Workspace.Document, Workspace.Document.Size, initialEdge, Workspace.Environment.BackColor, true, true);

            if (newDoc != null)
            {
                if (newDoc.DpuUnit != MeasurementUnit.Pixel)
                {
                    Settings.CurrentUser.SetString(PdnSettings.LastNonPixelUnits, newDoc.DpuUnit.ToString());

                    if (Workspace.Environment.Units != MeasurementUnit.Pixel)
                    {
                        Workspace.Environment.Units = newDoc.DpuUnit;
                    }
                }

                ReplaceDocumentHistoryAction rdha = new ReplaceDocumentHistoryAction(Name, StaticImage, Workspace);
                Workspace.SetDocument(newDoc);
                return rdha;
            }
            else
            {
                return null;
            }
        }

        public CanvasSizeAction(DocumentWorkspace workspace)
            : base(workspace, StaticName)
        {
        }
    }
}
