using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for Scanline.
    /// </summary>
    public struct Scanline
    {
        private Point point;

        public Point Point
        {
            get
            {
                return point;
            }
        }

        private int length;

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
                return length.GetHashCode() + point.GetHashCode();
            }
        }
        
        public override bool Equals(object obj)
        {
            Scanline s = (Scanline)obj;
            return point == s.point && length == s.length;
        }

        public static bool operator== (Scanline lhs, Scanline rhs)
        {
            return lhs.point == rhs.point && lhs.length == rhs.length;
        }

        public static bool operator!= (Scanline lhs, Scanline rhs)
        {
            return !(lhs == rhs);
        }

        public override string ToString()
        {
            return "(" + point.X.ToString() + "," + point.Y.ToString() + "):[" + length.ToString() + "]";
        }

        public Scanline(Point point, int length)
        {
            this.point = point;
            this.length = length;
        }
    }
}
