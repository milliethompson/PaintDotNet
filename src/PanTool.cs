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
		private Cursor cursorMouseDown, cursorMouseUp;

		public override char HotKey
		{
			get
			{
				return 'h';
			}
		}

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown (e);
			Cursor = cursorMouseDown;
            lastMouseXY = new Point(e.X, e.Y);
            tracking = true;
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp (e);
			Cursor = cursorMouseUp;
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
        }

        public PanTool(DocumentWorkspace workspace)
            : base(workspace)
        {
            toolBarImage = Utility.GetImageResource("Icons.PanToolIcon.bmp");
            name = "Pan";
            description = "Allows you to scroll throughout the document.";
			helpText = "When zoomed in close, click and drag to navigate the image";

			// cursor-action assignments
			cursorMouseDown = new Cursor(Utility.GetResourceStream("Cursors.PanToolCursorMouseDown.cur"));
			cursorMouseUp = new Cursor(Utility.GetResourceStream("Cursors.PanToolCursor.cur"));
			Cursor = cursorMouseUp;
			autoScroll = false;

            tracking = false;
        }
    }
}
