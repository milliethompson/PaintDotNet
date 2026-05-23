/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    /// <summary>
    /// Couples a WorkspaceAction with some parameters.
    /// </summary>
    public class InvokeWorkspaceAction
        : WorkspaceAction
    {
        private Type workspaceAction;

        public override void PerformAction(params object[] parameters)
        {
            Workspace.PerformAction(workspaceAction, parameters);
        }

        public InvokeWorkspaceAction(DocumentWorkspace workspace, Type workspaceAction, params object[] parameters)
            : base(workspace)
        {
            this.workspaceAction = workspaceAction;
        }
    }
}
