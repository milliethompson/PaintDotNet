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
	/// WorkspaceActions provide a way to encapsulate "global" actions in a properly
	/// object-oriented manner.
	/// WorkspaceActions are not, by definition, undoable. However, they may modify
	/// the history. This is unlike DocumentActions which must emit HistoryActions but
	/// otherwise must not touch the History.
	/// Examples of WorkspaceActions include Cut, Paste, Open, New, Save, etc.
	/// WorkspaceActions may also optionally take parameters. It is recommended that
	/// you provide an overload to allow strongly-typed execution. For instance:
	/// 
	///     public void PerformAction(params object[] parameters)
	///     {
	///         PerformAction((int)parameters[0], (int)parameters[1]);
	///     }
	///     
	///     public void PerformAction(int x, int y)
	///     {
	///         ...
	///     }
	///     
	/// To return an error, throw an exception.
	/// </summary>
	public abstract class WorkspaceAction
	{
        private DocumentWorkspace workspace;

        protected DocumentWorkspace Workspace
        {
            get
            {
                return workspace;
            }
        }

        public abstract void PerformAction(params object[] parameters);

		public WorkspaceAction(DocumentWorkspace workspace)
		{
            this.workspace = workspace;
		}
	}
}
