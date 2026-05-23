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
    /// <summary>
    /// Summary description for Scanline.
    /// </summary>
    public struct Scanline
    {
        private int x;
        private int y;
        private int length;

        public int X
        {
            get
            {
                return x;
            }
        }

        public int Y
        {
            get
            {
                return y;
            }
        }

        public int Length
        {
            get
            {
                return length;
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return length.GetHashCode() + x.GetHashCode() + y.GetHashCode();
            }
        }
        
        public override bool Equals(object obj)
        {
            if (obj is Scanline)
            {
                Scanline rhs = (Scanline)obj;
                return x == rhs.x && y == rhs.y && length == rhs.length;
            }
            else
            {
                return false;
            }
        }

        public static bool operator== (Scanline lhs, Scanline rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.length == rhs.length;
        }

        public static bool operator!= (Scanline lhs, Scanline rhs)
        {
            return !(lhs == rhs);
        }

        public override string ToString()
        {
            return "(" + x + "," + y + "):[" + length.ToString() + "]";
        }

        public Scanline(int x, int y, int length)
        {
            this.x = x;
            this.y = y;
            this.length = length;
        }
    }
}
