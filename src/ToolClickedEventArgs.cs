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
