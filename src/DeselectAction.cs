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
    /// Summary description for DeselectAction.
    /// </summary>
    public class DeselectAction
        : DocumentAction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("DeselectAction.Name");
            }
        }

        public static Image StaticImage
        {
            get
            {
                return PdnResources.GetImage("Icons.MenuEditDeselectIcon.bmp");
            }
        }
        
        public override HistoryAction PerformAction()
        {
            if (Workspace.Environment.Selection.IsEmpty)
            {
                return null;
            }
            else
            {
                SelectionHistoryAction sha = new SelectionHistoryAction(Name, StaticImage, Workspace);
                Workspace.Environment.Selection.Reset();
                return sha;
            }
        }

        public DeselectAction(DocumentWorkspace workspace)
            : base(workspace, DeselectAction.StaticName)
        {
        }
    }
}
