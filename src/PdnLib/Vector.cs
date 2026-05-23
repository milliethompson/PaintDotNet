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
using System.Collections.Generic;
using System.Drawing;

namespace PaintDotNet
{
    public class Vector<T>
        : IEnumerable<T>
    {
        // Ensure that these types get ngen'd
        private static Vector<Point> vectorPoint = new Vector<Point>(0);
        private static Vector<PointF> vectorPointF = new Vector<PointF>(0);
        private static Vector<Rectangle> vectorRectangle = new Vector<Rectangle>(0);

        private int count = 0;
        private T[] array;
        
        public Vector()
            : this(10)
        {
        }

        public Vector(int capacity)
        {
            this.array = new T[capacity];
        }

        public Vector(IEnumerable<T> copyMe)
        {
            foreach (T t in copyMe)
            {
                Add(t);
            }
        }

        public void Add(T pt)
        {
            if (count >= array.Length)
            {
                Grow(count + 1);
            }

            array[count++] = pt;
        }

        public void Clear()
        {
            count = 0;
        }

        public T this[int index]
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

        public T Get(int index)
        {
            if (index < 0 || index >= count)
            {
                throw new ArgumentOutOfRangeException("index", index, "0 <= index < count");
            }

            return array[index];
        }

        public unsafe T GetUnchecked(int index)
        {
            return array[index];
        }

        public void Set(int index, T pt)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", index, "0 <= index");
            }

            if (index >= array.Length)
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

        private class VectorEnumerator<ET>
            : IEnumerator<ET>
        {
            private Vector<ET> target;
            private int index;

            public VectorEnumerator(Vector<ET> target)
            {
                this.target = target;
            }

            ~VectorEnumerator()
            {
                Dispose(false);
            }

            public void Reset()
            {
                index = -1;
            }

            object IEnumerator.Current
            {
                get
                {
                    return (object)Current;
                }
            }

            public ET Current
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

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new VectorEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void Grow(int min)
        {
            int newSize = array.Length;

            if (newSize <= 0)
            {
                newSize = 1;
            }

            while (newSize < min)
            {
                newSize = 1 + ((newSize * 10) / 8);
            }

            T[] replacement = new T[newSize];

            for (int i = 0; i < count; i++)
            {
                replacement[i] = array[i];
            }

            array = replacement;
        }

        public T[] GetArray()
        {
            T[] ret = new T[count];

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
        public unsafe void GetArrayReadOnly(out T[] array, out int length)
        {
            array = this.array;
            length = this.count;
        }
    }
}
