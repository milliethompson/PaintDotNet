using System;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for SelectAllAction.
	/// </summary>
	public class SelectAllAction
		: DocumentAction
	{
		public override HistoryAction PerformAction()
		{
			SelectionHistoryAction sha = new SelectionHistoryAction(name, null, Workspace);

			Workspace.Environment.PerformSelectedPathChanging();
			Workspace.Environment.SelectedPath.Reset();
			Workspace.Environment.SelectedPath.AddRectangle(Workspace.Document.Bounds);
			Workspace.Environment.PerformSelectedPathChanged();

			return sha;
		}

		public SelectAllAction(DocumentWorkspace workspace)
			: base(workspace, "Select All")
		{
		}
	}
}
