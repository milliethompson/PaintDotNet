/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for PropertiesChangedEventArgs.
    /// </summary>
    public class PropertyEventArgs
        : System.EventArgs
    {
        private string propertyName;
        public string PropertyName
        {
            get
            {
                return propertyName;
            }
        }

        public PropertyEventArgs(string propertyName)
        {
            this.propertyName = propertyName;
        }
    }
}
