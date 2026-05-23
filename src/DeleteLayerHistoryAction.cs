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
    /// Provides the ability to undo deleting a layer.
    /// </summary>
    public class DeleteLayerHistoryAction
        : HistoryAction
    {
        private int index;
        private DocumentWorkspace workspace;

        [Serializable]
        private sealed class DeleteLayerHistoryActionData
            : HistoryActionData
        {
            private Layer layer;

            public Layer Layer
            {
                get
                {
                    return layer;
                }
            }

            public DeleteLayerHistoryActionData(Layer layer)
            {
                this.layer = layer;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (layer != null)
                    {
                        layer.Dispose();
                        layer = null;
                    }
                }
            }
        }

        protected override HistoryAction OnUndo()
        {
            DeleteLayerHistoryActionData data = (DeleteLayerHistoryActionData)this.Data;
            HistoryAction ha = new NewLayerHistoryAction(Name, Image, workspace, index);
            workspace.Document.Layers.Insert(index, data.Layer);
            ((Layer)workspace.Document.Layers[index]).Invalidate();
            return ha;
        }

        public DeleteLayerHistoryAction(string name, Image image, DocumentWorkspace workspace, Layer deleteMe)
            : base(name, image)
        {
            this.workspace = workspace;
            this.index = workspace.Document.Layers.IndexOf(deleteMe);
            this.Data = new DeleteLayerHistoryActionData(deleteMe);
        }
    }
}
