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
            : base(workspace, "Flip Vertical (all)", Utility.GetImageResource("Icons.MenuImageFlipVerticalIcon.bmp"), FlipType.Vertical)
        {
        }
    }
}
