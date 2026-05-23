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
    /// Summary description for FlipLayerVerticalAction.
    /// </summary>
    public class FlipLayerVerticalAction
        : FlipLayerAction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("FlipLayerVerticalAction.Name");
            }
        }

        public FlipLayerVerticalAction(DocumentWorkspace workspace)
            : base(workspace, 
                   StaticName,
                   PdnResources.GetImage("Icons.MenuLayersFlipVerticalIcon.bmp"), 
                   FlipType.Vertical)
        {
        }
    }
}
