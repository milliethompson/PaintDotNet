/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
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
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("FlipLayerHorizontalAction.Name");
            }
        }

        public FlipLayerHorizontalAction(DocumentWorkspace workspace)
            : base(workspace, 
                   StaticName, 
                   PdnResources.GetImage("Icons.MenuLayersFlipHorizontalIcon.bmp"), 
                   FlipType.Horizontal)
        {
        }
    }
}
