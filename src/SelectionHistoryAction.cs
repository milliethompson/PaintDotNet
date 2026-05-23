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
using System.Drawing.Drawing2D;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for SelectionHistoryAction.
    /// </summary>
    public class SelectionHistoryAction
        : HistoryAction
    {
        [Serializable]
        private class OurHistoryActionData
            : HistoryActionData
        {
        }

        private object savedSelectionData;
        private DocumentWorkspace workspace;

        public SelectionHistoryAction(string name, Image image, DocumentWorkspace workspace)
            : base(name, image)
        {
            this.workspace = workspace;
            this.savedSelectionData = this.workspace.Environment.Selection.Save();
        }

        protected override HistoryAction OnUndo()
        {
            SelectionHistoryAction sha = new SelectionHistoryAction(Name, Image, this.workspace);
            workspace.Environment.Selection.Restore(this.savedSelectionData);
            return sha;
        }
    }
}
