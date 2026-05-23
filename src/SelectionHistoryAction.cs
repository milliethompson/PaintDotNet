/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
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
        PdnGraphicsPath savedSelection;
        DocumentWorkspace workspace;

        public bool IsSelectionEmpty
        {
            get
            {
                if (savedSelection == null)
                {
                    return true;
                }

                try
                {
                    return savedSelection.PointCount == 0;
                }

                catch (ArgumentException)
                {
                    return true;
                }
            }
        }

        public SelectionHistoryAction(string name, Image image, DocumentWorkspace workspace)
            : base(name, image)
        {
            this.workspace = workspace;

            if (this.workspace.Environment.IsSelectionEmpty)
            {
                savedSelection = null;
            }
            else
            {
                savedSelection = (PdnGraphicsPath)this.workspace.Environment.SelectedPath.Clone();
            }
        }

        protected override HistoryAction OnUndo()
        {
            SelectionHistoryAction sha = new SelectionHistoryAction(Name, Image, this.workspace);

            workspace.Environment.PerformSelectedPathChanging();
            workspace.Environment.SelectedPath.Reset();

            if (savedSelection != null)
            {
                workspace.Environment.SelectedPath.AddPath(savedSelection, false);
            }

            workspace.Environment.PerformSelectedPathChanged();

            return sha;
        }
    }
}
