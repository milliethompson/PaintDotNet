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
	/// Summary description for SwapLayerHistoryAction.
	/// </summary>
	public class SwapLayerHistoryAction
        : HistoryAction
	{
        private int layerIndex1;
        private int layerIndex2;
        private DocumentWorkspace workspace;

        protected override HistoryAction OnUndo()
        {
            SwapLayerHistoryAction slha = new SwapLayerHistoryAction(this.Name, this.Image, this.workspace, this.layerIndex2, this.layerIndex1);

            Layer layer1 = (Layer)workspace.Document.Layers[this.layerIndex1];
            Layer layer2 = (Layer)workspace.Document.Layers[this.layerIndex2];

            int firstIndex = Math.Min(layerIndex1, layerIndex2);
            int secondIndex = Math.Max(layerIndex1, layerIndex2);

            if (secondIndex - firstIndex == 1)
            {
                workspace.Document.Layers.RemoveAt(layerIndex1);
                workspace.Document.Layers.Insert(layerIndex2, layer1);
            }
            else
            {
                // general version
                workspace.Document.Layers[layerIndex1] = layer2;
                workspace.Document.Layers[layerIndex2] = layer1;
            }

            ((Layer)workspace.Document.Layers[this.layerIndex1]).Invalidate();
            ((Layer)workspace.Document.Layers[this.layerIndex2]).Invalidate();

            return slha;
        }

		public SwapLayerHistoryAction(string name, Image image, DocumentWorkspace workspace, int layerIndex1, int layerIndex2)
            : base(name, image)
		{
            if (this.layerIndex1 < 0 || this.layerIndex2 < 0 ||
                this.layerIndex1 >= workspace.Document.Layers.Count ||
                this.layerIndex2 >= workspace.Document.Layers.Count)
            {
                throw new ArgumentOutOfRangeException("layerIndex[1|2]", "out of range");
            }

            this.workspace = workspace;
            this.layerIndex1 = layerIndex1;
            this.layerIndex2 = layerIndex2;
		}
	}
}
