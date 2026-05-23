using System;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for FlipDocumentHorizontalAction.
	/// </summary>
	public class FlipDocumentHorizontalAction
        : FlipDocumentAction
	{
		public FlipDocumentHorizontalAction(DocumentWorkspace workspace)
            : base(workspace, "Flip Horizontal (all)", null, FlipType.Horizontal)
        {
		}
	}
}
