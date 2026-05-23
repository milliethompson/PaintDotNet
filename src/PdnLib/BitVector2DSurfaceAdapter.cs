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
    /// Adapts a Surface class so it can be used as a two dimensional boolean array
    /// </summary>
    public sealed class BitVector2DSurfaceAdapter
        : IBitVector2D
    {
        private Surface surface;

        public BitVector2DSurfaceAdapter(Surface surface)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            this.surface = surface;
        }

        public int Width
        {
            get
            {
                return surface.Width;
            }
        }

        public int Height
        {
            get
            {
                return surface.Height;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (Width == 0) || (Height == 0);
            }
        }

        public void Clear(bool newValue)
        {
            unsafe
            {
                for (int y = 0; y < Height; ++y)
                {
                    ColorBgra *row = surface.GetRowAddress(y);

                    if (newValue)
                    {
                        for (int x = 0; x < Width; ++x)
                        {
                            row->A |= 1; // turn on low bit
                            ++row;
                        }
                    }
                    else
                    {
                        for (int x = 0; x < Width; ++x)
                        {
                            row->A &= 254; // turn off low bit
                            ++row;
                        }
                    }
                }
            }
        }

        public bool Get(int x, int y)
        {
            return this[x, y];
        }

        public unsafe bool GetUnchecked(int x, int y)
        {
            return (0 != (1 & surface.GetPointAddressUnchecked(x, y)->A));
        }

        public void Set(int x, int y, bool newValue)
        {
            this[x, y] = newValue;
        }

        public void Set(Point pt, bool newValue)
        {
            Set(pt.X, pt.Y, newValue);
        }

        public void Set(Rectangle rect, bool newValue)
        {
            for (int y = rect.Top; y < rect.Bottom; ++y)
            {
                for (int x = rect.Left; x < rect.Right; ++x)
                {
                    Set(x, y, newValue);
                }
            }
        }

        public void Set(Scanline scan, bool newValue)
        {
            int x = scan.X;

            while (x < scan.X + scan.Length)
            {
                Set(x, scan.Y, newValue);
                ++x;
            }
        }

        public void Set(PdnRegion region, bool newValue)
        {
            foreach (Rectangle rect in region.GetRegionScansReadOnlyInt())
            {
                Set(rect, newValue);
            }
        }

        public unsafe void SetUnchecked(int x, int y, bool newValue)
        {
            if (newValue)
            {
                surface.GetPointAddressUnchecked(x, y)->A |= 1;
            }
            else
            {
                surface.GetPointAddressUnchecked(x, y)->A &= 254;
            }
        }

        public void Invert(int x, int y)
        {
            Set(x, y, !Get(x, y));
        }

        public void Invert(Point pt)
        {
            Invert(pt.X, pt.Y);
        }

        public void Invert(Rectangle rect)
        {
            for (int y = rect.Top; y < rect.Bottom; ++y)
            {
                for (int x = rect.Left; x < rect.Right; ++x)
                {
                    Invert(x, y);
                }
            }
        }

        public void Invert(Scanline scan)
        {
            int x = scan.X;

            while (x < scan.X + scan.Length)
            {
                Invert(x, scan.Y);
                ++x;
            }
        }

        public void Invert(PdnRegion region)
        {
            foreach (Rectangle rect in region.GetRegionScansReadOnlyInt())
            {
                Invert(rect);
            }        
        }

        public bool this[System.Drawing.Point pt]
        {
            get
            {
                return this[pt.X, pt.Y];
            }

            set
            {
                this[pt.X, pt.Y] = value;
            }
        }

        public bool this[int x, int y]
        {
            get
            {
                return (surface[x, y].A & 1) == 1;
            }

            set
            {
                unsafe
                {
                    ColorBgra *ptr = surface.GetPointAddress(x, y);

                    if (value)
                    {
                        ptr->A |= 1; // turn on low bit
                    }
                    else
                    {
                        ptr->A &= 254; // turn off low bit
                    }
                }
            }
        }

        public BitVector2DSurfaceAdapter Clone()
        {
            Surface clonedSurface = this.surface.Clone();
            return new BitVector2DSurfaceAdapter(clonedSurface);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
