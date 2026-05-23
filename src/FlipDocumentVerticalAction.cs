using System;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for FlipDocumentVerticalAction.
    /// </summary>
    public class FlipDocumentVerticalAction
        : FlipDocumentAction
    {
        public FlipDocumentVerticalAction(DocumentWorkspace workspace)
            : base(workspace, "Flip Vertical (all)", null, FlipType.Vertical)
        {
        }
    }
}
