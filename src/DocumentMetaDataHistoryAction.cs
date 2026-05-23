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
    /// Saves the state of the Document's metadata.
    /// </summary>
    public class DocumentMetaDataHistoryAction
        : HistoryAction
    {
        private DocumentWorkspace workspace;

        [Serializable]
        private class OurHistoryActionData
            : HistoryActionData
        {
            private Document document;

            public Document Document
            {
                get
                {
                    return this.document;
                }
            }

            public OurHistoryActionData(Document document)
            {
                this.document = document;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (this.document != null)
                    {
                        this.document.Dispose();
                        this.document = null;
                    }
                }

                base.Dispose(disposing);
            }
        }

        public DocumentMetaDataHistoryAction(string name, Image image, DocumentWorkspace workspace)
            : base(name, image)
        {
            this.workspace = workspace;
            Document document = new Document(1, 1); // we need some place to store the metadata...
            document.ReplaceMetaDataFrom(workspace.Document);
            OurHistoryActionData data = new OurHistoryActionData(document);
            this.Data = data;
        }

        protected override HistoryAction OnUndo()
        {
            DocumentMetaDataHistoryAction redo = new DocumentMetaDataHistoryAction(this.Name, this.Image, this.workspace);
            OurHistoryActionData data = (OurHistoryActionData)this.Data;
            workspace.Document.ReplaceMetaDataFrom(data.Document);
            return redo;
        }
    }
}
