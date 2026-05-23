using System;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for NameEventArgs.
    /// </summary>
    public sealed class EnumValueEventArgs
        : System.EventArgs
    {
        private System.Enum enumValue;
        public System.Enum EnumValue
        {
            get
            {
                return enumValue;
            }
        }

        public EnumValueEventArgs(System.Enum enumValue)
        {
            this.enumValue = enumValue;
        }
    }
}
