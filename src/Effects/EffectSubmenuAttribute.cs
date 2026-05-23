using System;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Allows you to place an effect into a subMenu, which allows logical grouping.
    /// </summary>
    public class EffectSubMenuAttribute
        : Attribute
    {
        private string subMenuName;
        public string SubMenuName
        {
            get
            {
                return subMenuName;
            }
        }

        public EffectSubMenuAttribute(string subMenuName)
        {
            this.subMenuName = subMenuName;
        }
    }
}
