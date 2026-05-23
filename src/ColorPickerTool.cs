using System;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class ColorPickerTool : Tool
    {
        private bool mouseDown;

		public override char HotKey
		{
			get
			{
				return 'd';
			}
		}

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
            : base(parent)
        {
            toolBarImage = Utility.GetImageResource("Icons.ColorPickerToolIcon.bmp");
            cursor = new Cursor(Utility.GetResourceStream("Cursors.ColorPickerToolCursor.cur"));
            name = "Color Picker";
            description = "Gets current color from canvas";
			helpText = "Left click to set foreground color, right click to set background color";

            // initialize any state information you need
            mouseDown = false;
        }
    }
}