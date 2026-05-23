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
        : Tool
	{
        private bool tracking = false;
		private bool hasMoved = false;
		private SelectionHistoryAction undoAction;
        private GraphicsPath originalCopy;
        private ArrayList tracePoints = null;
        private DateTime startTime;

        protected override void OnActivate()
        {
            base.OnActivate ();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate ();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown (e);

            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                tracking = true;
                startTime = DateTime.Now;

                tracePoints = new ArrayList();
                tracePoints.Add(new Point(e.X, e.Y));

				undoAction = new SelectionHistoryAction("sentinel", toolBarImage, Workspace);

                // if the user is holding down the shift key then we don't want to reset the path, merely append to it
				// we don't use the DeselectAction because we only want to end up adding
				// one action to the history stack, not two
				if (!((ModifierKeys & Keys.Control) == Keys.Control))
                {
                    Workspace.Environment.PerformSelectedPathChanging();
                    Workspace.Environment.SelectedPath.Reset();
                    Workspace.Environment.SelectedPath.CloseAllFigures();
                    Workspace.Environment.PerformSelectedPathChanged();
                }

                if (Workspace.Environment.IsSelectionEmpty)
                {
                    originalCopy = null;
                }
                else
                {
                    originalCopy = (GraphicsPath)Workspace.Environment.SelectedPath.Clone();
                }

				hasMoved = false;
            }
        }
        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove (e);

            if (tracking)
            {
                Point mouseXY = new Point(e.X, e.Y);
                tracePoints.Add(mouseXY);

                if (tracePoints.Count > 2)
                {
                    Point[] polygon = Utility.SutherlandHodgman(Workspace.Document.Bounds, tracePoints);

                    if (polygon.Length > 2)
                    {
                        Workspace.Environment.PerformSelectedPathChanging();
                        Workspace.Environment.SelectedPath.Reset();

                        if (originalCopy != null)
                        {
                            Workspace.Environment.SelectedPath.AddPath(originalCopy, false);
                        }

                        Workspace.Environment.SelectedPath.AddLines(polygon);
                        Workspace.Environment.PerformSelectedPathChanged();
                    }

                    if (mouseXY != (Point)tracePoints[0])
                    {
                        hasMoved = true;
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp (e);

            if (tracking)
            {
                OnMouseMove(e);

				if (hasMoved)
				{
                    Point[] polygon = Utility.SutherlandHodgman(Workspace.Document.Bounds, tracePoints);

                    if (polygon.Length > 2)
                    {
                        if (Utility.TicksToMs((DateTime.Now - startTime).Ticks) > 100) // 100 is a magic number, and says "if they weren't selecting stuff for more than 40ms, we ignore them and reset to the last selection state"
                        {
                            undoAction.Name = "Lasso Select";
                            Workspace.History.PushNewAction(undoAction);
                            Workspace.Environment.PerformSelectedPathChanging();
                            Workspace.Environment.SelectedPath.CloseFigure();
                            Workspace.Environment.PerformSelectedPathChanged();
                        }
                        else
                        {
                            Workspace.Environment.PerformSelectedPathChanging();
                            Workspace.Environment.SelectedPath.Reset();

                            if (originalCopy != null)
                            {
                                Workspace.Environment.SelectedPath.AddPath(this.originalCopy, false);
                            }

                            Workspace.Environment.PerformSelectedPathChanged();
                        }
                    }
                    else
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
                    }
				}

                tracking = false;
				hasMoved = false;
            }
        }

		protected override void OnClick()
		{
			base.OnClick ();

            Point[] polygon = Utility.SutherlandHodgman(Workspace.Document.Bounds, tracePoints);

			if (!hasMoved || polygon.Length <= 2)
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
			}
		}

		public LassoSelectTool(DocumentWorkspace workspace)
            : base(workspace)
		{
            this.name = "Lasso Select";
            this.description = "Allows you to select an arbitrary region of the image.";
            this.toolBarImage = Utility.GetImageResource("Icons.LassoSelectToolIcon.bmp");
            this.cursor = new Cursor(Utility.GetResourceStream("Cursors.LassoSelectToolCursor.cur"));

            tracking = false;
		}
	}
}
