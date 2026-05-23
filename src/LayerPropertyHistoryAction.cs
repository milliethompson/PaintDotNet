/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Collections.Specialized;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for LayerPropertyHistoryAction.
    /// </summary>
    public class LayerPropertyHistoryAction
        : HistoryAction
    {
        private object properties;
        private DocumentWorkspace workspace;
        private int layerIndex;

        protected override HistoryAction OnUndo()
        {
            HistoryAction ha = new LayerPropertyHistoryAction(Name, Image, workspace, layerIndex);
            Layer layer = (Layer)workspace.Document.Layers[layerIndex];
            layer.LoadProperties(properties, true);
            layer.PerformPropertyChanged();
            return ha;
        }

        public LayerPropertyHistoryAction(string name, Image image, DocumentWorkspace workspace, int layerIndex)
            : base(name, image)
        {
            this.workspace = workspace;
            this.layerIndex = layerIndex;
            this.properties = ((Layer)workspace.Document.Layers[layerIndex]).SaveProperties();
        }
    }
}
