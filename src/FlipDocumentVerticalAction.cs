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
    /// Summary description for FlipDocumentVerticalAction.
    /// </summary>
    public class FlipDocumentVerticalAction
        : FlipDocumentAction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("FlipDocumentVerticalAction.Name");
            }
        }

        public FlipDocumentVerticalAction(DocumentWorkspace workspace)
            : base(workspace, 
                   StaticName,
                   PdnResources.GetImage("Icons.MenuImageFlipVerticalIcon.png"), 
                   FlipType.Vertical)
        {
        }
    }
}
