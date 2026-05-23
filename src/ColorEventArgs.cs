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

		private bool takeFocus;
		public bool TakeFocus
		{
			get
			{
				return takeFocus;
			}
		}

        public ColorEventArgs(ColorBgra color, bool takeFocus)
        {
            this.color = color;
			this.takeFocus = takeFocus;
        }
    }
}
