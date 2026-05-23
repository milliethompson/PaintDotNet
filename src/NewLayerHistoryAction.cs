using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for NewLayerHistoryAction.
    /// </summary>
    public class NewLayerHistoryAction
        : HistoryAction
    {
        private Layer layer;
        private DocumentWorkspace workspace;

        public Layer Layer
        {
            get
            {
                return layer;
            }
        }

        protected override HistoryAction OnUndo()
        {
            DeleteLayerHistoryAction ha = new DeleteLayerHistoryAction(Name, Image, workspace, layer);
            ha.ID = this.ID;
            workspace.Document.Layers.Remove(layer);
            workspace.Document.Invalidate();
            return ha;
        }

        public NewLayerHistoryAction(string name, Image image, DocumentWorkspace workspace, Layer layer)
            : base(name, image)
        {
            this.workspace = workspace;
            this.layer = layer;
        }
    }
}
