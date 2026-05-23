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
            FlipLayerHistoryAction flha = new FlipLayerHistoryAction(this.Name, this.undoImage, Workspace, Workspace.ActiveLayerIndex, flipType);
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
