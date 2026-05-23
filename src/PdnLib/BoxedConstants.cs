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
    /// Summary description for BoxedConstants.
    /// </summary>
    public sealed class BoxedConstants
    {
        private static object[] boxedInt32 = new object[1024];
        private static object boxedTrue = (object)true;
        private static object boxedFalse = (object)false;

        public static object GetInt32(int value)
        {
            if (value >= boxedInt32.Length || value < 0)
            {
                return (object)value;
            }

            if (boxedInt32[value] == null)
            {
                boxedInt32[value] = (object)value;
            }

            return boxedInt32[value];
        }

        public static object GetBoolean(bool value)
        {
            return value ? boxedTrue : boxedFalse;
        }

        static BoxedConstants()
        {
        }

        private BoxedConstants()
        {
        }
    }
}
