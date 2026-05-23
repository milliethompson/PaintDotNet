/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

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
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("CropAction.Name");
            }
        }

        public override HistoryAction PerformAction()
        {
            SelectionHistoryAction sha = new SelectionHistoryAction(name, null, Workspace);
            ReplaceDocumentHistoryAction rdha = new ReplaceDocumentHistoryAction(name, null, Workspace);
            Rectangle boundingBox;
            Rectangle[] inverseRegionRects = null;

            if (Workspace.Environment.Selection.IsEmpty)
            {
                throw new InvalidOperationException("There must be an area selected in order to perform the Crop action");
            }
            else
            {
                using (PdnRegion region = Workspace.Environment.Selection.CreateRegion())
                {
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
            newDocument.ReplaceMetaDataFrom(oldDocument);

            foreach (Layer layer in oldDocument.Layers)
            {
                if (layer is BitmapLayer)
                {
                    BitmapLayer oldLayer = (BitmapLayer)layer;
                    Surface croppedSurface = oldLayer.Surface.CreateWindow(boundingBox);
                    BitmapLayer newLayer = new BitmapLayer(croppedSurface);

                    ColorBgra clearWhite = ColorBgra.White.NewAlpha(0);

                    foreach (Rectangle rect in inverseRegionRects)
                    {
                        newLayer.Surface.Clear(clearWhite, rect);
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
            CompoundHistoryAction cha = new CompoundHistoryAction(Name, PdnResources.GetImage("Icons.MenuImageCropIcon.bmp"), new HistoryAction[] { sha, rdha });

            return cha;
        }

        public CropAction(DocumentWorkspace workspace)
            : base(workspace, StaticName)
        {
        }
    }
}
