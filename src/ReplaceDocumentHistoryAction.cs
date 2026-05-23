using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// This HistoryAction can be used to save an entire Document for undo purposes
    /// Create this HistoryAction, then use DocumentWorkspace.SetDocument, then push
    /// this onto the DocumentWorkspace.History using PushNewAction.
    /// </summary>
    public class ReplaceDocumentHistoryAction
        : HistoryAction
    {
        private DocumentWorkspace workspace;
        private Document oldDocument;

        public ReplaceDocumentHistoryAction(string name, Image image, DocumentWorkspace workspace)
            : base(name, image)
        {
            this.workspace = workspace;
            this.oldDocument = workspace.Document;
        }

        protected override HistoryAction OnUndo()
        {
            ReplaceDocumentHistoryAction ha = new ReplaceDocumentHistoryAction(Name, Image, workspace);
            ha.id = this.ID;
            workspace.SetDocument(oldDocument);
            return ha;
        }
    }
}
