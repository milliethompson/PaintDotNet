/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

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
            : base(workspace, "Flip Horizontal", Utility.GetImageResource("Icons.MenuLayersFlipHorizontalIcon.bmp"), FlipType.Horizontal)
        {
        }
    }
}
