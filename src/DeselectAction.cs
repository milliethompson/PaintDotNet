/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for DeselectAction.
    /// </summary>
    public class DeselectAction
        : DocumentAction
    {
        public static string StaticName
        {
            get
            {
                return "Deselect";
            }
        }

        public static Image StaticImage
        {
            get
            {
                return Utility.GetImageResource("Icons.MenuEditDeselectIcon.bmp");
            }
        }
        
        public override HistoryAction PerformAction()
        {
            if (Workspace.Environment.IsSelectionEmpty)
            {
                return null;
            }
            else
            {
                SelectionHistoryAction sha = new SelectionHistoryAction(Name, StaticImage, Workspace);

                Workspace.Environment.PerformSelectedPathChanging();
                Workspace.Environment.SelectedPath.Reset();
                Workspace.Environment.PerformSelectedPathChanged();

                return sha;
            }
        }

        public DeselectAction(DocumentWorkspace workspace)
            : base(workspace, DeselectAction.StaticName)
        {
        }
    }
}
