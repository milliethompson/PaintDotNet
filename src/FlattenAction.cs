using System;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for FlattenAction.
	/// </summary>
	public class FlattenAction
        : DocumentAction
	{
        public override HistoryAction PerformAction()
        {
            ReplaceDocumentHistoryAction rdha = new ReplaceDocumentHistoryAction(name, null, Workspace);
            Workspace.SetDocument(Workspace.Document.Flatten());
            return rdha;
        }

		public FlattenAction(DocumentWorkspace workspace)
            : base(workspace, "Flatten")
		{
		}
	}
}
