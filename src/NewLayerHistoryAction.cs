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
    /// Summary description for NewLayerHistoryAction.
    /// </summary>
    public class NewLayerHistoryAction
        : HistoryAction
    {
        private int layerIndex;
        private DocumentWorkspace workspace;

        protected override HistoryAction OnUndo()
        {
            DeleteLayerHistoryAction ha = new DeleteLayerHistoryAction(Name, Image, workspace, 
                (Layer)workspace.Document.Layers[layerIndex]);

            ha.ID = this.ID;
            workspace.Document.Layers.RemoveAt(layerIndex);
            workspace.Document.Invalidate();
            return ha;
        }

        /// <summary>
        /// Creates a NewLayerHistoryAction instnace.
        /// </summary>
        /// <param name="name">The friendly name of this history action.</param>
        /// <param name="image">A 16x16 icon for this history action</param>
        /// <param name="workspace">The DocumentWorkspace.</param>
        /// <param name="layerIndex">The index that you are about to insert a layer at.</param>
        public NewLayerHistoryAction(string name, Image image, DocumentWorkspace workspace, int layerIndex)
            : base(name, image)
        {
            this.workspace = workspace;
            this.layerIndex = layerIndex;
        }
    }
}
