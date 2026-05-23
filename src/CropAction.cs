using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Crops the image to the currently selected region.
    /// </summary>
    public class CropAction
        : DocumentAction
    {
        public override HistoryAction PerformAction()
        {
            SelectionHistoryAction sha = new SelectionHistoryAction(name, null, Workspace);
            ReplaceDocumentHistoryAction rdha = new ReplaceDocumentHistoryAction(name, null, Workspace);
            Rectangle boundingBox;
            //RectangleF[] inverseRegionRectsF = null;
            Rectangle[] inverseRegionRects = null;

            if (Workspace.Environment.IsSelectionEmpty)
            {
                throw new InvalidOperationException("There must be an area selected in order to perform the Crop action");
            }
            else
            {
                using (PdnRegion region = Workspace.Environment.CreateSelectedRegion())
                {
                    region.Intersect(Workspace.Document.Bounds);
                    boundingBox = Utility.GetRegionBounds(region);

                    using (PdnRegion inverseRegion = new PdnRegion(boundingBox))
                    {
                        inverseRegion.Exclude(region);
                        inverseRegionRects = Utility.TranslateRectangles(inverseRegion.GetRegionScansReadOnlyInt(), -boundingBox.X, -boundingBox.Y);
                    }
                }
            }

            Document oldDocument = Workspace.Document;
            Document newDocument = new Document(boundingBox.Width, boundingBox.Height);
            
            // copy the document's meta data over
           newDocument.CopyProperties(oldDocument);

            foreach (Layer layer in oldDocument.Layers)
            {
                if (layer is BitmapLayer)
                {
                    BitmapLayer oldLayer = (BitmapLayer)layer;
                    Surface croppedSurface = oldLayer.Surface.CreateWindow(boundingBox);
                    BitmapLayer newLayer = new BitmapLayer(croppedSurface);

                    UnaryPixelOp op = new UnaryPixelOps.Constant(ColorBgra.FromBgra(255, 255, 255, 0));

                    foreach (Rectangle rect in inverseRegionRects)
                    {
                        op.Apply(newLayer.Surface, rect);
                    }

                    newLayer.LoadProperties(oldLayer.SaveProperties());
                    newDocument.Layers.Add(newLayer);
                }
                else
                {
                    throw new InvalidOperationException("Crop does not support Layers that are not BitmapLayers");
                }
            }
            
            Workspace.SetDocument(newDocument);
            CompoundHistoryAction cha = new CompoundHistoryAction(Name, Utility.GetImageResource("Icons.MenuImageCropIcon.bmp"), new HistoryAction[] { sha, rdha });

            return cha;
        }

        public CropAction(DocumentWorkspace workspace)
            : base(workspace, "Crop to Selection")
        {
        }
    }
}
