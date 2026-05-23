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
    /// Summary description for FlipDocumentAction.
    /// </summary>
    public class FlipDocumentAction
        : DocumentAction
    {
        private Image undoImage;
        private FlipType flipType;

        public override HistoryAction PerformAction()
        {
            int count = Workspace.Document.Layers.Count;
            HistoryAction[] actions = new HistoryAction[count];

            for (int i = 0; i < count; ++i)
            {
                actions[i] = new FlipLayerHistoryAction(this.Name, undoImage, Workspace, i, flipType);
                actions[i] = actions[i].PerformUndo();
            }

            return new CompoundHistoryAction(Name, undoImage, actions);
        }

        public FlipDocumentAction(DocumentWorkspace workspace, string name, Image image, FlipType flipType)
            : base(workspace, name)
        {
            this.undoImage = image;
            this.flipType = flipType;
        }
    }
}
