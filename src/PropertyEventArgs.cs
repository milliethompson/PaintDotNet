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
