/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for LassoSelectTool.
    /// </summary>
    public class LassoSelectTool
        : SelectionTool
    {
        private Cursor lassoToolCursor;

        public LassoSelectTool(DocumentWorkspace workspace)
            : base(workspace,
                   Utility.GetImageResource("Icons.LassoSelectToolIcon.bmp"),
                   "Lasso Select",
                   "Allows you to select an arbitrary region of the image.",
                   "Click and move the mouse to select an arbitrary region of the image",
                   's')
        {
            this.lassoToolCursor = new Cursor(Utility.GetResourceStream("Cursors.LassoSelectToolCursor.cur"));
            this.Cursor = this.lassoToolCursor;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DisposeImage();

                if (this.lassoToolCursor != null)
                {
                    this.lassoToolCursor.Dispose();
                    this.lassoToolCursor = null;
                }
            }
        }
    }
}
