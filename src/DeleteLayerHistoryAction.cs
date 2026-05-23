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
		private Layer layer;
		private DocumentWorkspace workspace;

		protected override HistoryAction OnUndo()
		{
			HistoryAction ha = new NewLayerHistoryAction(Name, Image, workspace, layer);
            ha.ID = this.ID;
			workspace.Document.Layers.Insert(index, layer);
			((Layer)workspace.Document.Layers[index]).Invalidate();
			return ha;
		}

		public DeleteLayerHistoryAction(string name, Image image, DocumentWorkspace workspace, Layer deleteMe)
			: base(name, image)
		{
            this.workspace = workspace;
			this.index = workspace.Document.Layers.IndexOf(deleteMe);
			this.layer = deleteMe;
		}
	}
}
