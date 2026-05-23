/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Categories for effects that determine their placement within
    /// Paint.NET's menu hierarchy.
    /// </summary>
    public enum EffectCategory
    {
        /// <summary>
        /// The default category for an effect. This will place effects in to the "Effects" menu.
        /// </summary>
        Effect,

        /// <summary>
        /// Signifies that this effect should be an "Image Adjustment", placing the effect in
        /// the "Adjustments" submenu in the "Layers" menu.
        /// These types of effects are typically quick to execute. They are also preferably 
        /// "unary" (see EffectTypeHint) but are not required to be.
        /// </summary>
        Adjustment
    }
}
