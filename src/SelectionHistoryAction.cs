using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for SelectionHistoryAction.
	/// </summary>
	public class SelectionHistoryAction
		: HistoryAction
	{
		GraphicsPath savedSelection;
		DocumentWorkspace workspace;

        public bool IsSelectionEmpty
        {
            get
            {
                try
                {
                    return savedSelection.PointCount == 0;
                }

                catch (ArgumentException)
                {
                    return true;
                }

                catch (NullReferenceException)
                {
                    return true;
                }
            }
        }

		public SelectionHistoryAction(string name, Image image, DocumentWorkspace workspace)
			: base(name, image)
		{
			this.workspace = workspace;

			if (this.workspace.Environment.IsSelectionEmpty)
			{
				savedSelection = null;
			}
			else
			{
				savedSelection = (GraphicsPath)this.workspace.Environment.SelectedPath.Clone();
			}
		}

		protected override HistoryAction OnUndo()
		{
			SelectionHistoryAction sha = new SelectionHistoryAction(Name, Image, this.workspace);
			sha.id = id;

			workspace.Environment.PerformSelectedPathChanging();
			workspace.Environment.SelectedPath.Reset();

			if (savedSelection != null)
			{
				workspace.Environment.SelectedPath.AddPath(savedSelection, false);
			}

			workspace.Environment.PerformSelectedPathChanged();

			return sha;
		}
	}
}
