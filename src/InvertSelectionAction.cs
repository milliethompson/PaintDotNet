using System;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for InvertSelectionAction.
	/// </summary>
	public class InvertSelectionAction
		: DocumentAction
	{
		public override HistoryAction PerformAction()
		{
			SelectionHistoryAction sha = new SelectionHistoryAction(name, null, Workspace);

			Workspace.Environment.PerformSelectedPathChanging();
			Workspace.Environment.SelectedPath.AddRectangle(Workspace.Document.Bounds);
			Workspace.Environment.PerformSelectedPathChanged();

			return sha;
		}
 
		public InvertSelectionAction(DocumentWorkspace workspace)
			: base(workspace, "Invert Selection")
		{
		}
	}
}
