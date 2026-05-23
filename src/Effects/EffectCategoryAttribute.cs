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
