/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace PaintDotNet
{
    public class DataEventArgs<T>
        : EventArgs
    {
        private T data;
        public T Data
        {
            get
            {
                return data;
            }
        }

        public DataEventArgs(T data)
        {
            this.data = data;
        }
    }
}
