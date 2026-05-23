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
    /// Summary description for ToolClickedEventArgs.
    /// </summary>
    public class ToolClickedEventArgs
        : System.EventArgs
    {
        private Type toolType;
        public Type ToolType
        {
            get
            {
                return toolType;
            }
        }

        public ToolClickedEventArgs(Tool tool)
        {
            this.toolType = tool.GetType();
        }

        public ToolClickedEventArgs(Type toolType)
        {
            this.toolType = toolType;
        }
    }
}
