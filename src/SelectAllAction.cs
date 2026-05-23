/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing.Drawing2D;

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
                return PdnResources.GetString("SelectAllAction.Name");
            }
        }

        public override HistoryAction PerformAction()
        {
            SelectionHistoryAction sha = new SelectionHistoryAction(name, PdnResources.GetImage("Icons.MenuEditSelectAllIcon.png"), Workspace);

            Workspace.Environment.Selection.PerformChanging();
            Workspace.Environment.Selection.Reset();
            Workspace.Environment.Selection.SetContinuation(Workspace.Document.Bounds, CombineMode.Replace);
            Workspace.Environment.Selection.CommitContinuation();
            Workspace.Environment.Selection.PerformChanged();

            return sha;
        }

        public SelectAllAction(DocumentWorkspace workspace)
            : base(workspace, StaticName)
        {
        }
    }
}
