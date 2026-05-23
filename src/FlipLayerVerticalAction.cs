using System;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for FlipLayerVerticalAction.
    /// </summary>
    public class FlipLayerVerticalAction
        : FlipLayerAction
    {
        public FlipLayerVerticalAction(DocumentWorkspace workspace)
            : base(workspace, "Flip Vertical", Utility.GetImageResource("Icons.MenuLayersFlipVerticalIcon.bmp"), FlipType.Vertical)
        {
        }
    }
}
