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
    /// Summary description for IBitVector2D.
    /// </summary>
    public interface IBitVector2D
        : ICloneable
    {
        int Width 
        { 
            get;
        }

        int Height 
        { 
            get; 
        }

        bool this[int x, int y]
        {
            get;
            set;
        }

        bool this[System.Drawing.Point pt]
        {
            get;
            set;
        }

        bool IsEmpty
        {
            get;
        }

        void Clear(bool newValue);
        void Set(int x, int y, bool newValue);
        void Set(Point pt, bool newValue);
        void Set(Rectangle rect, bool newValue);
        void Set(Scanline scan, bool newValue);
        void Set(PdnRegion region, bool newValue);
        void SetUnchecked(int x, int y, bool newValue);
        bool Get(int x, int y);
        bool GetUnchecked(int x, int y);
        void Invert(int x, int y);
        void Invert(Point pt);
        void Invert(Rectangle rect);
        void Invert(Scanline scan);
        void Invert(PdnRegion region);
    }
}
