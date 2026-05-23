/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for RectangleSelectTool.
    /// </summary>
    public class MagicWandTool
        : FloodTool
    {
        private Cursor cursorMouseUp;
        private Cursor cursorMouseDown;
        private CombineMode combineMode;

        // nothing = replace
        // Ctrl = union
        // RMB = exclude
        // Ctrl+RMB = xor

        protected override void OnActivate()
        {
            Workspace.EnableSelectionTinting = true;
            this.cursorMouseUp = new Cursor(PdnResources.GetResourceStream("Cursors.MagicWandToolCursor.cur"));
            this.cursorMouseDown = new Cursor(PdnResources.GetResourceStream("Cursors.MagicWandToolCursorMouseDown.cur"));
            this.Cursor = cursorMouseUp;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (cursorMouseUp != null)
            {
                cursorMouseUp.Dispose();
                cursorMouseUp = null;
            }

            if (cursorMouseDown != null)
            {
                cursorMouseDown.Dispose();
                cursorMouseDown = null;
            }

            Workspace.EnableSelectionTinting = false;
            base.OnDeactivate();
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            Cursor = cursorMouseUp;
            base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Cursor = cursorMouseDown;

            if ((ModifierKeys & Keys.Control) != 0 && e.Button == MouseButtons.Left)
            {
                this.combineMode = CombineMode.Union;
            }
            else if ((ModifierKeys & Keys.Control) != 0 && e.Button == MouseButtons.Right)
            {
                this.combineMode = CombineMode.Xor;
            }
            else if (e.Button == MouseButtons.Right)
            {
                this.combineMode = CombineMode.Exclude;
            }
            else
            {
                this.combineMode = CombineMode.Replace;
            }

            base.OnMouseDown(e);
        }

        protected override void PerimeterFound(Point[][] polygonSet)
        {
            SelectionHistoryAction undoAction = new SelectionHistoryAction(this.Name, this.Image, Workspace);

            Workspace.Environment.Selection.PerformChanging();
            Workspace.Environment.Selection.SetContinuation(polygonSet, this.combineMode);
            Workspace.Environment.Selection.CommitContinuation();
            Workspace.Environment.Selection.PerformChanged();

            Workspace.History.PushNewAction(undoAction);           
        }

        public MagicWandTool(DocumentWorkspace workspace)
            : base(workspace,
                   PdnResources.GetImage("Icons.MagicWandToolIcon.png"),
                   PdnResources.GetString("MagicWandTool.Name"),
                   PdnResources.GetString("MagicWandTool.HelpText"), 
                   's')
        {
            LimitToSelection = false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DisposeImage();
            }
        }
    }
}