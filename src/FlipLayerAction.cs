using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for FlipLayerAction.
    /// </summary>
    public abstract class FlipLayerAction
        : DocumentAction
    {
        private FlipType flipType;
        private Image undoImage;

        public override HistoryAction PerformAction()
        {
            FlipLayerHistoryAction flha = new FlipLayerHistoryAction(this.Name, this.undoImage, Workspace.ActiveLayer, flipType);
            return flha.PerformUndo();
        }

        public FlipLayerAction(DocumentWorkspace workspace, string name, Image image, FlipType flipType)
            : base(workspace, name)
        {
            this.flipType = flipType;
            this.undoImage = image;
        }
    }
}
