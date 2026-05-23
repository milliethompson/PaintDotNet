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
        /// the "Adjustments" submenu in the "Image" menu.
        /// These types of effects are typically quick to execute.
        /// </summary>
        Adjustment
    }
}
