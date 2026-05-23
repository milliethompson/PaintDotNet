/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for PointVector.
    /// </summary>
    public class PointVector
        : IEnumerable
    {
        private int capacity;
        private int count = 0;
        private Point[] array;
        
        public PointVector()
            : this(32)
        {
        }

        public PointVector(int capacity)
        {
            this.array = new Point[capacity];
            this.capacity = capacity;
        }

        public void Add(Point pt)
        {
            if (count >= capacity)
            {
                Grow(count + 1);
            }

            array[count++] = pt;
        }

        public void Clear()
        {
            count = 0;
        }

        public Point this[int index]
        {
            get
            {
                return Get(index);
            }

            set
            {
                Set(index, value);
            }
        }

        public Point Get(int index)
        {
            if (index < 0 || index >= count)
            {
                throw new ArgumentOutOfRangeException("index", index, "0 <= index < count");
            }

            return array[index];
        }

        public unsafe Point GetUnchecked(int index)
        {
            return array[index];
        }

        public void Set(int index, Point pt)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", index, "0 <= index");
            }

            if (index >= capacity)
            {
                Grow(index + 1);
            }

            array[index] = pt;
        }

        public int Count
        {
            get
            {
                return count;
            }
        }

        private class PointVectorEnumerator : IEnumerator
        {
            private PointVector target;
            private int index;

            public PointVectorEnumerator(PointVector target)
            {
                this.target = target;
            }

            public void Reset()
            {
                index = -1;
            }

            public object Current
            {
                get
                {
                    return target.Get(index);
                }
            }

            public bool MoveNext()
            {
                if (index + 1 < target.count)
                {
                    ++index;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new PointVectorEnumerator(this);
        }

        private void Grow(int min)
        {
            int newSize = capacity;

            if (newSize <= 0)
            {
                newSize = 1;
            }

            while (newSize < min)
            {
                newSize = 1 + ((newSize * 10) / 8);
            }

            Point[] replacement = new Point[newSize];

            for (int i = 0; i < count; i++)
            {
                replacement[i] = array[i];
            }

            array = replacement;
            capacity = newSize;
        }

        public Point[] GetPointArray()
        {
            Point[] ret = new Point[count];

            for (int i = 0; i < count; i++)
            {
                ret[i] = array[i];
            }

            return ret;
        }

        /// <summary>
        /// Gets direct access to the array held by the PointVector.
        /// The caller must not modify the array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="length">The actual number of items stored in the array. This number will be less than or equal to array.Length.</param>
        /// <remarks>This method is supplied strictly for performance-critical purposes.</remarks>
        public unsafe void GetPointArrayReadOnly(out Point[] array, out int length)
        {
            array = this.array;
            length = this.count;
        }
    }
}
