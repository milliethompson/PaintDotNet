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
    /// Summary description for InvertSelectionAction.
    /// </summary>
    public class InvertSelectionAction
        : DocumentAction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("InvertSelectionAction.Name");
            }
        }

        public override HistoryAction PerformAction()
        {
            SelectionHistoryAction sha = new SelectionHistoryAction(name, 
                PdnResources.GetImage("Icons.MenuEditInvertSelectionIcon.bmp"), Workspace);

            PdnRegion selectedRegion = Workspace.Environment.Selection.CreateRegion();
            selectedRegion.Xor(Workspace.Document.Bounds);

            PdnGraphicsPath invertedSelection = PdnGraphicsPath.FromRegion(selectedRegion);
            selectedRegion.Dispose();

            Workspace.Environment.Selection.PerformChanging();
            Workspace.Environment.Selection.Reset();
            Workspace.Environment.Selection.SetContinuation(invertedSelection, CombineMode.Xor, true);
            Workspace.Environment.Selection.CommitContinuation();
            Workspace.Environment.Selection.PerformChanged();

            return sha;
        }
 
        public InvertSelectionAction(DocumentWorkspace workspace)
            : base(workspace, StaticName)
        {
        }
    }
}
