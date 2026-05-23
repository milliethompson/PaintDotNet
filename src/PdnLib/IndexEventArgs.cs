/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    /// <summary>
    /// Declares an EventArgs type for an event that needs a single integer, interpreted
    /// as an index, as event information.
    /// </summary>
    public class IndexEventArgs 
        : EventArgs
    {
        int index;

        public int Index
        {
            get
            {
                return index;
            }
        }

        public IndexEventArgs(int i)
        {
            this.index = i;
        }
    }
}
