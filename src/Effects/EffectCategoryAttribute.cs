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
    /// Allows you to categorize an Effect to place it in the appropriate menu
    /// within Paint.NET.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EffectCategoryAttribute :
        Attribute
    {
        private EffectCategory category;
        public EffectCategory Category
        {
            get
            {
                return category;
            }
        }

        public EffectCategoryAttribute(EffectCategory category)
        {
            this.category = category;
        }
    }
}
