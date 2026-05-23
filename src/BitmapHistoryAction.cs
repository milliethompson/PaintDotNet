/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace PaintDotNet
{
    public class BitmapHistoryAction
        : HistoryAction
    {
        private DocumentWorkspace workspace;

        [Serializable]
        private sealed class BitmapHistoryActionData
            : HistoryActionData
        {
            private int layerIndex;
            private IrregularSurface undoImage;

            public int LayerIndex
            {
                get
                {
                    return layerIndex;
                }
            }

            public IrregularSurface UndoImage
            {
                get
                {
                    return undoImage;
                }
            }

            public BitmapHistoryActionData(int layerIndex, IrregularSurface undoImage)
            {
                this.layerIndex = layerIndex;
                this.undoImage = undoImage;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (undoImage != null)
                    {
                        undoImage.Dispose();
                        undoImage = null;
                    }
                }
            }

        }

        public BitmapHistoryAction(string name, Image image, DocumentWorkspace workspace, int layerIndex, PdnRegion changedRegion)
            : base(name, image)
        {
            this.workspace = workspace;
            BitmapLayer layer = (BitmapLayer)workspace.Document.Layers[layerIndex];

            using (PdnRegion region = changedRegion.Clone())
            {
                region.Intersect(layer.Bounds);
                IrregularSurface undoImage = new IrregularSurface(layer.Surface, region);
                BitmapHistoryActionData data = new BitmapHistoryActionData(layerIndex, undoImage);
                this.Data = data;
            }
        }

        public BitmapHistoryAction(string name, Image image, DocumentWorkspace workspace, int layerIndex, IrregularSurface saved)
            : base(name, image)
        {
            this.workspace = workspace;
            BitmapHistoryActionData data = new BitmapHistoryActionData(layerIndex, (IrregularSurface)saved.Clone());
            this.Data = data;
        }

        protected override HistoryAction OnUndo()
        {
            BitmapHistoryActionData data = (BitmapHistoryActionData)this.Data;
            BitmapLayer layer = (BitmapLayer)workspace.Document.Layers[data.LayerIndex];
            
            BitmapHistoryAction redo = new BitmapHistoryAction(Name, Image, workspace, data.LayerIndex, data.UndoImage.Region);

            data.UndoImage.Draw(layer.Surface);

            using (PdnRegion simple = Utility.SimplifyAndInflateRegion(data.UndoImage.Region))
            {
                layer.Invalidate(simple);
            }

            data.UndoImage.Dispose();
            return redo;
        }
    }
}
