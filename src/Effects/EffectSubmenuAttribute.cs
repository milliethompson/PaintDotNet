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
    /// Allows you to place an effect into a subMenu, which allows logical grouping.
    /// </summary>
    [Obsolete("This attribute was removed because it defeats localization. Use the appropriate Effect constructor instead.", false)]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EffectSubMenuAttribute
        : Attribute
    {
        public EffectSubMenuAttribute(string subMenuName)
        {
        }
    }
}
