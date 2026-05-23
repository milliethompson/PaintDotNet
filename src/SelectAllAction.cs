/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for SelectAllAction.
    /// </summary>
    public class SelectAllAction
        : DocumentAction
    {
        public static string StaticName
        {
            get
            {
                return "Select All";
            }
        }

        public override HistoryAction PerformAction()
        {
            SelectionHistoryAction sha = new SelectionHistoryAction(name, Utility.GetImageResource("Icons.MenuEditSelectAllIcon.bmp"), Workspace);

            Workspace.Environment.PerformSelectedPathChanging();
            Workspace.Environment.SelectedPath.Reset();
            Workspace.Environment.SelectedPath.AddRectangle(Workspace.Document.Bounds);
            Workspace.Environment.PerformSelectedPathChanged();

            return sha;
        }

        public SelectAllAction(DocumentWorkspace workspace)
            : base(workspace, StaticName)
        {
        }
    }
}
