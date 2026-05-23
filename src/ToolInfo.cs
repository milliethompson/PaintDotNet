/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for ToolInfo.
	/// </summary>
    public class ToolInfo
    {
        private string name;
        private string description;
        private string helpText;
        private Image image;
        private char hotKey;
        private Type toolType;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
        }

        public string HelpText
        {
            get
            {
                return helpText;
            }
        }

        public Image Image
        {
            get
            {
                return image;
            }
        }

        public char HotKey
        {
            get
            {
                return hotKey;
            }
        }

        public Type ToolType
        {
            get
            {
                return toolType;
            }
        }

        public override bool Equals(object obj)
        {
            ToolInfo rhs = obj as ToolInfo;

            if (rhs == null)
            {
                return false;
            }

            return (name == rhs.name) && 
                   (description == rhs.description) && 
                   (helpText == rhs.helpText) && 
                   (hotKey == rhs.hotKey) &&
                   (toolType == rhs.toolType);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public ToolInfo(string name, string description, string helpText, Image image, char hotKey, Type toolType)
        {
            this.name = name;
            this.description = description;
            this.helpText = helpText;
            this.image = (Image)image.Clone();
            this.hotKey = hotKey;
            this.toolType = toolType;
        }

        [Obsolete]
        public ToolInfo(DocumentWorkspace workspace, Type toolType)
        {
            using (Tool tool = Tool.CreateTool(toolType, workspace))
            {
                this.name = tool.Name;
                this.description = tool.Description;
                this.helpText = tool.HelpText;
                this.image = (Image)tool.Image.Clone();
                this.hotKey = tool.HotKey;
                this.toolType = toolType;
            }
        }
    }
}
