/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    public struct Quadruple<T, U, V, W>
    {
        private T first;
        private U second;
        private V third;
        private W fourth;

        public T First
        {
            get
            {
                return this.first;
            }
        }

        public U Second
        {
            get
            {
                return this.second;
            }
        }

        public V Third
        {
            get
            {
                return this.third;
            }
        }

        public W Fourth
        {
            get
            {
                return this.fourth;
            }
        }

        public override int GetHashCode()
        {
            return this.first.GetHashCode() ^ this.second.GetHashCode() ^ this.third.GetHashCode() ^ this.fourth.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return ((obj != null) && (obj is Quadruple<T, U, V, W>) && (this == (Quadruple<T, U, V, W>)obj));
        }

        public static bool operator ==(Quadruple<T, U, V, W> lhs, Quadruple<T, U, V, W> rhs)
        {
            return (lhs.First.Equals(rhs.First) && lhs.Second.Equals(rhs.Second) && lhs.Third.Equals(rhs.Third) && lhs.Fourth.Equals(rhs.Fourth));
        }

        public static bool operator !=(Quadruple<T, U, V, W> lhs, Quadruple<T, U, V, W> rhs)
        {
            return !(lhs == rhs);
        }

        public Quadruple(T first, U second, V third, W fourth)
        {
            this.first = first;
            this.second = second;
            this.third = third;
            this.fourth = fourth;
        }
    }
}
