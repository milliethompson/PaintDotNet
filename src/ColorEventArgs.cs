using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for ColorEventArgs.
    /// </summary>
    [Serializable]
    public class ColorEventArgs
        : System.EventArgs
    {
        private ColorBgra color;
        public ColorBgra Color
        {
            get
            {
                return color;
            }
        }

        public ColorEventArgs(ColorBgra color)
        {
            this.color = color;
        }
    }
}
