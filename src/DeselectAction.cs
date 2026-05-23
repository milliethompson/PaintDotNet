using System;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for DeselectAction.
	/// </summary>
	public class DeselectAction
		: DocumentAction
	{
		public override HistoryAction PerformAction()
		{
			if (Workspace.Environment.IsSelectionEmpty)
			{
				return null;
			}
			else
			{
				SelectionHistoryAction sha = new SelectionHistoryAction(Name, null, Workspace);
            
				Workspace.Environment.PerformSelectedPathChanging();
				Workspace.Environment.SelectedPath.Reset();
				Workspace.Environment.PerformSelectedPathChanged();

				return sha;
			}
		}

		public DeselectAction(DocumentWorkspace workspace)
			: base(workspace, "Deselect")
		{
		}
	}
}
