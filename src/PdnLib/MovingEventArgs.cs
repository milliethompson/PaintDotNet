/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace PaintDotNet
{
    public class MovingEventArgs
        : System.EventArgs
    {
        private Rectangle rectangle;
        public Rectangle Rectangle
        {
            get
            {
                return this.rectangle;
            }

            set
            {
                this.rectangle = value;
            }
        }

        public MovingEventArgs(Rectangle rect)
        {
            this.rectangle = rect;
        }
    }
}
