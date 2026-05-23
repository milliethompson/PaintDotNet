using System;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for FlipLayerHorizontalAction.
	/// </summary>
	public class FlipLayerHorizontalAction
        : FlipLayerAction
	{
		public FlipLayerHorizontalAction(DocumentWorkspace workspace)
            : base(workspace, "Flip Horizontal", null, FlipType.Horizontal)
		{
		}
	}
}
