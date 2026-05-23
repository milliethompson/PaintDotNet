/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
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
            }
            else if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
            {   
				ColorBgra col;
				col = LiftColor(e.X, e.Y);
				this.Workspace.Environment.BackColor = col;
            }
        }

        public ColorPickerTool(DocumentWorkspace parent)
            : base(parent,
                   Utility.GetImageResource("Icons.ColorPickerToolIcon.bmp"),
                   "Color Picker",
                   "Gets current color from canvas",
                   "Left click to set foreground color, right click to set background color",
                   'd')
        {
            this.colorPickerToolCursor = new Cursor(Utility.GetResourceStream("Cursors.ColorPickerToolCursor.cur"));
            this.Cursor = this.colorPickerToolCursor;

            // initialize any state information you need
            mouseDown = false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DisposeImage();

                if (this.colorPickerToolCursor != null)
                {
                    this.colorPickerToolCursor.Dispose();
                    this.colorPickerToolCursor = null;
                }
            }
        }

    }
}