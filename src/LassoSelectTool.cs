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

        protected override void OnActivate()
        {
            this.lassoToolCursor = new Cursor(PdnResources.GetResourceStream("Cursors.LassoSelectToolCursor.cur"));
            this.Cursor = this.lassoToolCursor;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (this.lassoToolCursor != null)
            {
                this.lassoToolCursor.Dispose();
                this.lassoToolCursor = null;
            }

            base.OnDeactivate ();
        }

        public LassoSelectTool(DocumentWorkspace workspace)
            : base(workspace,
                   PdnResources.GetImage("Icons.LassoSelectToolIcon.png"),
                   PdnResources.GetString("LassoSelectTool.Name"),
                   PdnResources.GetString("LassoSelectTool.HelpText"),
                   's')
        {
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
