/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Use this to mark that a particular effect should only use 1 thread
    /// for execution. This is especially important if you want to use GDI+
    /// (that is, System.Drawing) facilities for drawing.
    /// </summary>
    [Obsolete("This attribute has been removed. Please use the appropriate Effect constructor instead.", true)]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SingleThreadedEffectAttribute
        : Attribute
    {
        public SingleThreadedEffectAttribute()
        {
        }
    }
}
