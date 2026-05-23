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
using System.Windows.Forms;

namespace PaintDotNet
{
    public class ColorPickerTool : Tool
    {
        private bool mouseDown;
        private Cursor colorPickerToolCursor;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseDown)
            {
                PickColor(e);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            mouseDown = true;
        
            PickColor(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            mouseDown = false;
        }

        private ColorBgra LiftColor(int x, int y)
        {
            ColorBgra newColor;
            newColor = ((BitmapLayer)this.Workspace.ActiveLayer).Surface[x, y];
            return newColor;
        }

        private void PickColor(MouseEventArgs e)
        {
            if (!Utility.IsPointInRectangle(e.X, e.Y, Workspace.Document.Bounds))
            {
                return;
            }

            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                ColorBgra col;
                col = LiftColor(e.X, e.Y);
                this.Workspace.Environment.ForeColor = col;
                this.Workspace.Widgets.ColorsForm.WhichUserColor = WhichUserColor.Foreground;
            }
            else if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
            {   
                ColorBgra col;
                col = LiftColor(e.X, e.Y);
                this.Workspace.Environment.BackColor = col;
                this.Workspace.Widgets.ColorsForm.WhichUserColor = WhichUserColor.Background;
            }
        }

        protected override void OnActivate()
        {
            this.colorPickerToolCursor = new Cursor(PdnResources.GetResourceStream("Cursors.ColorPickerToolCursor.cur"));
            this.Cursor = this.colorPickerToolCursor;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (this.colorPickerToolCursor != null)
            {
                this.colorPickerToolCursor.Dispose();
                this.colorPickerToolCursor = null;
            }

            base.OnDeactivate ();
        }

        public ColorPickerTool(DocumentWorkspace parent)
            : base(parent,
                   PdnResources.GetImage("Icons.ColorPickerToolIcon.png"),
                   PdnResources.GetString("ColorPickerTool.Name"),
                   PdnResources.GetString("ColorPickerTool.HelpText"),
                   'p')
        {
            // initialize any state information you need
            mouseDown = false;
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