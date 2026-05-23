/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Re-implements System.Drawing.PropertyItem so that the data is serializable.
    /// </summary>
    [Serializable]
    internal sealed class PropertyItem2
    {
        private int id;
        private int len;
        private short type;
        private byte[] value;

        public int Id
        {
            get
            {
                return id;
            }
        }

        public int Len
        {
            get
            {
                return len;
            }
        }

        public short Type
        {
            get
            {
                return type;
            }
        }

        public byte[] Value
        {
            get
            {
                return (byte[])value.Clone();
            }
        }

        public PropertyItem2(int id, int len, short type, byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            this.id = id;
            this.len = len;
            this.type = type;
            this.value = (byte[])value.Clone();
        }

        public string ToBlob()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);
            byte[] bytes = ms.ToArray();
            string blob = Convert.ToBase64String(bytes);
            return blob;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public PropertyItem ToPropertyItem()
        {
            PropertyItem pi = GetPropertyItem();

            pi.Id = this.Id;
            pi.Len = this.Len;
            pi.Type = this.Type;
            pi.Value = this.Value;

            return pi;
        }

        public static PropertyItem2 FromPropertyItem(PropertyItem pi)
        {
            return new PropertyItem2(pi.Id, pi.Len, pi.Type, pi.Value);
        }

        public static PropertyItem2 FromBlob(string blob)
        {
            byte[] bytes = Convert.FromBase64String(blob);
            MemoryStream ms = new MemoryStream(bytes);
            BinaryFormatter bf = new BinaryFormatter();
            PropertyItem2 pi2 = (PropertyItem2)bf.Deserialize(ms);
            return pi2;
        }

        // System.Drawing.Imaging.PropertyItem does not have a public constructor
        // So, as per the documentation, we have to "steal" one.
        // Quite ridiculous.
        // This depends on PropertyItem.png being an embedded resource in this assembly.
        private static Image propertyItemImage;

        private static PropertyItem GetPropertyItem()
        {
            if (propertyItemImage == null)
            {
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PaintDotNet.SystemLayer.PropertyItem.png");
                propertyItemImage = Image.FromStream(stream);
            }

            PropertyItem pi = propertyItemImage.PropertyItems[0];
            pi.Id = 0;
            pi.Len = 0;
            pi.Type = 0;
            pi.Value = new byte[0];

            return pi;
        }
    }
}
