using System;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class ColorPickerTool : Tool
    {
        private bool mouseDown;

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

        private ColorBgra LiftColor( int x, int y )
        {
            ColorBgra newColor;
            newColor = ((BitmapLayer)this.Workspace.ActiveLayer).Surface[x, y];
            return newColor;
        }

        private void PickColor(MouseEventArgs e)
        {
            if (!Utility.IsPointInRectangle(new Point(e.X, e.Y), new Rectangle(new Point(0,0), this.Workspace.Document.Size)))
            {
                return;
            }

            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                this.Workspace.Environment.ForeColor = LiftColor(e.X, e.Y);
            }
            else
                if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
            {   
                this.Workspace.Environment.BackColor = LiftColor(e.X, e.Y);
            }
        }

        public ColorPickerTool(DocumentWorkspace parent)
            : base(parent)
        {
            toolBarImage = Utility.GetImageResource("Icons.ColorPickerToolIcon.bmp");
            cursor = new Cursor(Utility.GetResourceStream("Cursors.ColorPickerCursor.cur"));
            name = "Color Picker";
            description = "Gets current color from canvas";

            // initialize any state information you need
            mouseDown = false;
        }
    }
}