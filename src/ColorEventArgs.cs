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
