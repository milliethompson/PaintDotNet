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
    /// This HistoryAction can be used to save an entire Document for undo purposes
    /// Create this HistoryAction, then use DocumentWorkspace.SetDocument, then push
    /// this onto the DocumentWorkspace.History using PushNewAction.
    /// </summary>
    public class ReplaceDocumentHistoryAction
        : HistoryAction
    {
        [Serializable]
        private sealed class ReplaceDocumentHistoryActionData
            : HistoryActionData
        {
            private Document oldDocument;

            public Document OldDocument
            {
                get
                {
                    return oldDocument;
                }
            }

            public ReplaceDocumentHistoryActionData(Document oldDocument)
            {
                this.oldDocument = oldDocument;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (oldDocument != null)
                    {
                        oldDocument.Dispose();
                        oldDocument = null;
                    }
                }
            }
        }

        private DocumentWorkspace workspace;

        public ReplaceDocumentHistoryAction(string name, Image image, DocumentWorkspace workspace)
            : base(name, image)
        {
            this.workspace = workspace;

            ReplaceDocumentHistoryActionData data = new ReplaceDocumentHistoryActionData(workspace.Document);
            this.Data = data;
        }

        protected override HistoryAction OnUndo()
        {
            ReplaceDocumentHistoryAction ha = new ReplaceDocumentHistoryAction(Name, Image, workspace);
            workspace.SetDocument(((ReplaceDocumentHistoryActionData)Data).OldDocument);
            return ha;
        }
    }
}
