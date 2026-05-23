/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for PanTool.
    /// </summary>
    public class PanTool
        : Tool
    {
        private bool tracking = false;
		private Point lastMouseXY;
		private Cursor cursorMouseDown;
        private Cursor cursorMouseUp;
        private Cursor cursorMouseInvalid;

        private bool CanPan()
        {
            if (Workspace.DocumentView.VisibleDocumentRectangle.Size == Workspace.Document.Size)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown (e);

            lastMouseXY = new Point(e.X, e.Y);
            tracking = true;

            if (CanPan())
            {
                Cursor = cursorMouseDown;
            }
            else
            {
                Cursor = cursorMouseInvalid;
            }
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp (e);

            if (CanPan())
            {
                Cursor = cursorMouseUp;
            }
            else
            {
                Cursor = cursorMouseInvalid;
            }

            tracking = false;
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove (e);

            if (tracking)
            {
                Point mouseXY = new Point(e.X, e.Y);
                Size delta = new Size(mouseXY.X - lastMouseXY.X, mouseXY.Y - lastMouseXY.Y);

                if (delta.Width != 0 || delta.Height != 0)
                {
                    Point scrollPos = Point.Round(Workspace.DocumentView.DocumentScrollPosition);
                    Point newScrollPos = new Point(scrollPos.X - delta.Width, scrollPos.Y - delta.Height);
                    
                    Workspace.DocumentView.DocumentScrollPosition = newScrollPos;
                    Workspace.DocumentView.Update();

                    lastMouseXY = mouseXY;
                    lastMouseXY.X -= delta.Width;
                    lastMouseXY.Y -= delta.Height;
                }
            }
            else
            {
                if (CanPan())
                {
                    Cursor = cursorMouseUp;
                }
                else
                {
                    Cursor = cursorMouseInvalid;
                }
            }
        }

        public PanTool(DocumentWorkspace workspace)
            : base(workspace,
                   Utility.GetImageResource("Icons.PanToolIcon.bmp"),
                   "Pan",
                   "Allows you to scroll throughout the document.",
                   "When zoomed in close, click and drag to navigate the image",
                   'h')
        {
			// cursor-action assignments
			cursorMouseDown = new Cursor(Utility.GetResourceStream("Cursors.PanToolCursorMouseDown.cur"));
			cursorMouseUp = new Cursor(Utility.GetResourceStream("Cursors.PanToolCursor.cur"));
            cursorMouseInvalid = new Cursor(Utility.GetResourceStream("Cursors.PanToolCursorInvalid.cur"));
			Cursor = cursorMouseUp;
			autoScroll = false;

            tracking = false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DisposeImage();

                if (cursorMouseDown != null)
                {
                    cursorMouseDown.Dispose();
                    cursorMouseDown = null;
                }

                if (cursorMouseUp != null)
                {
                    cursorMouseUp.Dispose();
                    cursorMouseUp = null;
                }

                if (cursorMouseInvalid != null)
                {
                    cursorMouseInvalid.Dispose();
                    cursorMouseInvalid = null;
                }
            }
        }

    }
}
