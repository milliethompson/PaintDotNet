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
    /// A Tool may implement this class in order to provide history actions that do
    /// not deactivate the tool while being undone or redone.
    /// </summary>
    public abstract class ToolHistoryAction
        : HistoryAction
    {
        private DocumentWorkspace workspace;
        private Type toolType;

        protected DocumentWorkspace Workspace
        {
            get
            {
                return this.workspace;
            }
        }

        public Type ToolType
        {
            get
            {
                return this.toolType;
            }
        }

        protected abstract HistoryAction OnToolUndo();

        protected sealed override HistoryAction OnUndo()
        {
            if (this.workspace.Environment.GetToolType() != this.toolType)
            {
                this.workspace.Environment.SetTool(this.toolType, this.workspace);
            }

            return OnToolUndo();
        }

        public ToolHistoryAction(DocumentWorkspace workspace, string name, Image image)
            : base(name, image)
        {
            this.workspace = workspace;
            this.toolType = workspace.Environment.GetToolType();
        }
    }
}
