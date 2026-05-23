using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for RectangleSelectTool.
    /// </summary>
    public class RectangleSelectTool
        : Tool
    {
        private bool tracking;   // if true, then the left mouse button is down and OnMouseMove will know to actually do stuff
        private bool hasMoved = false;
        private Point firstXY;
        private Point lastXY;
        private PdnGraphicsPath originalCopy;
        private SelectionHistoryAction undoAction;
        private Rectangle ourRect = Rectangle.Empty;
        private DateTime startTime;

        protected override void OnActivate()
        {
            base.OnActivate ();
            hasMoved = false;
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate ();

            if (originalCopy != null)
            {
                originalCopy.Dispose();
                originalCopy = null;
            }
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown (e);

            tracking = true;
            firstXY = new Point(e.X, e.Y);
            lastXY = firstXY;

            undoAction = new SelectionHistoryAction("sentinel", toolBarImage, Workspace);

            // if they are NOT holding down control, reset the path
            // we don't use the DeselectAction because we only want to end up adding
            // one action to the history stack, not two
            if (!((ModifierKeys & Keys.Control) == Keys.Control))
            {
                Workspace.Environment.PerformSelectedPathChanging();
                Workspace.Environment.SelectedPath.Reset();
                Workspace.Environment.PerformSelectedPathChanged();
            }

            if (originalCopy != null)
            {
                originalCopy.Dispose();
            }

            if (Workspace.Environment.IsSelectionEmpty)
            {
                originalCopy = null;
            }
            else
            {
                originalCopy = (PdnGraphicsPath)Workspace.Environment.SelectedPath.Clone();
            }

            hasMoved = false;
            startTime = DateTime.Now;
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove (e);

            if (tracking && Utility.TicksToMs((DateTime.Now - startTime).Ticks) > 100)
            {
                Point mouseXY = new Point(e.X, e.Y);

                Workspace.Environment.PerformSelectedPathChanging();
                Workspace.Environment.SelectedPath.Reset();

                if (originalCopy != null)
                {
                    Workspace.Environment.SelectedPath.AddPath(originalCopy, false);
                }

                Rectangle rect;
                
                if ((ModifierKeys & Keys.Shift) != Keys.None)
                {
                    rect = Utility.PointsToConstrainedRectangle(firstXY, mouseXY);
                }
                else
                {
                    rect = Utility.PointsToRectangle(firstXY, mouseXY);
                }

                rect = Rectangle.Intersect(rect, Workspace.Document.Bounds);
                ourRect = rect;

                if (!rect.IsEmpty)
                {
                    Workspace.Environment.SelectedPath.AddRectangle(rect);
                }

                Workspace.Environment.PerformSelectedPathChanged();

                lastXY = mouseXY;
                hasMoved = true;
            }
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp (e);

            if (tracking)
            {
                if (hasMoved)
                {
                    if (!ourRect.IsEmpty)
                    {
                        undoAction.Name = "Rectangle Select";
                        Workspace.History.PushNewAction(undoAction);
                        Workspace.Environment.PerformSelectedPathChanging();
                        Workspace.Environment.SelectedPath.CloseFigure();
                        Workspace.Environment.PerformSelectedPathChanged();
                    }
                }

                tracking = false;
                hasMoved = false;
                undoAction = null;
            }
        }

        protected override void OnClick()
        {
            base.OnClick ();

            if (!hasMoved)
            {
                if (!(undoAction.IsSelectionEmpty && Workspace.Environment.IsSelectionEmpty))
                {
                    if (this.ModifierKeys == Keys.None)
                    {
                        undoAction.Name = "Deselect";
                        undoAction.Image = Image.FromStream(Utility.GetResourceStream("Icons.MenuEditDeselectIcon.bmp"));
                        Workspace.History.PushNewAction(undoAction);
                        Workspace.Environment.PerformSelectedPathChanging();
                        Workspace.Environment.SelectedPath.Reset();
                        Workspace.Environment.PerformSelectedPathChanged();
                    }
                }

                tracking = false;
                hasMoved = false;
                undoAction = null;
            }
        }

        public RectangleSelectTool(DocumentWorkspace workspace)
            : base(workspace)
        {
            toolBarImage = Utility.GetImageResource("Icons.RectangleSelectToolIcon.bmp");
            cursor = new Cursor(Utility.GetResourceStream("Cursors.RectangleSelectToolCursor.cur"));
            name = "Rectangle Select";
            description = "Allows you to select a rectangular region of the image.";

            tracking = false;
        }
    }
}
