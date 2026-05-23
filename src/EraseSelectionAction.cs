using System;
using System.Drawing;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for EraseSelectionAction.
	/// </summary>
	public class EraseSelectionAction
		: DocumentAction
	{
		public override HistoryAction PerformAction()
		{
			if (Workspace.Environment.IsSelectionEmpty)
			{
				return null;
			}

            Type oldToolType = Workspace.Environment.GetToolType();
            Workspace.Environment.SetTool(null);
            SelectionHistoryAction sha = new SelectionHistoryAction(string.Empty, null, Workspace);

			Region region = Workspace.Environment.CreateSelectedRegion();

			region.Intersect(Workspace.Document.Bounds);
			BitmapLayer layer = ((BitmapLayer)Workspace.ActiveLayer);
            Region simplifiedRegion = Utility.SimplifyAndInflateRegion(region);

			HistoryAction ha = layer.CreateHistoryAction(name, null, simplifiedRegion);
            //new UnaryPixelOps.SetChannel(ColorBgra.AlphaChannel, 0).Apply(layer.Surface, region);
            new UnaryPixelOps.Constant(ColorBgra.FromBgra(255, 255, 255, 0)).Apply(layer.Surface, region);
			layer.Invalidate(simplifiedRegion);

			Workspace.Document.Invalidate(simplifiedRegion);

            simplifiedRegion.Dispose();

            Workspace.Environment.PerformSelectedPathChanging();
            Workspace.Environment.SelectedPath.Reset();
            Workspace.Environment.PerformSelectedPathChanged();
            Workspace.Environment.SetTool(oldToolType, Workspace);

            return new CompoundHistoryAction(this.Name, null, new HistoryAction[] { ha, sha });
		}

		public EraseSelectionAction(DocumentWorkspace workspace)
			: base(workspace, "Clear Selection")
		{
		}
	}
}
